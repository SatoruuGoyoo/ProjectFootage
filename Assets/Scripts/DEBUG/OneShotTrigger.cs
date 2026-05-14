using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OneShotTrigger : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference oneShotEvent;

    [Header("Detección")]
    [SerializeField] private string playerTag = "Player";

    [Header("Comportamiento")]
    [SerializeField] private bool triggerOnce = true;

    [SerializeField] private float retriggerCooldown = 1f;

    [SerializeField] private float delay = 0f;

    private bool fired = false;
    private float lastTriggerTime = -999f;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (triggerOnce && fired) return;
        if (Time.time - lastTriggerTime < retriggerCooldown) return;

        fired = true;
        lastTriggerTime = Time.time;

        if (delay > 0f) Invoke(nameof(PlayScare), delay);
        else PlayScare();
    }

    private void PlayScare()
    {
        FMODManager.Instance.PlayOneShot(oneShotEvent, transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = fired ? new Color(0.5f, 0.5f, 0.5f, 0.3f) : new Color(1f, 0.3f, 0.3f, 0.3f);
        var col = GetComponent<Collider>();

        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
            Gizmos.DrawCube(box.center, box.size);
        else if (col is SphereCollider sphere)
            Gizmos.DrawSphere(sphere.center, sphere.radius);
    }
}