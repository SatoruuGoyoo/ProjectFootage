//using UnityEngine;

//public class CameraDolly : MonoBehaviour
//{
//    [Header("Waypoints")]
//    public Transform[] waypoints;

//    [Header("Distancia al player")]
//    public float distanceAhead = 3f;

//    [Header("Altura")]
//    public float heightOffset = 1f;

//    private Transform player;

//    private void Start()
//    {
//        var go = GameObject.FindGameObjectWithTag("Player");
//        if (go != null) player = go.transform;
//    }

//    private void LateUpdate()
//    {
//        if (waypoints == null || waypoints.Length < 2 || player == null) return;

//        float playerT = GetTOnPath(player.position);
//        float camT = GetTAhead(playerT, distanceAhead);
//        Vector3 camPos = GetPositionOnPath(camT) + Vector3.up * heightOffset;

//        transform.position = camPos;
//    }

//    private float GetTOnPath(Vector3 pos)
//    {
//        float totalLength = GetPathLength();
//        float bestT = 0f;
//        float minDist = float.MaxValue;
//        float accumulated = 0f;

//        for (int i = 0; i < waypoints.Length - 1; i++)
//        {
//            Vector3 a = waypoints[i].position;
//            Vector3 b = waypoints[i + 1].position;
//            float segLen = Vector3.Distance(a, b);
//            float localT = Mathf.Clamp01(Vector3.Dot(pos - a, b - a) / (b - a).sqrMagnitude);
//            Vector3 closest = Vector3.Lerp(a, b, localT);
//            float dist = Vector3.Distance(pos, closest);

//            if (dist < minDist)
//            {
//                minDist = dist;
//                bestT = (accumulated + localT * segLen) / totalLength;
//            }
//            accumulated += segLen;
//        }
//        return bestT;
//    }

//    private float GetTAhead(float playerT, float distance)
//    {
//        float totalLength = GetPathLength();
//        return Mathf.Clamp01(playerT + (distance / totalLength));
//    }

//    private Vector3 GetPositionOnPath(float t)
//    {
//        float totalLength = GetPathLength();
//        float target = t * totalLength;
//        float accumulated = 0f;

//        for (int i = 0; i < waypoints.Length - 1; i++)
//        {
//            float segLen = Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
//            if (accumulated + segLen >= target)
//            {
//                float localT = (target - accumulated) / segLen;
//                return Vector3.Lerp(waypoints[i].position, waypoints[i + 1].position, localT);
//            }
//            accumulated += segLen;
//        }
//        return waypoints[waypoints.Length - 1].position;
//    }

//    private float GetPathLength()
//    {
//        float length = 0f;
//        for (int i = 0; i < waypoints.Length - 1; i++)
//            length += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
//        return length;
//    }

//#if UNITY_EDITOR
//    private void OnDrawGizmos()
//    {
//        if (waypoints == null || waypoints.Length < 2) return;

//        Gizmos.color = Color.yellow;
//        for (int i = 0; i < waypoints.Length - 1; i++)
//        {
//            if (waypoints[i] == null || waypoints[i + 1] == null) continue;
//            Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
//        }

//        Gizmos.color = Color.cyan;
//        foreach (var wp in waypoints)
//        {
//            if (wp == null) continue;
//            Gizmos.DrawSphere(wp.position, 0.15f);
//        }
//    }
//#endif
//}