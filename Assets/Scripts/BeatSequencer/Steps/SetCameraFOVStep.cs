using System.Collections;
using UnityEngine;

public class SetCameraFOVStep : SequenceStep
{
    [SerializeField] private float targetFOV = 40f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool waitForCompletion = true;

    protected override void OnExecute()
    {
        Camera cam = CameraManager.Instance?.ActiveCamera;

        if (cam == null)
        {
            Debug.LogWarning("[SetCameraFOVStep] No hay cámara activa.");
            Complete();
            return;
        }

        if (duration <= 0f)
        {
            cam.fieldOfView = targetFOV;
            Complete();
            return;
        }

        if (waitForCompletion)
            StartCoroutine(AnimateFOV(cam));
        else
        {
            StartCoroutine(AnimateFOV(cam));
            Complete();
        }
    }

    private IEnumerator AnimateFOV(Camera cam)
    {
        float startFOV = cam.fieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }

        cam.fieldOfView = targetFOV;

        if (waitForCompletion)
            Complete();
    }
}