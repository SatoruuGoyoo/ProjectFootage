using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class RainQuadAligner : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distance = 6f;
    [SerializeField] private float coverage = 1.15f;

    void OnEnable() => Align();
    void OnValidate() => Align();

    void Align()
    {
        if (targetCamera == null) return;

        Transform cam = targetCamera.transform;
        transform.position = cam.position + cam.forward * distance;
        transform.rotation = Quaternion.LookRotation(-cam.forward, cam.up);

        float h = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float w = h * targetCamera.aspect;
        transform.localScale = new Vector3(w * coverage, h * coverage, 1f);
    }
}