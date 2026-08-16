using UnityEngine;
using UnityEngine.Video;

public class CamcorderKeyTarget : MonoBehaviour, ICamcorderTarget
{
    [SerializeField] private bool _isActive = true;

    public bool IsActive => _isActive;
    public Transform TargetTransform => transform;

    public float DetectionRadius => 0.5f;

    private void OnEnable()
    {
        if (CamcorderDetectionSystem.Instance != null)
            CamcorderDetectionSystem.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (CamcorderDetectionSystem.Instance != null)
            CamcorderDetectionSystem.Instance.Unregister(this);
    }

    public void SetActive(bool value) => _isActive = value;

    public bool TryGetLiveActionClip(out VideoClip clip)
    {
        clip = null;
        return false;
    }
}