using UnityEngine;
public class RainQuadAligner : MonoBehaviour
{
    public Transform targetCamera;
    public float distance = 6f;

    void Update()
    {
        if (targetCamera == null) return;
        transform.position = targetCamera.position + targetCamera.forward * distance;
        transform.rotation = Quaternion.LookRotation(-targetCamera.forward, Vector3.up);
    }
}