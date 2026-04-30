using UnityEngine;

public class CreepyHeadTrack : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform headBone;
    [SerializeField] private Transform target; // el Player

    [Header("Config")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxAngle = 80f; // límite para que no gire 360

    private Quaternion initialHeadRotation;

    private void Start()
    {
        initialHeadRotation = headBone.rotation;
    }

    // LateUpdate porque el Animator actualiza los huesos en Update
    // si lo hacés en Update, el Animator pisa tu rotación
    private void LateUpdate()
    {
        Vector3 directionToTarget = target.position - headBone.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Chequeamos que no exceda el límite de giro
        float angle = Quaternion.Angle(initialHeadRotation, targetRotation);
        if (angle > maxAngle) return;

        headBone.rotation = Quaternion.Slerp(
            headBone.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}