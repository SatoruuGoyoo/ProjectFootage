using UnityEngine;

[ExecuteAlways]
public class EyeTargetBroadcaster : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform eye;
    [SerializeField] Material targetMaterial;

    [SerializeField] float restPupilSize = 0.35f;
    [SerializeField] float contractedPupilSize = 0.12f;
    [SerializeField] float noticeDistance = 5f;
    [SerializeField] float contractSpeed = 14f;
    [SerializeField] float dilateSpeed = 2f;

    static readonly int TargetPosID = Shader.PropertyToID("_TargetWorldPos");
    static readonly int PupilSizeID = Shader.PropertyToID("_PupilSize");

    float currentPupil;

    void OnEnable()
    {
        currentPupil = restPupilSize;
    }

    void Update()
    {
        if (target == null || targetMaterial == null) return;

        targetMaterial.SetVector(TargetPosID, target.position);

        Transform origin = eye != null ? eye : transform;
        float distance = Vector3.Distance(target.position, origin.position);

        float t = Mathf.InverseLerp(noticeDistance, noticeDistance * 2f, distance);
        float desired = Mathf.Lerp(contractedPupilSize, restPupilSize, t);

        float speed = desired < currentPupil ? contractSpeed : dilateSpeed;
        currentPupil = Mathf.Lerp(currentPupil, desired, speed * Time.deltaTime);

        targetMaterial.SetFloat(PupilSizeID, currentPupil);
    }

    public void Flinch()
    {
        currentPupil = contractedPupilSize;
    }
}