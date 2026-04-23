using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteractable : MonoBehaviour
{
    [Header("Identification")]
    [Range(0, 4)]
    public int doorIndex = 0;

    [Header("Input")]
    public KeyCode knockKey = KeyCode.E;
    public KeyCode openKey = KeyCode.F;

    [Header("Proximity")]
    public string playerTag = "Player";

    [Header("FMOD")]
    [SerializeField] private EventReference knockEvent;
    [SerializeField] private EventReference openEvent;

    [Header("Prompt UI")]
    public GameObject promptUI;

    private bool playerNearby = false;
    private float knockCooldown = 0.5f; // Cooldown to prevent spamming
    [SerializeField] private float knockCoolDuration = 0.5f;


    private void Start()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;
        SetPrompt(false);
        GameEvents.OnIterationChanged += OnIterationChanged;
    }

    private void OnDestroy()
    {
        GameEvents.OnIterationChanged -= OnIterationChanged;
    }

    // Teleport doesn't trigger OnTriggerExit — reset manually on iteration change
    private void OnIterationChanged(int iteration)
    {
        playerNearby = false;
        SetPrompt(false);

    }

    private void Update()
    {
        if (knockCooldown > 0f) knockCooldown -= Time.deltaTime;

        if (!playerNearby) return;
        if (PuzzleManager.Instance == null) return;

        var transition = PuzzleManager.Instance.camcorderTransition;
        if (transition != null && transition.IsTransitioning) return;

        if (Input.GetKeyDown(knockKey) && knockCooldown <= 0f)
        {
            Knock();
            knockCooldown = knockCoolDuration;
        }

        if (Input.GetKeyDown(openKey)) Open();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerNearby = true;
        SetPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerNearby = false;
        SetPrompt(false);
    }

    public void Knock()
    {
        FMODManager.Instance.PlayOneShot(knockEvent, transform.position);
        PuzzleManager.Instance?.OnDoorKnocked(doorIndex);
    }

    public void Open()
    {
        FMODManager.Instance.PlayOneShot(openEvent, transform.position);
        PuzzleManager.Instance?.OnDoorOpened(doorIndex);
    }

    private void SetPrompt(bool active)
    {
        if (promptUI != null) promptUI.SetActive(active);
    }
}