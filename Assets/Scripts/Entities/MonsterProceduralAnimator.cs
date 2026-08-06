using UnityEngine;

public class MonsterProceduralAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform head;
    [SerializeField] Transform spine;

    [Header("Head Twitch")]
    [SerializeField] float minInterval = 0.05f;
    [SerializeField] float maxInterval = 0.15f;
    [SerializeField] float amplitude = 45f;
    [SerializeField] float snapSpeed = 25f;

    [Header("Spine Twitch")]
    [SerializeField] float spineMinInterval = 0.05f;
    [SerializeField] float spineMaxInterval = 0.15f;
    [SerializeField] float spineAmplitude = 45f;
    [SerializeField] float spineSnapSpeed = 25f;

    Quaternion headRest, headTargetOffset;
    Quaternion spineRest, spineTargetOffset;
    float headNextSnap, spineNextSnap;

    void Awake()
    {
        headRest = head.localRotation;
        spineRest = spine.localRotation;

        PickNewHeadTarget();
        PickNewSpineTarget();
    }

    void LateUpdate()
    {
        if (Time.time >= headNextSnap) PickNewHeadTarget();
        if (Time.time >= spineNextSnap) PickNewSpineTarget();

        Quaternion desiredHead = headRest * headTargetOffset;
        Quaternion desiredSpine = spineRest * spineTargetOffset;

        head.localRotation = Quaternion.Slerp(head.localRotation, desiredHead, snapSpeed * Time.deltaTime);
        spine.localRotation = Quaternion.Slerp(spine.localRotation, desiredSpine, spineSnapSpeed * Time.deltaTime);
    }

    void PickNewHeadTarget()
    {
        headNextSnap = Time.time + Random.Range(minInterval, maxInterval);
        headTargetOffset = RandomOffset(amplitude);
    }

    void PickNewSpineTarget()
    {
        spineNextSnap = Time.time + Random.Range(spineMinInterval, spineMaxInterval);
        spineTargetOffset = RandomOffset(spineAmplitude);
    }

    Quaternion RandomOffset(float range)
    {
        return Quaternion.Euler(
            Random.Range(-range, range),
            Random.Range(-range, range),
            Random.Range(-range, range));
    }
}