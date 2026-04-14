using UnityEngine;

public class CamcorderKeyTarget : MonoBehaviour, ICamcorderTarget
{
    [SerializeField] private bool _isActive = true;

    public bool IsActive => _isActive;
    public Transform TargetTransform => transform;

    // Allows enabling or disabling the target at runtime
    public void SetActive(bool value) => _isActive = value;


}
