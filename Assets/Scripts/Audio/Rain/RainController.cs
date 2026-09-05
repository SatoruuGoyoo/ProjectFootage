using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Local Beat 1 rain mixer. Both events run together and their volumes blend
/// continuously according to the player's distance from each 3D source point.
/// </summary>
public class RainController : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference interiorRain;
    [SerializeField] private EventReference exteriorRain;

    [Header("3D Source Positions")]
    [SerializeField] private Transform interiorSource;
    [SerializeField] private Transform exteriorSource;

    [Header("Blend")]
    [Tooltip("Optional. If empty, the object tagged Player is used.")]
    [SerializeField] private Transform player;
    [Tooltip("How quickly the volume follows the player's position.")]
    [SerializeField, Min(0.01f)] private float blendSpeed = 3f;
    [Tooltip("At this distance from Interior Source, interior rain reaches zero volume.")]
    [SerializeField, Min(0.01f)] private float interiorFadeOutDistance = 10f;
    [Tooltip("At this distance from Exterior Source, exterior rain reaches zero volume.")]
    [SerializeField, Min(0.01f)] private float exteriorFadeOutDistance = 10f;

    private EventInstance _interiorInstance;
    private EventInstance _exteriorInstance;
    private float _interiorVolume = 1f;
    private float _exteriorVolume;
    private bool _rainStopped;

    private IEnumerator Start()
    {
        // Wait one frame so FMOD is initialized before the first events start.
        yield return null;
        ResolvePlayer();
        StartRain(ref _interiorInstance, interiorRain, interiorSource);
        StartRain(ref _exteriorInstance, exteriorRain, exteriorSource);
        UpdateBlend(true);
    }

    private void Update()
    {
        if (_rainStopped) return;
        UpdateBlend(false);
    }

    private void UpdateBlend(bool immediate)
    {
        if (player == null) ResolvePlayer();
        if (player == null || interiorSource == null || exteriorSource == null) return;

        float distanceToInterior = Vector3.Distance(player.position, interiorSource.position);
        float distanceToExterior = Vector3.Distance(player.position, exteriorSource.position);
        // Each source is silent outside its configured fade-out distance.
        // Make the two distances overlap around the doorway for a smooth blend.
        float interiorTarget = 1f - Mathf.Clamp01(distanceToInterior / interiorFadeOutDistance);
        float exteriorTarget = 1f - Mathf.Clamp01(distanceToExterior / exteriorFadeOutDistance);

        float maxDelta = immediate ? 1f : blendSpeed * Time.deltaTime;
        _interiorVolume = Mathf.MoveTowards(_interiorVolume, interiorTarget, maxDelta);
        _exteriorVolume = Mathf.MoveTowards(_exteriorVolume, exteriorTarget, maxDelta);

        if (_interiorInstance.isValid()) _interiorInstance.setVolume(_interiorVolume);
        if (_exteriorInstance.isValid()) _exteriorInstance.setVolume(_exteriorVolume);
    }

    private void ResolvePlayer()
    {
        if (player != null) return;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    private void StartRain(ref EventInstance instance, EventReference eventReference, Transform source)
    {
        if (eventReference.IsNull)
        {
            Debug.LogWarning($"{name}: assign a rain event.", this);
            return;
        }

        instance = RuntimeManager.CreateInstance(eventReference);
        RuntimeManager.AttachInstanceToGameObject(instance, source != null ? source.gameObject : gameObject);
        instance.start();
    }

    /// <summary>Stops both rain instances for the rest of this Beat.</summary>
    public void StopRain()
    {
        _rainStopped = true;
        StopAndRelease(ref _interiorInstance);
        StopAndRelease(ref _exteriorInstance);
    }

    private void OnDisable() => StopRain();
    private void OnDestroy() => StopRain();

    private static void StopAndRelease(ref EventInstance instance)
    {
        if (!instance.isValid()) return;
        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
        instance = default;
    }
}
