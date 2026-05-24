using System.Collections;
using UnityEngine;

public class IterationVisualManager : MonoBehaviour
{
    [System.Serializable]
    public struct IterationVisuals
    {
        public string iterationName;

        [System.Serializable]
        public struct MaterialSwap
        {
            public Renderer target;
            public Material material;
        }

        public MaterialSwap[] materialSwaps;

        [Header("Environment Lighting")]
        [Range(0f, 1f)]
        public float environmentIntensity;

        [Header("Objetos")]
        public GameObject[] objectsToEnable;
        public GameObject[] objectsToDisable;
    }

    [Header("Visuales por iteración")]
    public IterationVisuals[] iterations;

    [Header("Transición de luz")]
    [Tooltip("Segundos que tarda en bajar la intensidad ambiental al valor objetivo.")]
    [SerializeField, Min(0.1f)] private float lightTransitionDuration = 3f;

    private Coroutine _lightTransition;

    private void OnEnable() => CorridorTeleporter.OnIterationChanged += OnIterationChanged;
    private void OnDisable() => CorridorTeleporter.OnIterationChanged -= OnIterationChanged;

    private void Start() => ApplyVisuals(0);

    private void OnIterationChanged(int iteration) => ApplyVisuals(iteration - 1);

    private void ApplyVisuals(int index)
    {
        if (index < 0 || index >= iterations.Length) return;

        var v = iterations[index];

        foreach (var swap in v.materialSwaps)
        {
            if (swap.target == null || swap.material == null) continue;
            swap.target.material = swap.material;
        }

        // Luz: transición suave en lugar de snap
        if (_lightTransition != null) StopCoroutine(_lightTransition);
        _lightTransition = StartCoroutine(TransitionLight(v.environmentIntensity));

        foreach (var obj in v.objectsToEnable)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in v.objectsToDisable)
            if (obj != null) obj.SetActive(false);

        Debug.Log($"[IterationVisualManager] Iteración {index + 1} — {v.iterationName}");
    }

    private IEnumerator TransitionLight(float targetIntensity)
    {
        float startIntensity = RenderSettings.ambientIntensity;
        float elapsed = 0f;

        while (elapsed < lightTransitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lightTransitionDuration;

            // EaseInOut para que la bajada se sienta orgánica, no lineal
            float tSmooth = t * t * (3f - 2f * t);

            RenderSettings.ambientIntensity = Mathf.Lerp(startIntensity, targetIntensity, tSmooth);
            yield return null;
        }

        RenderSettings.ambientIntensity = targetIntensity;
        _lightTransition = null;
    }
}