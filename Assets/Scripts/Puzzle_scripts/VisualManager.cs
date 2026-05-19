//using UnityEngine;

//public class IterationVisualManager : MonoBehaviour
//{
//    [System.Serializable]
//    public struct IterationVisuals
//    {
//        public string iterationName;

//        [System.Serializable]
//        public struct MaterialSwap
//        {
//            public Renderer target;
//            public Material material;
//        }

//        public MaterialSwap[] materialSwaps;

//        [Header("Environment Lighting")]
//        [Range(0f, 1f)]
//        public float environmentIntensity;

//        [Header("TV Settings")]
//        public GameObject[] objectsToEnable;
//        public GameObject[] objectsToDisable;
//    }

//    [Header("Visuales por iteración")]
//    public IterationVisuals[] iterations = new IterationVisuals[3];

//    private void OnEnable() => GameEvents.OnIterationChanged += OnIterationChanged;
//    private void OnDisable() => GameEvents.OnIterationChanged -= OnIterationChanged;

//    private void Start()
//    {
//        OnIterationChanged(0);
//    }

//    private void OnIterationChanged(int iteration)
//    {
//        if (iteration >= iterations.Length) return;

//        var visuals = iterations[iteration];

  
//        foreach (var swap in visuals.materialSwaps)
//        {
//            if (swap.target == null || swap.material == null) continue;
//            swap.target.material = swap.material;
//        }
//        RenderSettings.ambientIntensity = visuals.environmentIntensity;

//        foreach (var obj in visuals.objectsToEnable)
//            if (obj != null) obj.SetActive(true);

//        foreach (var obj in visuals.objectsToDisable)
//            if (obj != null) obj.SetActive(false);

//        Debug.Log($"[IterationVisualManager] Iteración {iteration + 1} — Env Intensity: {visuals.environmentIntensity}");
//    }
//}