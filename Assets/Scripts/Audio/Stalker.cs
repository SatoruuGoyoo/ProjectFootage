using UnityEngine;
using FMODUnity;

public class Stalker : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Spawn")]
    [Tooltip("Posición donde el Stalker aparece cuando se activa.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Movement")]
    [Tooltip("Velocidad a la que el Stalker se acerca al player.")]
    [SerializeField] private float approachSpeed = 2.5f;
    [Tooltip("Altura del Stalker respecto al player (ajuste vertical).")]
    [SerializeField] private float heightOffset = 0f;

    [Header("Footsteps")]
    [SerializeField] private EventReference footstepEvent;
    [Tooltip("Metros recorridos entre cada paso.")]
    [SerializeField] private float stepInterval = 0.7f;

    private float _distanceAccumulated;
    private bool _isActive;

    private void OnEnable()
    {
        ResetToSpawn();
        _isActive = true;
    }

    private void OnDisable()
    {
        _isActive = false;
    }

    private void ResetToSpawn()
    {
        _distanceAccumulated = 0f;
        if (spawnPoint != null)
            transform.position = spawnPoint.position;
    }

    private void Update()
    {
        if (!_isActive) return;
        if (player == null) return;

        Vector3 targetPos = player.position;
        targetPos.y += heightOffset;

        Vector3 prevPos = transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            approachSpeed * Time.deltaTime
        );

        float moved = Vector3.Distance(prevPos, transform.position);
        _distanceAccumulated += moved;

        if (_distanceAccumulated >= stepInterval)
        {
            _distanceAccumulated -= stepInterval;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        if (footstepEvent.IsNull) return;
        FMODManager.Instance.PlayOneShot(footstepEvent, transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, 0.4f);

        if (player != null)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
            Gizmos.DrawLine(transform.position, player.position);
        }

        if (spawnPoint != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
            Gizmos.DrawWireSphere(spawnPoint.position, 0.6f);
            Gizmos.DrawLine(spawnPoint.position, transform.position);
        }
    }
}