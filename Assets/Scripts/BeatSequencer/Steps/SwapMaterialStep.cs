using System;
using UnityEngine;

public class SwapMaterialStep : SequenceStep
{
    [Serializable]
    public class MaterialSwap
    {
        public Renderer targetRenderer;
        public Material newMaterial;
        public int materialIndex = 0;
    }

    [Serializable]
    public class ZoneSwap
    {
        public Transform zoneRoot;
        public Material newMaterial;
        public bool includeInactive = false;
    }

    [SerializeField] private MaterialSwap[] swaps;
    [SerializeField] private ZoneSwap[] zoneSwaps;

    private Renderer[][] _cachedZoneRenderers;

    private void Awake()
    {
        CacheZoneRenderers();
    }

    private void CacheZoneRenderers()
    {
        if (zoneSwaps == null) return;

        _cachedZoneRenderers = new Renderer[zoneSwaps.Length][];
        for (int i = 0; i < zoneSwaps.Length; i++)
        {
            if (zoneSwaps[i].zoneRoot != null)
                _cachedZoneRenderers[i] = zoneSwaps[i].zoneRoot.GetComponentsInChildren<Renderer>(zoneSwaps[i].includeInactive);
        }
    }

    protected override void OnExecute()
    {
        if (swaps != null)
        {
            foreach (var swap in swaps)
                ApplySingle(swap);
        }

        if (zoneSwaps != null)
        {
            for (int i = 0; i < zoneSwaps.Length; i++)
                ApplyZone(i);
        }

        Complete();
    }

    private void ApplySingle(MaterialSwap swap)
    {
        if (swap.targetRenderer == null || swap.newMaterial == null) return;

        if (swap.materialIndex == 0 && swap.targetRenderer.sharedMaterials.Length <= 1)
        {
            swap.targetRenderer.sharedMaterial = swap.newMaterial;
            return;
        }

        Material[] mats = swap.targetRenderer.sharedMaterials;
        if (swap.materialIndex >= 0 && swap.materialIndex < mats.Length)
        {
            mats[swap.materialIndex] = swap.newMaterial;
            swap.targetRenderer.sharedMaterials = mats;
        }
    }

    private void ApplyZone(int index)
    {
        Renderer[] renderers = _cachedZoneRenderers != null && index < _cachedZoneRenderers.Length
            ? _cachedZoneRenderers[index]
            : null;

        if (renderers == null || zoneSwaps[index].newMaterial == null) return;

        Material mat = zoneSwaps[index].newMaterial;
        foreach (var r in renderers)
        {
            if (r == null) continue;

            if (r.sharedMaterials.Length <= 1)
            {
                r.sharedMaterial = mat;
            }
            else
            {
                Material[] mats = r.sharedMaterials;
                for (int m = 0; m < mats.Length; m++)
                    mats[m] = mat;
                r.sharedMaterials = mats;
            }
        }
    }
}