using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(1000)]
public class InteractPromptUI : MonoBehaviour
{
    private enum PromptPlacement
    {
        ScreenSlot,
        WorldAnchor,
    }

    [Header("Root")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private RectTransform promptRect;

    [Header("Placement")]
    [SerializeField] private PromptPlacement placement = PromptPlacement.WorldAnchor;

    [Header("Screen Slot Mode")]
    [SerializeField] private UIPositioner positioner;
    [SerializeField] private UIPositioner.ScreenPosition defaultPosition = UIPositioner.ScreenPosition.LowerRight;

    [Header("Distance Falloff (World Anchor Mode)")]
    [Tooltip("Dejar vacío para buscar por tag Player.")]
    [SerializeField] private Transform player;
    [Tooltip("A esta distancia o menos el badge está al 100%.")]
    [SerializeField] private float fullDistance = 1.4f;
    [Tooltip("Más lejos que esto el badge no se muestra.")]
    [SerializeField] private float fadeDistance = 4f;
    [Range(0.1f, 1f)][SerializeField] private float minScale = 0.55f;
    [Range(0f, 1f)][SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float blendSpeed = 6f;

    [Header("World Anchor Mode")]
    [Tooltip("Dejar vacío para usar CameraManager.ActiveCamera.")]
    [SerializeField] private Camera promptCamera;
    [SerializeField] private Vector2 screenPadding = new Vector2(48f, 48f);
    [SerializeField] private bool clampToScreen = true;

    [Header("Interact Icon")]
    [SerializeField] private Image interactIcon;
    [SerializeField] private Sprite defaultInteractSprite;
    [SerializeField] private Sprite defaultCancelSprite;

    [Header("Key Badge — assign sprites OR leave empty to use text")]
    [SerializeField] private Image keyImage;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private Sprite interactKeySprite;
    [SerializeField] private Sprite cancelKeySprite;
    [SerializeField] private string interactKeyText = "[E]";
    [SerializeField] private string cancelKeyText = "[F]";

    private bool _isVisible;
    private bool _hasPrompt;
    private bool _inRange;
    private bool _onScreen = true;
    private Transform _anchor;
    private Vector3 _offset;
    private Camera _cameraOverride;
    private float _strength;
    private bool _playerSearched;

    private void Awake()
    {
        if (promptRect == null && container != null) promptRect = container.transform as RectTransform;
        if (promptRect != null && placement == PromptPlacement.ScreenSlot) promptRect.localScale = Vector3.one;
        _isVisible = true;
        RefreshKeyBadgeMode();
        SetVisible(false);
    }

    private void OnEnable()
    {
        GameEvents.OnInteractPromptShown += OnShown;
        GameEvents.OnInteractPromptHidden += OnHidden;
        UILayerManager.OnModalChanged += OnModalChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnInteractPromptShown -= OnShown;
        GameEvents.OnInteractPromptHidden -= OnHidden;
        UILayerManager.OnModalChanged -= OnModalChanged;
    }

    private void OnShown(InteractPrompt prompt)
    {
        _hasPrompt = true;
        _inRange = prompt.InRange;
        _anchor = prompt.Anchor;
        _offset = prompt.Offset;

        if (placement == PromptPlacement.ScreenSlot)
        {
            positioner?.SetPosition(defaultPosition);
            if (promptRect != null) promptRect.localScale = Vector3.one;
            _onScreen = true;
        }

        if (interactIcon != null)
            interactIcon.sprite = prompt.Icon != null ? prompt.Icon : DefaultIconFor(prompt.Key);

        SetKeyBadge(prompt.Key);

        if (placement == PromptPlacement.WorldAnchor) UpdateWorldPosition();
        else ApplyVisibility();
    }

    private void OnHidden()
    {
        _hasPrompt = false;
        _inRange = false;
        _anchor = null;
        ApplyVisibility();
    }

    private void OnModalChanged(bool modalOpen) => ApplyVisibility();

    private void LateUpdate()
    {
        if (placement != PromptPlacement.WorldAnchor) return;
        if (!_hasPrompt) return;
        UpdateWorldPosition();
        if (_isVisible) ApplyFalloff();
    }

    private void UpdateWorldPosition()
    {
        if (promptRect == null || _anchor == null)
        {
            _onScreen = false;
            ApplyVisibility();
            return;
        }

        Camera cam = ResolveCamera();
        if (cam == null)
        {
            _onScreen = false;
            ApplyVisibility();
            return;
        }

        Vector3 screenPoint = cam.WorldToScreenPoint(_anchor.position + _offset);

        if (screenPoint.z <= 0f)
        {
            _onScreen = false;
            ApplyVisibility();
            return;
        }

        if (clampToScreen)
        {
            screenPoint.x = Mathf.Clamp(screenPoint.x, screenPadding.x, Screen.width - screenPadding.x);
            screenPoint.y = Mathf.Clamp(screenPoint.y, screenPadding.y, Screen.height - screenPadding.y);
        }
        else if (screenPoint.x < 0f || screenPoint.x > Screen.width || screenPoint.y < 0f || screenPoint.y > Screen.height)
        {
            _onScreen = false;
            ApplyVisibility();
            return;
        }

        screenPoint.z = 0f;
        promptRect.position = screenPoint;

        _onScreen = true;
        ApplyVisibility();
    }

    public void SetCameraOverride(Camera camera) => _cameraOverride = camera;

    public void ClearCameraOverride() => _cameraOverride = null;

    private Camera ResolveCamera()
    {
        if (_cameraOverride != null && _cameraOverride.isActiveAndEnabled) return _cameraOverride;
        if (promptCamera != null && promptCamera.isActiveAndEnabled) return promptCamera;

        Camera active = CameraManager.Instance != null ? CameraManager.Instance.ActiveCamera : null;
        if (active != null && active.isActiveAndEnabled) return active;

        return Camera.main;
    }

    private void ApplyVisibility()
    {
        bool allowed = placement == PromptPlacement.WorldAnchor || _inRange;
        SetVisible(_hasPrompt && allowed && _onScreen && !UILayerManager.IsModalOpen);
    }

    private void ApplyFalloff()
    {
        float target = TargetStrength();
        _strength = Mathf.MoveTowards(_strength, target, blendSpeed * Time.deltaTime);

        if (promptRect != null)
            promptRect.localScale = Vector3.one * Mathf.Lerp(minScale, 1f, _strength);

        if (container != null)
            container.alpha = Mathf.Lerp(minAlpha, 1f, _strength);
    }

    private float TargetStrength()
    {
        if (_anchor == null) return 0f;

        ResolvePlayer();
        if (player == null) return 1f;

        float distance = Vector3.Distance(player.position, _anchor.position + _offset);
        if (fadeDistance <= fullDistance) return 1f;

        return Mathf.InverseLerp(fadeDistance, fullDistance, distance);
    }

    private void ResolvePlayer()
    {
        if (player != null || _playerSearched) return;
        _playerSearched = true;
        var found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;
    }

    private Sprite DefaultIconFor(InteractPromptKey key) =>
        key == InteractPromptKey.Cancel ? defaultCancelSprite : defaultInteractSprite;

    private void SetKeyBadge(InteractPromptKey key)
    {
        bool cancel = key == InteractPromptKey.Cancel;

        if (keyImage != null)
            keyImage.sprite = cancel ? cancelKeySprite : interactKeySprite;

        if (keyLabel != null)
            keyLabel.SetText(cancel ? cancelKeyText : interactKeyText);
    }

    private void SetVisible(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;

        if (!visible) _strength = 0f;

        if (container == null) return;
        container.interactable = visible;
        container.blocksRaycasts = visible;

        if (!visible) container.alpha = 0f;
        else if (placement == PromptPlacement.ScreenSlot) container.alpha = 1f;
        else container.alpha = Mathf.Lerp(minAlpha, 1f, _strength);
    }

    [ContextMenu("Refresh Key Badge Mode")]
    private void RefreshKeyBadgeMode()
    {
        bool useSprite = interactKeySprite != null || cancelKeySprite != null;
        if (keyImage != null) keyImage.gameObject.SetActive(useSprite);
        if (keyLabel != null) keyLabel.gameObject.SetActive(!useSprite);
    }
}