using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class Door : Interactable
{
    [Header("Lock")]
    [SerializeField] private string requiredItemId;
    [SerializeField] private bool startLocked = false;
    [SerializeField] private bool playerCanToggle = true;
    [SerializeField] private bool canClose = true;
    [SerializeField] private string lockedFeedback = "";
    [SerializeField] private float lockedFeedbackDuration = 3f;

    [Header("Confirmation (shown once when player has the item)")]
    [TextArea(2, 5)]
    [SerializeField] private string confirmationText = "";

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    [Header("Interaction")]
    [SerializeField] private float interactCooldown = 0.5f;

    [Header("Motion")]
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform door;
    [SerializeField] private Transform audioOrigin;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 120f;

    [Header("Swing Clearance")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform leafEdge;
    [SerializeField] private float leafWidth = 0f;
    [SerializeField] private float playerRadius = 0.35f;
    [SerializeField] private bool openAwayFromPlayer = true;
    [SerializeField] private bool blockWhenPlayerInArc = true;
    [SerializeField] private bool holdWhilePlayerInArc = true;
    [SerializeField] private string blockedFeedback = "";
    [SerializeField] private float blockedFeedbackDuration = 2f;

    [Header("Audio")]
    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;
    [SerializeField] private EventReference lockedSound;
    [SerializeField] private EventReference blockedSound;

    [Header("Events")]
    public UnityEvent OnFirstInteract;
    public UnityEvent OnFirstLockedInteract;
    public UnityEvent OnLockedInteract;
    public UnityEvent OnUnlockedWithItem;
    public UnityEvent OnBlockedByPlayer;

    private bool isOpen;
    private bool manualLock;
    private bool _itemUsed;
    private bool _pendingConfirm;
    private bool _interactedOnce;
    private bool _lockedInteractedOnce;
    private bool _lockedFeedbackActive;
    private float _interactCooldownTimer;

    private Quaternion _closedLocal;
    private Quaternion _doorLocalToPivot;
    private Vector3 _doorOffsetLocal;
    private Vector3 _leafLocal = Vector3.forward;
    private float _resolvedWidth = 1f;
    private float _swingAngle;
    private float _currentAngle;
    private bool _playerSearched;

    private bool IsLocked => manualLock
        || (!string.IsNullOrEmpty(requiredItemId)
            && !_itemUsed
            && (ItemRegistry.Instance == null || !ItemRegistry.Instance.Has(requiredItemId)));

    private bool NeedsConfirmation => !string.IsNullOrEmpty(requiredItemId)
        && !_itemUsed
        && !manualLock
        && ItemRegistry.Instance != null
        && ItemRegistry.Instance.Has(requiredItemId);

    private bool CanToggle => playerCanToggle && (!isOpen || canClose) && SwingIsClear;

    private bool SwingIsClear
    {
        get
        {
            if (!blockWhenPlayerInArc) return true;
            if (isOpen) return !PlayerInArc(_currentAngle, 0f);

            float swing = ResolveSwingAngle();
            return !PlayerInArc(0f, swing) || !PlayerInArc(0f, -swing);
        }
    }

    public override string PromptMessage => (CanToggle || IsLocked || NeedsConfirmation) ? "door" : "";
    public override bool CanInteract => _interactCooldownTimer <= 0f && (IsLocked || NeedsConfirmation || CanToggle);
    public override bool BlockMovement => false;

    private Vector3 AudioPosition => audioOrigin != null
        ? audioOrigin.position
        : (pivot != null ? pivot.position : transform.position);

    private Quaternion PivotParentRotation => pivot.parent != null ? pivot.parent.rotation : Quaternion.identity;
    private Quaternion ClosedWorldRotation => PivotParentRotation * _closedLocal;

    private void Awake()
    {
        if (pivot == null) pivot = transform;

        manualLock = startLocked;
        isOpen = startOpen;
        _swingAngle = openAngle;

        CaptureClosedRotations();

        _currentAngle = 0f;
        ApplyAngle();

        CaptureLeafGeometry();

        if (startOpen)
        {
            _currentAngle = openAngle;
            ApplyAngle();
        }

        ResolvePlayer();
    }

    private void CaptureClosedRotations()
    {
        Quaternion pivotClosedWorld = pivot.rotation;
        Quaternion doorClosedWorld = door != null ? door.rotation : Quaternion.identity;
        Vector3 doorClosedOffset = door != null ? door.position - pivot.position : Vector3.zero;

        if (startOpen)
        {
            Quaternion unswing = Quaternion.AngleAxis(-openAngle, Vector3.up);
            pivotClosedWorld = unswing * pivotClosedWorld;
            doorClosedWorld = unswing * doorClosedWorld;
            doorClosedOffset = unswing * doorClosedOffset;
        }

        _closedLocal = Quaternion.Inverse(PivotParentRotation) * pivotClosedWorld;

        if (door == null) return;

        Quaternion inverseClosed = Quaternion.Inverse(pivotClosedWorld);
        _doorLocalToPivot = inverseClosed * doorClosedWorld;
        _doorOffsetLocal = inverseClosed * doorClosedOffset;
    }

    private void CaptureLeafGeometry()
    {
        Vector3 worldDir = MeasureLeafDirection(out float measuredWidth);

        _resolvedWidth = leafWidth > 0f ? leafWidth : measuredWidth;
        if (_resolvedWidth <= 0f) _resolvedWidth = 1f;

        if (worldDir.sqrMagnitude < 1e-6f) return;
        _leafLocal = Quaternion.Inverse(ClosedWorldRotation) * worldDir.normalized;
    }

    private Vector3 MeasureLeafDirection(out float width)
    {
        width = 0f;
        Transform origin = pivot != null ? pivot : transform;

        if (leafEdge != null)
        {
            Vector3 toEdge = leafEdge.position - origin.position;
            toEdge.y = 0f;
            width = toEdge.magnitude;
            return toEdge;
        }

        Transform source = door != null ? door : origin;
        var meshRenderer = source.GetComponentInChildren<Renderer>();
        if (meshRenderer == null) return Vector3.zero;

        Vector3 toCenter = meshRenderer.bounds.center - origin.position;
        toCenter.y = 0f;
        width = toCenter.magnitude * 2f;
        return toCenter;
    }

    private Vector3 LeafDirection()
    {
        Vector3 dir = ClosedWorldRotation * _leafLocal;
        dir.y = 0f;
        return dir.sqrMagnitude < 1e-6f ? Vector3.forward : dir.normalized;
    }

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;
    private void OnDisable() => GameEvents.OnConfirmationClosed -= OnConfirmationClosed;

    public override void Interact()
    {
        if (_interactCooldownTimer > 0f) return;

        if (!_interactedOnce)
        {
            _interactedOnce = true;
            OnFirstInteract?.Invoke();
        }

        if (IsLocked)
        {
            if (_lockedFeedbackActive) return;
            _interactCooldownTimer = interactCooldown;
            if (!lockedSound.IsNull) RuntimeManager.PlayOneShot(lockedSound, AudioPosition);
            StartCoroutine(LockedFeedbackRoutine());
            return;
        }

        if (NeedsConfirmation)
        {
            if (_pendingConfirm) return;
            _pendingConfirm = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined, uiPosition);
            return;
        }

        if (!CanToggle) return;

        if (isOpen) RequestClose();
        else RequestOpen();
    }

    private void RequestOpen()
    {
        float swing = ResolveSwingAngle();

        if (blockWhenPlayerInArc && PlayerInArc(0f, swing))
        {
            float flipped = -swing;
            if (!PlayerInArc(0f, flipped))
            {
                swing = flipped;
            }
            else
            {
                ReportBlocked();
                return;
            }
        }

        _swingAngle = swing;
        _interactCooldownTimer = interactCooldown;
        Open();
    }

    private void RequestClose()
    {
        if (blockWhenPlayerInArc && PlayerInArc(_currentAngle, 0f))
        {
            ReportBlocked();
            return;
        }

        _interactCooldownTimer = interactCooldown;
        Close();
    }

    private void ReportBlocked()
    {
        _interactCooldownTimer = interactCooldown;
        if (!blockedSound.IsNull) RuntimeManager.PlayOneShot(blockedSound, AudioPosition);
        if (!string.IsNullOrEmpty(blockedFeedback))
            GameEvents.FeedbackMessage(blockedFeedback, uiPosition, blockedFeedbackDuration);
        OnBlockedByPlayer?.Invoke();
    }

    private float ResolveSwingAngle()
    {
        if (!openAwayFromPlayer) return openAngle;
        if (!TryGetPlayerOffset(out Vector3 toPlayer, out float distance)) return openAngle;
        if (distance < 0.01f) return openAngle;

        float playerSide = Vector3.SignedAngle(LeafDirection(), toPlayer, Vector3.up);
        if (Mathf.Abs(playerSide) < 1f || Mathf.Abs(playerSide) > 179f) return openAngle;

        return playerSide > 0f ? -Mathf.Abs(openAngle) : Mathf.Abs(openAngle);
    }

    private bool PlayerInArc(float fromAngle, float toAngle)
    {
        if (!TryGetPlayerOffset(out Vector3 toPlayer, out float distance)) return false;
        if (distance > _resolvedWidth + playerRadius) return false;
        if (distance < 0.01f) return true;

        float playerAngle = Vector3.SignedAngle(LeafDirection(), toPlayer, Vector3.up);
        float pad = Mathf.Atan2(playerRadius, distance) * Mathf.Rad2Deg;

        float min = Mathf.Min(fromAngle, toAngle) - pad;
        float max = Mathf.Max(fromAngle, toAngle) + pad;

        return playerAngle > min && playerAngle < max;
    }

    private bool TryGetPlayerOffset(out Vector3 offset, out float distance)
    {
        offset = Vector3.zero;
        distance = 0f;

        if (player == null) ResolvePlayer();
        if (player == null) return false;

        offset = player.position - pivot.position;
        offset.y = 0f;
        distance = offset.magnitude;
        return true;
    }

    private void ResolvePlayer()
    {
        if (player != null || _playerSearched) return;
        _playerSearched = true;
        var found = GameObject.FindGameObjectWithTag("Player");
        if (found != null) player = found.transform;
    }

    private IEnumerator LockedFeedbackRoutine()
    {
        _lockedFeedbackActive = true;
        GameEvents.FeedbackMessage(lockedFeedback, uiPosition, lockedFeedbackDuration);

        yield return new WaitForSeconds(lockedFeedbackDuration);

        OnLockedInteract?.Invoke();
        if (!_lockedInteractedOnce)
        {
            _lockedInteractedOnce = true;
            OnFirstLockedInteract?.Invoke();
        }

        _lockedFeedbackActive = false;
    }

    private void OnConfirmed()
    {
        _pendingConfirm = false;
        _itemUsed = true;
        _interactCooldownTimer = interactCooldown;
        OnUnlockedWithItem?.Invoke();
        _swingAngle = ResolveSwingAngle();
        Open();
    }

    private void OnDeclined() => _pendingConfirm = false;

    private void OnConfirmationClosed() => _pendingConfirm = false;

    public void Toggle() { if (isOpen) Close(); else Open(); }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        if (!openSound.IsNull) RuntimeManager.PlayOneShot(openSound, AudioPosition);
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        if (!closeSound.IsNull) RuntimeManager.PlayOneShot(closeSound, AudioPosition);
    }

    public void Lock() => manualLock = true;
    public void Unlock() => manualLock = false;

    private void Update()
    {
        if (_interactCooldownTimer > 0f) _interactCooldownTimer -= Time.deltaTime;

        float target = isOpen ? _swingAngle : 0f;
        if (Mathf.Approximately(_currentAngle, target)) return;

        float next = Mathf.MoveTowards(_currentAngle, target, speed * Time.deltaTime);

        if (holdWhilePlayerInArc && PlayerInArc(_currentAngle, next)) return;

        _currentAngle = next;
        ApplyAngle();
    }

    private void ApplyAngle()
    {
        Quaternion swing = Quaternion.AngleAxis(_currentAngle, Vector3.up);
        Quaternion pivotNow = swing * ClosedWorldRotation;

        pivot.rotation = pivotNow;

        if (door == null) return;
        door.rotation = pivotNow * _doorLocalToPivot;
        door.position = pivot.position + pivotNow * _doorOffsetLocal;
    }

    private void OnDrawGizmosSelected()
    {
        Transform p = pivot != null ? pivot : transform;

        Vector3 leaf;
        float width;

        if (Application.isPlaying)
        {
            leaf = LeafDirection();
            width = _resolvedWidth;
        }
        else
        {
            leaf = MeasureLeafDirection(out width);
            leaf.y = 0f;
            if (leaf.sqrMagnitude < 1e-6f) return;
            leaf.Normalize();
            if (leafWidth > 0f) width = leafWidth;
            if (width <= 0f) width = 1f;
        }

        Vector3 origin = p.position;

        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
        Gizmos.DrawLine(origin, origin + leaf * width);

        DrawArcGizmo(origin, leaf, width, openAngle, new Color(0.2f, 1f, 0.4f, 0.6f));
        DrawArcGizmo(origin, leaf, width, -openAngle, new Color(1f, 0.3f, 0.3f, 0.6f));
    }

    private void DrawArcGizmo(Vector3 origin, Vector3 leaf, float width, float angle, Color color)
    {
        Gizmos.color = color;
        const int steps = 12;
        Vector3 prev = origin + leaf * width;
        for (int i = 1; i <= steps; i++)
        {
            float a = angle * i / steps;
            Vector3 point = origin + (Quaternion.AngleAxis(a, Vector3.up) * leaf) * width;
            Gizmos.DrawLine(prev, point);
            prev = point;
        }
        Gizmos.DrawLine(origin, prev);
    }
}