using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CamcorderCameraClip : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Anchor desde donde se tira el SphereCast (típicamente el pecho/cabeza del player)")]
    [SerializeField] private Transform anchor;
    [Tooltip("Layers que bloquean la cámara (paredes, puertas, etc.)")]
    [SerializeField] private LayerMask blockingMask = ~0;

    [Header("Config")]
    [Tooltip("Radio de la SphereCast — más grande = la cámara se queda más lejos de paredes")]
    [SerializeField] private float sphereRadius = 0.15f;
    [Tooltip("Padding adicional — separa la cámara un poco más de la pared")]
    [SerializeField] private float padding = 0.05f;
    [Tooltip("Velocidad de suavizado al adaptarse a paredes (más alto = más rápido)")]
    [SerializeField] private float smoothSpeed = 15f;

    private Vector3 _originalLocalPosition;
    private Transform _parent;
    private float _currentDistance;
    private float _maxDistance;

    private void Awake()
    {
        _parent = transform.parent;
        _originalLocalPosition = transform.localPosition;
        _maxDistance = _originalLocalPosition.magnitude;
        _currentDistance = _maxDistance;
    }

    private void LateUpdate()
    {
        if (anchor == null || _parent == null) return;

        Vector3 anchorPos = anchor.position;
        Vector3 targetPos = _parent.TransformPoint(_originalLocalPosition);

        Vector3 dir = targetPos - anchorPos;
        float maxDist = dir.magnitude;
        if (maxDist < 0.0001f) return;

        Vector3 dirNorm = dir / maxDist;

        float desiredDistance;
        if (Physics.SphereCast(anchorPos, sphereRadius, dirNorm, out RaycastHit hit, maxDist, blockingMask, QueryTriggerInteraction.Ignore))
        {
            desiredDistance = Mathf.Max(0f, hit.distance - padding);
            Debug.Log($"[Clip] HIT: {hit.collider.name} | layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} | dist: {hit.distance}");
        }
        else
        {
            desiredDistance = maxDist;
            Debug.Log($"[Clip] NO HIT | maxDist: {maxDist} | anchor: {anchorPos} | target: {targetPos}");
        }

        _currentDistance = Mathf.Lerp(_currentDistance, desiredDistance, Time.deltaTime * smoothSpeed);

        transform.position = anchorPos + dirNorm * _currentDistance;

        Debug.DrawLine(anchorPos, transform.position, Color.cyan);
    }
}