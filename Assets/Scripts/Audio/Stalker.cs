using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Stalker : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMotor playerMotor;

    [Header("Follow")]
    [Tooltip("Distancia normal por detrás del player (caminando)")]
    [SerializeField] private float walkDistance = 4f;
    [Tooltip("Distancia cuando el player corre (se acerca)")]
    [SerializeField] private float sprintDistance = 1.5f;
    [Tooltip("Qué tan rápido cambia la distancia entre caminar y correr")]
    [SerializeField] private float distanceLerpSpeed = 3f;
    [Tooltip("Qué tan rápido se reposiciona detrás del player")]
    [SerializeField] private float followSmooth = 5f;
    [Tooltip("Altura del seguidor respecto al player")]
    [SerializeField] private float heightOffset = 0f;

    [Header("Footsteps")]
    [SerializeField] private EventReference walkFootstep;
    [SerializeField] private EventReference sprintFootstep;
    [Tooltip("Metros entre pasos caminando")]
    [SerializeField] private float walkStepInterval = 1.5f;
    [Tooltip("Metros entre pasos corriendo (más chico = más seguidos)")]
    [SerializeField] private float sprintStepInterval = 0.9f;
    [Tooltip("Velocidad mínima del player para que cuenten los pasos")]
    [SerializeField] private float minMoveSpeed = 0.1f;

    [Header("Teleport")]
    [Tooltip("Si el player se mueve más que esto en un frame, se asume teleport y se ignora")]
    [SerializeField] private float teleportThreshold = 2f;

    private Vector3 _lastPlayerPos;
    private float _distanceAccumulated;
    private float _currentDistance;
    private bool _wasSprinting;

    private void OnEnable()
    {
        _currentDistance = walkDistance;
        _wasSprinting = false;
        if (player != null)
        {
            _lastPlayerPos = player.position;
            transform.position = GetTargetPosition();
        }
        _distanceAccumulated = 0f;
    }

    private void Update()
    {
        if (player == null) return;

        bool sprinting = playerMotor != null && playerMotor.IsSprinting;

        if (sprinting != _wasSprinting)
        {
            _distanceAccumulated = 0f;
            _wasSprinting = sprinting;
        }

        float targetDistance = sprinting ? sprintDistance : walkDistance;
        _currentDistance = Mathf.Lerp(_currentDistance, targetDistance, Time.deltaTime * distanceLerpSpeed);

        Vector3 delta = player.position - _lastPlayerPos;
        delta.y = 0f;
        float moved = delta.magnitude;

        if (moved > teleportThreshold)
        {
            _lastPlayerPos = player.position;
            transform.position = GetTargetPosition();
            return;
        }

        transform.position = Vector3.Lerp(
            transform.position,
            GetTargetPosition(),
            Time.deltaTime * followSmooth
        );

        if (moved / Time.deltaTime >= minMoveSpeed)
        {
            _distanceAccumulated += moved;
            float interval = sprinting ? sprintStepInterval : walkStepInterval;

            if (_distanceAccumulated >= interval)
            {
                _distanceAccumulated -= interval;
                PlayFootstep(sprinting);
            }
        }

        _lastPlayerPos = player.position;
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 behind = player.position - player.forward * _currentDistance;
        behind.y = player.position.y + heightOffset;
        return behind;
    }

    private void PlayFootstep(bool sprinting)
    {
        EventReference evt = sprinting ? sprintFootstep : walkFootstep;
        if (evt.IsNull) return;
        FMODManager.Instance.PlayOneShot(evt, transform.position);
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
    }
}