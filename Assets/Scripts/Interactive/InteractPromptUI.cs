using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows a minimal interact hint (icon + key) when the player is near an interactable.
/// No action text — just the visual cue that something is interactable.
///
/// HIERARCHY EXAMPLE:
/// InteractPromptUI (this script + CanvasGroup)
///  └─ Container
///      ├─ InteractIcon   (Image — eye/hand sprite)
///      └─ KeyBadge
///          ├─ KeyImage   (Image — optional key sprite, assign if you have one)
///          └─ KeyLabel   (TMP_Text — fallback "E" text, shown when KeyImage has no sprite)
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup container;

    [Header("Interact Icon")]
    [SerializeField] private Image interactIcon;

    [Header("Key Badge — assign sprite OR leave empty to use text")]
    [SerializeField] private Image keyImage;   // sprite of the key (optional)
    [SerializeField] private TMP_Text keyLabel;   // text fallback (e.g. "E")

    [Header("Feedback")]
    [SerializeField] private float feedbackDuration = 3f;

    // ── internal ──────────────────────────────────────────────────────────────
    private bool _isVisible;
    private bool _hasPrompt;          // true while inside interact range
    private float _feedbackTimer;

    // ── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Force hidden on start — bypass the _isVisible equality check.
        if (container != null)
        {
            container.alpha = 0f;
            container.interactable = false;
            container.blocksRaycasts = false;
        }
        _isVisible = false;

        // Pick key display mode: sprite wins over text when both are assigned.
        RefreshKeyBadgeMode();
    }

    private void OnEnable()
    {
        GameEvents.OnInteractPromptChanged += OnPromptChanged;
        GameEvents.OnFeedbackMessage += OnFeedback;
    }

    private void OnDisable()
    {
        GameEvents.OnInteractPromptChanged -= OnPromptChanged;
        GameEvents.OnFeedbackMessage -= OnFeedback;
    }

    private void Update()
    {
        if (_feedbackTimer <= 0f) return;

        _feedbackTimer -= Time.deltaTime;

        // Feedback expired → return to normal interact visibility
        if (_feedbackTimer <= 0f)
            SetVisible(_hasPrompt);
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by the interaction detector whenever the target interactable changes.
    /// Pass an empty/null string when nothing is in range.
    /// The *content* of the prompt string is intentionally ignored here;
    /// we only care whether there IS one.
    /// </summary>
    private void OnPromptChanged(string prompt)
    {
        _hasPrompt = !string.IsNullOrEmpty(prompt);

        // Don't override an active feedback message
        if (_feedbackTimer <= 0f)
            SetVisible(_hasPrompt);
    }

    /// <summary>
    /// Called by GameEvents when a feedback message should be shown (e.g. "Door is locked").
    /// We briefly hide the interact hint while the feedback is on screen.
    /// The feedback message itself is displayed by a separate FeedbackUI component.
    /// </summary>
    private void OnFeedback(string message)
    {
        _feedbackTimer = feedbackDuration;
        SetVisible(false); // hide hint while feedback plays
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetVisible(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;

        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }

    /// <summary>
    /// If a key sprite is assigned use the Image; otherwise fall back to TMP text.
    /// Call this from Awake (and optionally from editor tooling via [ContextMenu]).
    /// </summary>
    [ContextMenu("Refresh Key Badge Mode")]
    private void RefreshKeyBadgeMode()
    {
        bool useSprite = keyImage != null && keyImage.sprite != null;

        if (keyImage != null) keyImage.gameObject.SetActive(useSprite);
        if (keyLabel != null) keyLabel.gameObject.SetActive(!useSprite);
    }
}