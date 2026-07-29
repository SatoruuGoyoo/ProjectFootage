using System;
using UnityEngine;

public class SwapMeshStep : SequenceStep
{
    [Serializable]
    public class MeshSwap
    {
        public MeshFilter targetMeshFilter;
        public Mesh newMesh;
        public MeshCollider colliderToUpdate;
    }

    [SerializeField] private MeshSwap[] swaps;

    protected override void OnExecute()
    {
        if (swaps != null)
        {
            foreach (var swap in swaps)
                Apply(swap);
        }

        Complete();
    }

    private void Apply(MeshSwap swap)
    {
        if (swap.targetMeshFilter == null || swap.newMesh == null) return;

        swap.targetMeshFilter.sharedMesh = swap.newMesh;

        if (swap.colliderToUpdate != null)
            swap.colliderToUpdate.sharedMesh = swap.newMesh;
    }
}