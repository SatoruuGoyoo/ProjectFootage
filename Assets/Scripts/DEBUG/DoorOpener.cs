using System.Collections;
using UnityEngine;

public class DoorOpener : MonoBehaviour
{
    [Header("Bisagra (GameObject vacío en el borde)")]
    public Transform hinge;

    [Header("Rotación")]
    public float targetAngle = 90f;  
    public float duration = 0.8f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool _opened = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_opened || !other.CompareTag("Player")) return;
        _opened = true;

        StartCoroutine(RotateDoor());
        GetComponent<Collider>().enabled = false;
    }

    private IEnumerator RotateDoor()
    {
        Quaternion startRot = hinge.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, targetAngle, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve.Evaluate(elapsed / duration);
            hinge.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        hinge.localRotation = endRot;
    }
}