using UnityEngine;

/// <summary>
/// Prop de la camcorder en el suelo.
/// Al recogerla: desactiva este GO y activa el sistema de camcorder ya existente.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CamcorderPickup : MonoBehaviour
{
    [Header("Referencia al sistema de camcorder")]
    [Tooltip("El GameObject raíz de tu camcorder ya existente (el que tiene CorderVisual, etc.)")]
    public GameObject camcorderSystem;

    [Tooltip("La linterna dentro del sistema de camcorder. Empieza desactivada.")]
    public GameObject flashlight;

    [Header("Input")]
    public KeyCode pickupKey = KeyCode.F;

    [Header("Prompt UI")]
    [Tooltip("UI hint que aparece al estar cerca (ej: 'F: Recoger camcorder')")]
    public GameObject promptUI;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    [Header("Detección")]
    public string playerTag = "Player";

    // ── Estado ──────────────────────────────────────────────────────
    private bool playerNearby = false;
    private bool pickedUp = false;

    // ───────────────────────────────────────────────────────────────
    private void Start()
    {
        // Asegura trigger
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;

        // El sistema de camcorder empieza desactivado
        if (camcorderSystem != null)
            camcorderSystem.SetActive(false);

        // La linterna también (por si el camcorderSystem arranca activo en otra escena)
        if (flashlight != null)
            flashlight.SetActive(false);

        SetPrompt(false);
    }

    private void Update()
    {
        if (!playerNearby || pickedUp) return;

        if (Input.GetKeyDown(pickupKey))
            Pickup();
    }

    // ── Trigger ─────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp || !other.CompareTag(playerTag)) return;
        playerNearby = true;
        SetPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerNearby = false;
        SetPrompt(false);
    }

    // ── Pickup ───────────────────────────────────────────────────────

    private void Pickup()
    {
        pickedUp = true;
        SetPrompt(false);

        // Sonido
        if (audioSource != null && pickupSound != null)
            audioSource.PlayOneShot(pickupSound);

        // Activa el sistema existente (linterna sigue apagada dentro de él)
        if (camcorderSystem != null)
            camcorderSystem.SetActive(true);

        // Oculta el prop del suelo
        // Usamos SetActive false en vez de Destroy para no romper referencias
        gameObject.SetActive(false);

        Debug.Log("[CamcorderPickup] Camcorder recogida.");
    }

    // ── Helper ───────────────────────────────────────────────────────

    private void SetPrompt(bool active)
    {
        if (promptUI != null) promptUI.SetActive(active);
    }
}