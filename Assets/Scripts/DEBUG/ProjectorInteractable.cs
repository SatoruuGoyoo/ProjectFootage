//using UnityEngine;
//using UnityEngine.Video;

///// <summary>
///// Proyector interactuable.
///// - Jugador se acerca y presiona F → enciende luces, reproduce video, bloquea movimiento, cambia cámara.
///// - Al terminar el video → desbloquea jugador, restaura cámara, activa objeto oculto.
///// - El proyector queda encendido. Volver a presionar F reinicia el video.
///// </summary>
//public class ProjectorInteractable : MonoBehaviour
//{
//    // ── References ────────────────────────────────────────────────────────────

//    [Header("Player")]
//    [Tooltip("Transform del jugador (para medir distancia).")]
//    [SerializeField] private Transform player;

//    [Tooltip("Script de movimiento del jugador. Se deshabilita durante el video.")]
//    [SerializeField] private MonoBehaviour playerMovement;

//    [Tooltip("Script de look/mouse del jugador (opcional). Se deshabilita durante el video.")]
//    [SerializeField] private MonoBehaviour playerLook;

//    // ── Interaction ───────────────────────────────────────────────────────────

//    [Header("Interaction")]
//    [SerializeField] private float interactRadius = 2.5f;
//    [SerializeField] private KeyCode interactKey = KeyCode.F;

//    [Tooltip("UI que muestra el prompt '[F] Ver proyector'. Puede ser null.")]
//    [SerializeField] private GameObject interactPrompt;

//    // ── Projector ─────────────────────────────────────────────────────────────

//    [Header("Projector")]
//    [SerializeField] private VideoPlayer videoPlayer;

//    [Tooltip("Todas las luces del proyector que querés encender al activarlo.")]
//    [SerializeField] private Light[] projectorLights;

//    // ── Cameras ───────────────────────────────────────────────────────────────

//    [Header("Cameras")]
//    [Tooltip("Cámara del jugador (la que se apaga durante el video).")]
//    [SerializeField] private Camera playerCamera;

//    [Tooltip("Cámara cinemática que se activa durante el video.")]
//    [SerializeField] private Camera cinematicCamera;

//    // ── On Video End ──────────────────────────────────────────────────────────

//    [Header("On Video End")]
//    [Tooltip("Objetos que aparecen en escena cuando el video termina por primera vez.")]
//    [SerializeField] private GameObject[] objectsToReveal;

//    // ── Private State ─────────────────────────────────────────────────────────

//    private bool _isWatching = false;   // jugador actualmente viendo el video
//    private bool _projectorOn = false;   // proyector encendido (se mantiene tras primer uso)
//    private bool _revealedAlready = false;  // para revelar el objeto solo la primera vez

//    // ─────────────────────────────────────────────────────────────────────────
//    // Lifecycle
//    // ─────────────────────────────────────────────────────────────────────────

//    private void Awake()
//    {
//        // Estado inicial
//        SetLights(false);

//        if (objectsToReveal != null)
//            foreach (var obj in objectsToReveal)
//                if (obj != null) obj.SetActive(false);

//        if (interactPrompt != null)
//            interactPrompt.SetActive(false);

//        if (cinematicCamera != null)
//            cinematicCamera.gameObject.SetActive(false);

//        // Configurar VideoPlayer
//        if (videoPlayer != null)
//        {
//            videoPlayer.playOnAwake = false;
//            videoPlayer.loopPointReached += OnVideoFinished;
//        }
//    }

//    private void Update()
//    {
//        if (player == null) return;

//        bool inRange = Vector3.Distance(player.position, transform.position) <= interactRadius;

//        // Prompt solo si está en rango y no está viendo el video ahora mismo
//        if (interactPrompt != null)
//            interactPrompt.SetActive(inRange && !_isWatching);

//        // Interacción
//        if (inRange && !_isWatching && Input.GetKeyDown(interactKey))
//            ActivateProjector();
//    }

//    // ─────────────────────────────────────────────────────────────────────────
//    // Projector logic
//    // ─────────────────────────────────────────────────────────────────────────

//    private void ActivateProjector()
//    {
//        _isWatching = true;
//        _projectorOn = true;

//        // Encender luces
//        SetLights(true);

//        // Reiniciar y reproducir video
//        if (videoPlayer != null)
//        {
//            videoPlayer.time = 0;
//            videoPlayer.Play();
//        }

//        // Cambiar cámara
//        SwitchCamera(toCinematic: true);

//        // Bloquear jugador
//        SetPlayerLocked(true);
//    }

//    private void OnVideoFinished(VideoPlayer vp)
//    {
//        _isWatching = false;

//        // Desbloquear jugador
//        SetPlayerLocked(false);

//        // Restaurar cámara del jugador
//        SwitchCamera(toCinematic: false);

//        // Revelar objeto solo la primera vez que termina el video
//        if (!_revealedAlready && objectsToReveal != null)
//        {
//            foreach (var obj in objectsToReveal)
//                if (obj != null) obj.SetActive(true);
//            _revealedAlready = true;
//        }

//        // Las luces y el proyector se quedan encendidos (video pausado al final)
//        // La próxima vez que el jugador presione F, el video se reiniciará desde el principio.
//    }

//    // ─────────────────────────────────────────────────────────────────────────
//    // Helpers
//    // ─────────────────────────────────────────────────────────────────────────

//    private void SetLights(bool on)
//    {
//        foreach (var light in projectorLights)
//            if (light != null) light.enabled = on;
//    }

//    private void SwitchCamera(bool toCinematic)
//    {
//        if (playerCamera != null) playerCamera.gameObject.SetActive(!toCinematic);
//        if (cinematicCamera != null) cinematicCamera.gameObject.SetActive(toCinematic);
//    }

//    private void SetPlayerLocked(bool locked)
//    {
//        // Deshabilitar scripts de movimiento y look
//        if (playerMovement != null) playerMovement.enabled = !locked;
//        if (playerLook != null) playerLook.enabled = !locked;

//        // Cursor — ajustá esto según lo que uses en tu juego normalmente
//        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
//        Cursor.visible = locked;
//    }

//    // ─────────────────────────────────────────────────────────────────────────
//    // Gizmos
//    // ─────────────────────────────────────────────────────────────────────────

//    private void OnDrawGizmosSelected()
//    {
//        Gizmos.color = new Color(1f, 0.9f, 0f, 0.35f);
//        Gizmos.DrawSphere(transform.position, interactRadius);
//        Gizmos.color = new Color(1f, 0.9f, 0f, 0.9f);
//        Gizmos.DrawWireSphere(transform.position, interactRadius);
//    }
//}