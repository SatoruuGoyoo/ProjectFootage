using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RNGAmbienceZone : MonoBehaviour
{
    [Header("Zone")]
    [SerializeField] private string zoneId;

    [Header("Timing (seconds between sounds)")]
    [SerializeField] private float minInterval = 4f;
    [SerializeField] private float maxInterval = 12f;

    [Header("Spatialization")]
    [Tooltip("Emission points where ambient sounds can play from (corners, window, ceiling, etc). If empty, falls back to the zone's own position.")]
    [SerializeField] private Transform[] emissionPoints;
    [Tooltip("If true, picks the emission point closest to the player instead of a random one.")]
    [SerializeField] private bool preferClosestToPlayer = false;

    private bool _playerInside;
    private float _timer;
    private Transform _player;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        _player = other.transform;
        ResetTimer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
        _player = null;
    }

    private void Update()
    {
        if (!_playerInside) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            PlayRandomSound();
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        _timer = Random.Range(minInterval, maxInterval);
    }

    private void PlayRandomSound()
    {
        AmbiencePack pack = RNGAmbienceManager.Instance != null ? RNGAmbienceManager.Instance.CurrentPack : null;
        if (pack == null) return;

        EventReference[] sounds = pack.GetSoundsForZone(zoneId);
        if (sounds == null || sounds.Length == 0) return;

        EventReference chosen = sounds[Random.Range(0, sounds.Length)];
        if (chosen.IsNull) return;

        Vector3 emitPos = GetEmissionPosition();
        RuntimeManager.PlayOneShot(chosen, emitPos);
    }

    private Vector3 GetEmissionPosition()
    {
        if (emissionPoints == null || emissionPoints.Length == 0)
            return transform.position;

        if (preferClosestToPlayer && _player != null)
        {
            Transform closest = null;
            float best = float.MaxValue;
            foreach (var p in emissionPoints)
            {
                if (p == null) continue;
                float d = (p.position - _player.position).sqrMagnitude;
                if (d < best) { best = d; closest = p; }
            }
            if (closest != null) return closest.position;
        }

        Transform pick = emissionPoints[Random.Range(0, emissionPoints.Length)];
        return pick != null ? pick.position : transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (emissionPoints == null) return;
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.9f);
        foreach (var p in emissionPoints)
        {
            if (p == null) continue;
            Gizmos.DrawWireSphere(p.position, 0.3f);
            Gizmos.DrawLine(transform.position, p.position);
        }
    }
}