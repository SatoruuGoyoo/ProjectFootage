using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class PushInteractable : Interactable
{
    [System.Serializable]
    public class PushSpot
    {
        [Tooltip("Dónde se para el jugador. Su forward define hacia dónde queda mirando.")]
        public Transform anchor;

        [Tooltip("Desde qué desvío angular respecto de este puesto se permite interactuar.")]
        [Range(10f, 180f)]
        public float maxApproachAngle = 60f;
    }

    [Header("Prompt")]
    [SerializeField] private string pushPrompt = "empujar";

    [Header("Puestos de empuje")]
    [SerializeField] private PushSpot[] spots = new PushSpot[1];
    [SerializeField] private float snapDuration = 0.35f;
    [Tooltip("Rota al jugador para que mire al mueble, ignorando el forward del anchor.")]
    [SerializeField] private bool lookAtPushedObject = true;
    [Tooltip("Si lookAtPushedObject está apagado, usa el forward del anchor.")]
    [SerializeField] private bool matchAnchorRotation = true;
    [Tooltip("Corrección en grados, por si la malla del jugador está rotada respecto de la raíz.")]
    [SerializeField] private float rotationOffset = 0f;

    [Header("Push")]
    [SerializeField] private float pushDuration = 2f;
    [SerializeField] private bool oneTimeOnly = true;

    [Header("Objeto empujado")]
    [SerializeField] private Transform pushedObject;
    [SerializeField] private Vector3 pushOffset = new Vector3(0f, 0f, 1f);
    [SerializeField] private bool offsetIsLocal = true;
    [SerializeField] private AnimationCurve pushCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("El jugador acompaña al mueble con el mismo desplazamiento.")]
    [SerializeField] private bool playerFollowsPush = true;

    [Header("Referencias")]
    [SerializeField] private PlayerAnimator playerAnimator;

    [Header("Audio")]
    [SerializeField] private EventReference pushSound;

    [Header("Events")]
    public UnityEvent OnPushStarted;
    public UnityEvent OnPushFinished;

    private bool _pushing;
    private bool _used;
    private bool _playerSearched;
    private Coroutine _routine;
    private CharacterController _controller;

    public override string PromptMessage => _pushing ? "" : pushPrompt;
    public override bool CanInteract => !_used && !_pushing && FindSpot() != null;
    public override bool IsActive => _pushing;
    public override bool BlockMovement => true;

    private Vector3 PushOrigin => pushedObject != null ? pushedObject.position : transform.position;

    public override void Interact()
    {
        if (_used || _pushing) return;

        PushSpot spot = FindSpot();
        if (spot == null) return;

        _routine = StartCoroutine(PushRoutine(spot));
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
        if (_pushing) Finish();
    }

    private PushSpot FindSpot()
    {
        if (spots == null || spots.Length == 0) return null;
        if (!ResolvePlayer()) return null;

        Vector3 origin = PushOrigin;
        Vector3 toPlayer = Flatten(_controller.transform.position - origin);
        if (toPlayer.sqrMagnitude < 1e-4f) return FirstValidSpot();

        PushSpot best = null;
        float bestAngle = float.MaxValue;

        foreach (var spot in spots)
        {
            if (spot == null || spot.anchor == null) continue;

            Vector3 toAnchor = Flatten(spot.anchor.position - origin);
            if (toAnchor.sqrMagnitude < 1e-4f) continue;

            float angle = Vector3.Angle(toAnchor, toPlayer);
            if (angle > spot.maxApproachAngle) continue;

            if (angle < bestAngle)
            {
                bestAngle = angle;
                best = spot;
            }
        }

        return best;
    }

    private PushSpot FirstValidSpot()
    {
        foreach (var spot in spots)
            if (spot != null && spot.anchor != null) return spot;
        return null;
    }

    private static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        return v;
    }

    private IEnumerator PushRoutine(PushSpot spot)
    {
        _pushing = true;
        EnterInteractionMode();

        yield return MovePlayerToAnchor(spot.anchor);

        playerAnimator.TriggerPush();

        if (!pushSound.IsNull)
            RuntimeManager.PlayOneShot(pushSound, transform.position);

        OnPushStarted?.Invoke();

        yield return MovePushedObject();

        _routine = null;
        Finish();
        OnPushFinished?.Invoke();
    }

    private IEnumerator MovePlayerToAnchor(Transform anchor)
    {
        if (anchor == null) yield break;

        Transform player = _controller.transform;
        Vector3 startPos = player.position;
        Quaternion startRot = player.rotation;
        Vector3 targetPos = anchor.position;
        Quaternion targetRot = ResolveAnchorRotation(anchor, targetPos, startRot);

        _controller.enabled = false;

        if (snapDuration <= 0f)
        {
            player.SetPositionAndRotation(targetPos, targetRot);
            yield break;
        }

        float t = 0f;
        while (t < snapDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / snapDuration);
            player.SetPositionAndRotation(
                Vector3.Lerp(startPos, targetPos, k),
                Quaternion.Slerp(startRot, targetRot, k));
            yield return null;
        }

        player.SetPositionAndRotation(targetPos, targetRot);
    }

    private Quaternion ResolveAnchorRotation(Transform anchor, Vector3 anchorPosition, Quaternion fallback)
    {
        Quaternion baseRotation;

        if (lookAtPushedObject)
        {
            Vector3 toTarget = Flatten(PushOrigin - anchorPosition);
            baseRotation = toTarget.sqrMagnitude > 1e-4f
                ? Quaternion.LookRotation(toTarget, Vector3.up)
                : anchor.rotation;
        }
        else
        {
            baseRotation = matchAnchorRotation ? anchor.rotation : fallback;
        }

        return baseRotation * Quaternion.Euler(0f, rotationOffset, 0f);
    }

    private IEnumerator MovePushedObject()
    {
        if (pushedObject == null)
        {
            yield return new WaitForSeconds(pushDuration);
            yield break;
        }

        Transform player = _controller.transform;

        Vector3 start = pushedObject.position;
        Vector3 delta = offsetIsLocal ? pushedObject.TransformVector(pushOffset) : pushOffset;
        Vector3 end = start + delta;
        Vector3 previous = start;

        float t = 0f;
        while (t < pushDuration)
        {
            t += Time.deltaTime;
            float k = pushCurve.Evaluate(Mathf.Clamp01(t / pushDuration));

            Vector3 next = Vector3.LerpUnclamped(start, end, k);
            pushedObject.position = next;

            if (playerFollowsPush) player.position += next - previous;
            previous = next;

            yield return null;
        }

        pushedObject.position = end;
        if (playerFollowsPush) player.position += end - previous;
    }

    private void Finish()
    {
        _pushing = false;
        if (oneTimeOnly) _used = true;
        playerAnimator?.ClearPush();
        if (_controller != null) _controller.enabled = true;
        ExitInteractionMode();
    }

    private bool ResolvePlayer()
    {
        if (_controller != null) return true;

        if (playerAnimator == null && !_playerSearched)
        {
            _playerSearched = true;
            playerAnimator = FindObjectOfType<PlayerAnimator>();
        }

        if (playerAnimator == null) return false;

        _controller = playerAnimator.GetComponent<CharacterController>();
        return _controller != null;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Vector3 origin = PushOrigin;

        if (spots != null)
        {
            foreach (var spot in spots)
            {
                if (spot == null || spot.anchor == null) continue;

                Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.9f);
                Gizmos.DrawWireSphere(spot.anchor.position, 0.25f);
                Gizmos.DrawRay(spot.anchor.position, spot.anchor.forward * 0.6f);

                DrawApproachArc(origin, spot);
            }
        }

        if (pushedObject == null) return;

        Vector3 delta = offsetIsLocal ? pushedObject.TransformVector(pushOffset) : pushOffset;
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.9f);
        Gizmos.DrawLine(pushedObject.position, pushedObject.position + delta);
        Gizmos.DrawWireCube(pushedObject.position + delta, Vector3.one * 0.2f);
    }

    private void DrawApproachArc(Vector3 origin, PushSpot spot)
    {
        Vector3 toAnchor = Flatten(spot.anchor.position - origin);
        if (toAnchor.sqrMagnitude < 1e-4f) return;

        float radius = toAnchor.magnitude;
        Vector3 dir = toAnchor.normalized;

        Gizmos.color = new Color(0.3f, 1f, 0.5f, 0.35f);
        const int steps = 16;
        Vector3 prev = origin + Quaternion.AngleAxis(-spot.maxApproachAngle, Vector3.up) * dir * radius;
        Gizmos.DrawLine(origin, prev);

        for (int i = 1; i <= steps; i++)
        {
            float a = Mathf.Lerp(-spot.maxApproachAngle, spot.maxApproachAngle, i / (float)steps);
            Vector3 point = origin + Quaternion.AngleAxis(a, Vector3.up) * dir * radius;
            Gizmos.DrawLine(prev, point);
            prev = point;
        }

        Gizmos.DrawLine(origin, prev);
    }
}