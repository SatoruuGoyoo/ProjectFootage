using UnityEngine;

public class ClockController : MonoBehaviour
{
    [Header("Setup")]
    public Transform hourHandPivot;
    [SerializeField] private float rotationSpeed = 30f;

    public void Update()
    {
        hourHandPivot.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}