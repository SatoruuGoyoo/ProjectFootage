using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class CamcorderPickup : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject camcorderSystem;
    public GameObject camcorderModel;

    [Header("Input")]
    public KeyCode pickupKey = KeyCode.F;

    [Header("Prompt UI")]
    public GameObject promptUI;
    [TextArea] public string pickupPromptText = "PRESIONA F PARA RECOGER";
    [TextArea] public string postPickupPromptText = "PRESIONA TAB PARA ABRIR MENU DE LA CAMARA";
    public float postPickupDisplayDuration = 3f;
    private TextMeshProUGUI promptText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    [Header("Detección")]
    public string playerTag = "Player";

    private bool playerNearby = false;
    private bool pickedUp = false;

    private void Start()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;

        if (camcorderSystem != null)
        {
            camcorderSystem.SetActive(true);
            camcorderSystem.SetActive(false);
        }
        if (camcorderModel != null)
        {
            camcorderModel.SetActive(true);
            camcorderModel.SetActive(false);
        }

        if (promptUI != null)
        {
            promptText = promptUI.GetComponentInChildren<TextMeshProUGUI>();
            if (promptText != null) promptText.text = pickupPromptText;
        }

        SetPrompt(false);
    }

    private void Update()
    {
        if (!playerNearby || pickedUp) return;
        if (Input.GetKeyDown(pickupKey)) Pickup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp || !other.CompareTag(playerTag)) return;
        playerNearby = true;
        if (promptText != null) promptText.text = pickupPromptText;
        SetPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerNearby = false;
        SetPrompt(false);
    }

    private void Pickup()
    {
        pickedUp = true;

        if (audioSource != null && pickupSound != null)
            audioSource.PlayOneShot(pickupSound);

        if (camcorderSystem != null) camcorderSystem.SetActive(true);
        if (camcorderModel != null) camcorderModel.SetActive(true);

        GameEvents.CamcorderPickedUp();

        if (promptText != null) promptText.text = postPickupPromptText;
        SetPrompt(true);


        promptUI.GetComponent<MonoBehaviour>()
                .StartCoroutine(HidePromptAfterDelay());

        gameObject.SetActive(false);
        Debug.Log("[CamcorderPickup] Camcorder recogida.");
    }

    private IEnumerator HidePromptAfterDelay()
    {
        yield return new WaitForSeconds(postPickupDisplayDuration);
        SetPrompt(false);
    }

    private void SetPrompt(bool active)
    {
        if (promptUI != null) promptUI.SetActive(active);
    }
}