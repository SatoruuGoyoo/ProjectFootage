using UnityEngine;

public class ClockController : MonoBehaviour
{
    [Header("Setup")]
    public Transform hourHandPivot;
    [SerializeField] private float anglePerFrame = 6f;

    private void OnEnable()
    {
        GameEvents.OnFrameChanged += UpdateClock;
    }

    private void OnDisable()
    {
        GameEvents.OnFrameChanged -= UpdateClock;
    }

    public void UpdateClock(int frame)
    {
        hourHandPivot.localRotation = Quaternion.Euler(0f, frame * anglePerFrame, 0f);
    }
}