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
    [SerializeField] private bool playAtZonePosition = true;

    private bool _playerInside;
    private float _timer;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = true;
        ResetTimer();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;
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

        if (playAtZonePosition)
            RuntimeManager.PlayOneShot(chosen, transform.position);
        else
            RuntimeManager.PlayOneShot(chosen);
    }
}
