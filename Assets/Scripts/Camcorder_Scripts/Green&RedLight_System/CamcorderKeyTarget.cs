using UnityEngine;

public class CamcorderKeyTarget : MonoBehaviour, ICamcorderTarget
{
    [SerializeField] private bool _isActive = true;

    public bool IsActive => _isActive;
    public Transform TargetTransform => transform;

    public float DetectionRadius => 0.5f; 

    private void OnEnable()
    {
        if(CamcorderLightSystem.Instance != null)
            CamcorderLightSystem.Instance.Register(this);
    }

    private void OnDisable()
    {
        if (CamcorderLightSystem.Instance != null)
            CamcorderLightSystem.Instance.Unregister(this);
    }

    // Allows enabling or disabling the target at runtime
    public void SetActive(bool value) => _isActive = value;


}
