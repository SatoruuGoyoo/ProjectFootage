using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class Stalker : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Movement")]
    [SerializeField] private float approachSpeed = 2.5f;
    [SerializeField] private float heightOffset = 0f;

    [Header("Footsteps")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private float stepInterval = 0.7f;

    [Header("Arrival")]
    [SerializeField] private float arrivalDistance = 1f;
    public UnityEvent OnReachedPlayer;

    private float _distanceAccumulated;
    private bool _isActive;
    private bool _hasReached;

    private void OnEnable()
    {
        ResetToSpawn();
        _isActive = true;
        _hasReached = false;
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
        if (!_isActive || player == null) return;

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

        if (!_hasReached && Vector3.Distance(transform.position, player.position) <= arrivalDistance)
        {
            _hasReached = true;
            _isActive = false;
            OnReachedPlayer?.Invoke();
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

        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        if (player != null)
            Gizmos.DrawWireSphere(player.position, arrivalDistance);
    }
}