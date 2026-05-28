using UnityEngine;
using FMODUnity;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMotor playerMotor;

    [Header("Footstep SFX")]
    [SerializeField] private EventReference walkEvent;
    [SerializeField] private EventReference runEvent;
    [SerializeField] private float walkStepInterval = 1.5f;
    [SerializeField] private float runStepInterval = 0.75f;
    [SerializeField] private float minMoveSpeed = 0.1f;

    [Header("Just in case")]
    [SerializeField] private float teleportThreshold = 5f;

    private Vector3 _lastPos;
    private float _distanceAccumulated;
    private bool _wasSprinting;

    private void OnEnable()
    {
        _lastPos = transform.position;
        _distanceAccumulated = 0;
        _wasSprinting = false;
    }

    private void Update()
    {
        bool sprinting = playerMotor != null && playerMotor.IsSprinting;

        if (sprinting != _wasSprinting)
        {
            _distanceAccumulated = 0f;
            _wasSprinting = sprinting;
        }

        Vector3 delta = transform.position - _lastPos;
        delta.y = 0f;
        float moved = delta.magnitude;

        if (moved > teleportThreshold)
        {
            _lastPos = transform.position;
            return;
        }

        if (moved / Time.deltaTime >= minMoveSpeed)
        {
            _distanceAccumulated += moved;
            float interval = sprinting ? runStepInterval : walkStepInterval;

            if (_distanceAccumulated >= interval)
            {
                _distanceAccumulated -= interval;
                PlayFootstep(sprinting);
            }
        }

        _lastPos = transform.position;
    }

    private void PlayFootstep(bool sprinting)
    {
        EventReference evt = sprinting ? runEvent : walkEvent;
        if (evt.IsNull) return;
        RuntimeManager.PlayOneShot(evt, transform.position);
    }
}
