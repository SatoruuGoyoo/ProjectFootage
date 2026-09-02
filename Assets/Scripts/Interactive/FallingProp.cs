using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

[RequireComponent(typeof(Rigidbody))]
public class FallingProp : MonoBehaviour
{
    [Header("Caída")]
    [SerializeField] private float delay = 0f;
    [Tooltip("Se desprende del padre para no seguir arrastrado por el mueble.")]
    [SerializeField] private bool detachFromParent = true;
    [SerializeField] private Vector3 impulse = Vector3.zero;
    [SerializeField] private Vector3 torque = new Vector3(0f, 0f, 2f);
    [SerializeField] private bool impulseIsLocal = true;

    [Header("Audio")]
    [SerializeField] private EventReference dropSound;
    [SerializeField] private EventReference impactSound;
    [Tooltip("Velocidad mínima del choque para que suene el impacto.")]
    [SerializeField] private float minImpactSpeed = 1.2f;
    [SerializeField] private int maxImpacts = 2;

    [Header("Events")]
    public UnityEvent OnDropped;
    public UnityEvent OnFirstImpact;

    private Rigidbody _body;
    private bool _dropped;
    private int _impacts;

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _body.isKinematic = true;
        _body.useGravity = false;
    }

    [ContextMenu("Drop")]
    public void Drop()
    {
        if (_dropped) return;
        _dropped = true;

        if (delay > 0f) StartCoroutine(DropAfterDelay());
        else Release();
    }

    private IEnumerator DropAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        Release();
    }

    private void Release()
    {
        if (detachFromParent) transform.SetParent(null, true);

        _body.isKinematic = false;
        _body.useGravity = true;

        Vector3 push = impulseIsLocal ? transform.TransformVector(impulse) : impulse;
        if (push != Vector3.zero) _body.AddForce(push, ForceMode.VelocityChange);
        if (torque != Vector3.zero) _body.AddTorque(torque, ForceMode.VelocityChange);

        if (!dropSound.IsNull) RuntimeManager.PlayOneShot(dropSound, transform.position);

        OnDropped?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_dropped || _impacts >= maxImpacts) return;
        if (collision.relativeVelocity.magnitude < minImpactSpeed) return;

        _impacts++;

        if (!impactSound.IsNull)
            RuntimeManager.PlayOneShot(impactSound, collision.GetContact(0).point);

        if (_impacts == 1) OnFirstImpact?.Invoke();
    }
}