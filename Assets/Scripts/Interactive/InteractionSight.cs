using UnityEngine;

public static class InteractionSight
{
    private static readonly RaycastHit[] Buffer = new RaycastHit[8];
    private const float SurfaceMargin = 0.05f;

    public static bool IsBlocked(Vector3 origin, Collider targetCollider, IInteractable target, LayerMask occluderMask)
    {
        if (occluderMask.value == 0 || targetCollider == null) return false;

        Vector3 point = targetCollider.bounds.ClosestPoint(origin);
        Vector3 delta = point - origin;
        float distance = delta.magnitude;
        if (distance <= SurfaceMargin) return false;

        int count = Physics.RaycastNonAlloc(
            origin,
            delta / distance,
            Buffer,
            distance - SurfaceMargin,
            occluderMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider hit = Buffer[i].collider;
            if (hit == targetCollider) continue;
            if (target != null && ReferenceEquals(hit.GetComponentInParent<IInteractable>(), target)) continue;
            return true;
        }

        return false;
    }
}