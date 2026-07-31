using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(APathFinding))]
public class UniCubePathFinding : MonoBehaviour
{
    private const float WaypointEpsilonSqr = 0.0001f;

    public List<Vector3> pathWaypoints = new();
    
    [SerializeField] private APathFinding pathFinding;

    public void GetPathWaypoints(Vector3 targetPosition)
    {
        pathWaypoints.Clear();

        var startPos = transform.position;
        var startNode = ResolveStartNode(startPos, targetPosition);
        var endNode = Map.Instance.GetNode(targetPosition);
        if (startNode == null || endNode == null)
            return;

        var path = pathFinding.FindPath(startNode, endNode);
        BuildSmoothWaypoints(path, startPos, targetPosition);
    }

    private static Node ResolveStartNode(Vector3 from, Vector3 to)
    {
        var map = Map.Instance;
        if (map == null)
            return null;

        var nearest = map.GetNode(from);
        if (nearest == null)
            return null;

        var toTarget = to - from;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < WaypointEpsilonSqr)
            return nearest;

        var toCenter = nearest.transform.position - from;
        toCenter.y = 0f;

        if (Vector3.Dot(toCenter, toTarget) < 0f)
        {
            var biasedPos = from + toTarget.normalized * 0.5f;
            var candidate = map.GetNode(biasedPos);
            if (candidate != null && candidate.able)
                return candidate;
        }

        return nearest;
    }

    private void BuildSmoothWaypoints(List<Node> path, Vector3 startPos, Vector3 endPos)
    {
        pathWaypoints.Clear();
        if (path == null || path.Count == 0 || Map.Instance == null)
            return;

        AddWaypoint(startPos);

        for (var i = 0; i < path.Count - 1; i++)
        {
            var current = path[i];
            var next = path[i + 1];
            if (!TryGetStep(current, next, out var dx, out var dy))
                continue;

            var offset = new SubNodeOffset((sbyte)dx, (sbyte)dy).ToOffset();
            var mid = current.transform.position + ToWorld(offset);
            AddWaypoint(mid);

            // Intermediate node: keep center on straight segments, skip on L-corners.
            var intermediateIndex = i + 1;
            if (intermediateIndex >= path.Count - 1)
                continue;

            if (!TryGetStep(next, path[intermediateIndex + 1], out var nextDx, out var nextDy))
                continue;

            var isCorner = dx != nextDx || dy != nextDy;
            if (!isCorner)
                AddWaypoint(next.transform.position);
        }

        AddWaypoint(endPos);
        PruneBacktrackingFromStart();
    }

    private static bool TryGetStep(Node from, Node to, out int dx, out int dy)
    {
        dx = 0;
        dy = 0;
        if (Map.Instance == null)
            return false;
        if (!Map.Instance.TryGetCoords(from, out var x0, out var y0))
            return false;
        if (!Map.Instance.TryGetCoords(to, out var x1, out var y1))
            return false;

        dx = x1 - x0;
        dy = y1 - y0;
        return true;
    }

    /// <summary>
    /// Drop early mids that sit behind the unit relative to the following waypoint.
    /// </summary>
    private void PruneBacktrackingFromStart()
    {
        while (pathWaypoints.Count >= 3)
        {
            var start = pathWaypoints[0];
            var mid = pathWaypoints[1];
            var next = pathWaypoints[2];

            var toMid = mid - start;
            var toNext = next - start;
            toMid.y = 0f;
            toNext.y = 0f;

            // Mid is behind / not progressing toward next → skip it.
            if (Vector3.Dot(toMid, toNext) > 0f && toMid.sqrMagnitude <= toNext.sqrMagnitude)
                break;

            pathWaypoints.RemoveAt(1);
        }
    }

    private void AddWaypoint(Vector3 point)
    {
        if (pathWaypoints.Count > 0)
        {
            var last = pathWaypoints[pathWaypoints.Count - 1];
            if ((last - point).sqrMagnitude < WaypointEpsilonSqr)
                return;
        }

        pathWaypoints.Add(point);
    }

    private static Vector3 ToWorld(Vector2 offset) => new(offset.x, 0f, offset.y);

    private void OnDrawGizmos()
    {
        if (pathWaypoints == null || pathWaypoints.Count == 0)
            return;

        for (var i = 0; i < pathWaypoints.Count; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pathWaypoints[i], 0.15f);

            if (i < pathWaypoints.Count - 1)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pathWaypoints[i], pathWaypoints[i + 1]);
            }
        }
    }
}
