using System.Collections;
using UnityEngine;

public class TransformStep : SequenceStep
{
    private enum MotionMode { Duration, Speed }

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("What to animate")]
    [SerializeField] private bool move = false;
    [SerializeField] private bool rotate = false;
    [SerializeField] private bool scale = false;

    [Header("Space")]
    [SerializeField] private bool useLocalSpace = true;
    [SerializeField] private bool relative = false;

    [Header("Destination")]
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Vector3 targetEulerAngles;
    [SerializeField] private Vector3 targetScale = Vector3.one;

    [Header("Timing")]
    [SerializeField] private MotionMode mode = MotionMode.Duration;
    [SerializeField] private float duration = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private AnimationCurve easing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Sequencing")]
    [SerializeField] private bool waitForCompletion = true;

    private Vector3 _startPos;
    private Quaternion _startRot;
    private Vector3 _startScale;
    private Vector3 _endPos;
    private Quaternion _endRot;
    private Vector3 _endScale;

    protected override void OnExecute()
    {
        if (target == null)
        {
            Complete();
            return;
        }

        CaptureStartAndEnd();
        StartCoroutine(Animate());
    }

    private void CaptureStartAndEnd()
    {
        _startPos = useLocalSpace ? target.localPosition : target.position;
        _startRot = useLocalSpace ? target.localRotation : target.rotation;
        _startScale = target.localScale;

        _endPos = relative ? _startPos + targetPosition : targetPosition;
        _endRot = relative
            ? _startRot * Quaternion.Euler(targetEulerAngles)
            : Quaternion.Euler(targetEulerAngles);
        _endScale = relative ? _startScale + targetScale : targetScale;
    }

    private IEnumerator Animate()
    {
        float totalTime = mode == MotionMode.Duration ? duration : DurationFromSpeed();

        // A zero or negative time means snap straight to the destination.
        if (totalTime <= 0f)
        {
            ApplyDestination();
            Complete();
            yield break;
        }

        // When we don't need to block the sequence, let the next step start
        // right away while this one keeps animating in the background.
        if (!waitForCompletion)
            Complete();

        float elapsed = 0f;
        while (elapsed < totalTime)
        {
            elapsed += Time.deltaTime;
            float eased = easing.Evaluate(Mathf.Clamp01(elapsed / totalTime));
            ApplyInterpolated(eased);
            yield return null;
        }

        // Land exactly on the destination so float drift never leaves a gap.
        ApplyDestination();

        if (waitForCompletion)
            Complete();
    }

    private void ApplyInterpolated(float t)
    {
        if (move)
        {
            Vector3 pos = Vector3.LerpUnclamped(_startPos, _endPos, t);
            if (useLocalSpace) target.localPosition = pos;
            else target.position = pos;
        }

        if (rotate)
        {
            Quaternion rot = Quaternion.SlerpUnclamped(_startRot, _endRot, t);
            if (useLocalSpace) target.localRotation = rot;
            else target.rotation = rot;
        }

        if (scale)
            target.localScale = Vector3.LerpUnclamped(_startScale, _endScale, t);
    }

    private void ApplyDestination()
    {
        if (move)
        {
            if (useLocalSpace) target.localPosition = _endPos;
            else target.position = _endPos;
        }

        if (rotate)
        {
            if (useLocalSpace) target.localRotation = _endRot;
            else target.rotation = _endRot;
        }

        if (scale)
            target.localScale = _endScale;
    }

   
    private float DurationFromSpeed()
    {
        if (speed <= 0f)
            return 0f;

        float longest = 0f;

        if (move)
            longest = Mathf.Max(longest, Vector3.Distance(_startPos, _endPos) / speed);

        if (scale)
            longest = Mathf.Max(longest, Vector3.Distance(_startScale, _endScale) / speed);

        if (rotate)
            longest = Mathf.Max(longest, Quaternion.Angle(_startRot, _endRot) / (90f * speed));

        return longest;
    }
}