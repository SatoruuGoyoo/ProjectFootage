using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteractable : MonoBehaviour
{
    [Header("Identificación")]
    [Range(0, 4)]
    public int doorIndex = 0;

    [Header("Input")]
    public KeyCode knockKey = KeyCode.E;
    public KeyCode openKey = KeyCode.F;

    [Header("Detección de proximidad")]
    public string playerTag = "Player";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip knockSound;
    public AudioClip openSound;

    [Header("Prompt UI")]
    public GameObject promptUI;

    private bool playerNearby = false;

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

    // Al cambiar iteración el jugador se teletransporta y OnTriggerExit
    // nunca se dispara — reseteamos la puerta manualmente
    private void OnIterationChanged(int iteration)
    {
        playerNearby = false;
        SetPrompt(false);
    }

    private void Update()
    {
        if (!playerNearby) return;
        if (Input.GetKeyDown(knockKey)) Knock();
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
        PlaySound(knockSound);
        PuzzleManager.Instance?.OnDoorKnocked(doorIndex);
        Debug.Log($"[DoorInteractable] Knock — puerta {doorIndex}.");
    }

    public void Open()
    {
        PlaySound(openSound);
        PuzzleManager.Instance?.OnDoorOpened(doorIndex);
        Debug.Log($"[DoorInteractable] Open — puerta {doorIndex}.");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void SetPrompt(bool active)
    {
        if (promptUI != null) promptUI.SetActive(active);
    }
}