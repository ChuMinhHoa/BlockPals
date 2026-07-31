using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class APathFinding : MonoBehaviour
{
    public List<Node> FindPath(Node start, Node end)
    {
        if (!TryResolveCoords(start, end, out var sx, out var sy, out var ex, out var ey))
            return new List<Node>();

        var indices = FindPathIndices(sx, sy, ex, ey, CancellationToken.None);
        return IndicesToNodes(indices);
    }

    public List<Vector3> FindPath(Vector3 from, Vector3 to)
    {
        if (Map.Instance == null)
            return new List<Vector3>();

        var start = Map.Instance.GetNearestNode(from);
        var end = Map.Instance.GetNearestNode(to);
        if (!TryResolveCoords(start, end, out var sx, out var sy, out var ex, out var ey))
            return new List<Vector3>();

        var indices = FindPathIndices(sx, sy, ex, ey, CancellationToken.None);
        return IndicesToPositions(indices);
    }

    public async UniTask<List<Node>> FindPathAsync(Node start, Node end, CancellationToken cancellationToken = default)
    {
        if (!TryResolveCoords(start, end, out var sx, out var sy, out var ex, out var ey))
            return new List<Node>();

        var indices = await FindPathIndicesAsync(sx, sy, ex, ey, cancellationToken);
        return IndicesToNodes(indices);
    }

    public async UniTask<List<Vector3>> FindPathAsync(Vector3 from, Vector3 to, CancellationToken cancellationToken = default)
    {
        if (Map.Instance == null)
            return new List<Vector3>();

        var start = Map.Instance.GetNearestNode(from);
        var end = Map.Instance.GetNearestNode(to);
        if (!TryResolveCoords(start, end, out var sx, out var sy, out var ex, out var ey))
            return new List<Vector3>();

        var indices = await FindPathIndicesAsync(sx, sy, ex, ey, cancellationToken);
        return IndicesToPositions(indices);
    }

    private bool TryResolveCoords(Node start, Node end, out int sx, out int sy, out int ex, out int ey)
    {
        sx = sy = ex = ey = 0;
        if (Map.Instance == null || start == null || end == null)
            return false;

        if (!Map.Instance.TryGetCoords(start, out sx, out sy))
            return false;

        if (!Map.Instance.TryGetCoords(end, out ex, out ey))
            return false;

        return start.able && end.able;
    }

    private List<int> FindPathIndices(int sx, int sy, int ex, int ey, CancellationToken cancellationToken)
    {
        var context = PathSearchContextPool.Rent();
        try
        {
            PrepareContext(context);
            var endIndex = context.ToIndex(ex, ey);
            if (!RunAStar(context, context.ToIndex(sx, sy), endIndex, cancellationToken))
                return new List<int>();

            return RebuildIndexPath(context, endIndex);
        }
        finally
        {
            PathSearchContextPool.Return(context);
        }
    }

    private async UniTask<List<int>> FindPathIndicesAsync(int sx, int sy, int ex, int ey, CancellationToken cancellationToken)
    {
        var context = PathSearchContextPool.Rent();
        try
        {
            PrepareContext(context);
            var startIndex = context.ToIndex(sx, sy);
            var endIndex = context.ToIndex(ex, ey);

            var found = await UniTask.RunOnThreadPool(
                () => RunAStar(context, startIndex, endIndex, cancellationToken),
                cancellationToken: cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);

            return !found ? new List<int>() : RebuildIndexPath(context, endIndex);
        }
        catch (OperationCanceledException)
        {
            return new List<int>();
        }
        finally
        {
            PathSearchContextPool.Return(context);
        }
    }

    private void PrepareContext(PathSearchContext context)
    {
        context.Reset(Map.Instance.mapSize);
        Map.Instance.BuildWalkableSnapshot(context.Walkable);
    }

    private static bool RunAStar(
        PathSearchContext context,
        int startIndex,
        int endIndex,
        CancellationToken cancellationToken)
    {
        var walkable = context.Walkable;
        if (!walkable[startIndex] || !walkable[endIndex])
            return false;

        var mapSize = context.MapSize;
        if (startIndex == endIndex)
        {
            context.GCost[startIndex] = 0;
            context.Parent[startIndex] = -1;
            return true;
        }

        var endX = endIndex / mapSize;
        var endY = endIndex % mapSize;

        context.GCost[startIndex] = 0;
        context.Status[startIndex] = PathNodeStatus.Open;
        var startH = Heuristic(startIndex / mapSize, startIndex % mapSize, endX, endY);
        context.Open.Push(startIndex, startH, startH);

        while (context.Open.TryPop(out var current, out var poppedF, out _))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentG = context.GCost[current];
            if (currentG == int.MaxValue)
                continue;

            var cx = current / mapSize;
            var cy = current % mapSize;
            var currentH = Heuristic(cx, cy, endX, endY);
            if (poppedF != currentG + currentH)
                continue;

            if (context.Status[current] == PathNodeStatus.Closed)
                continue;

            context.Status[current] = PathNodeStatus.Closed;
            if (current == endIndex)
                return true;

            FillNeighbors(cx, cy, mapSize, walkable, context.Neighbors);
            for (var i = 0; i < context.Neighbors.Count; i++)
            {
                var neighbor = context.Neighbors[i];
                var nIndex = neighbor.X * mapSize + neighbor.Y;
                if (context.Status[nIndex] == PathNodeStatus.Closed)
                    continue;

                var tentativeG = currentG + neighbor.Cost;
                if (tentativeG >= context.GCost[nIndex])
                    continue;

                context.Parent[nIndex] = current;
                context.GCost[nIndex] = tentativeG;
                var h = Heuristic(neighbor.X, neighbor.Y, endX, endY);
                context.Status[nIndex] = PathNodeStatus.Open;
                context.Open.Push(nIndex, tentativeG + h, h);
            }
        }

        return false;
    }

    private static void FillNeighbors(int x, int y, int mapSize, bool[] walkable, List<PathNeighbor> buffer)
    {
        buffer.Clear();
        var offsets = Node.DefaultSubNodeOffsets;
        for (var i = 0; i < offsets.Length; i++)
        {
            var ox = offsets[i].X;
            var oy = offsets[i].Y;
            var nx = x + ox;
            var ny = y + oy;
            if (nx < 0 || ny < 0 || nx >= mapSize || ny >= mapSize)
                continue;

            var nIndex = nx * mapSize + ny;
            if (!walkable[nIndex])
                continue;

            var isDiagonal = ox != 0 && oy != 0;
            if (isDiagonal)
            {
                var ax = x + ox;
                var by = y + oy;
                if (ax < 0 || ax >= mapSize || by < 0 || by >= mapSize)
                    continue;
                if (!walkable[ax * mapSize + y] || !walkable[x * mapSize + by])
                    continue;
            }

            buffer.Add(new PathNeighbor(nx, ny, isDiagonal ? PathCosts.Diagonal : PathCosts.Straight));
        }
    }

    private static int Heuristic(int x, int y, int endX, int endY)
    {
        var dx = Math.Abs(x - endX);
        var dy = Math.Abs(y - endY);
        var min = Math.Min(dx, dy);
        return PathCosts.Straight * (dx + dy) + (PathCosts.Diagonal - 2 * PathCosts.Straight) * min;
    }

    private static List<int> RebuildIndexPath(PathSearchContext context, int endIndex)
    {
        var path = new List<int>();
        var current = endIndex;
        while (current >= 0)
        {
            path.Add(current);
            current = context.Parent[current];
        }

        path.Reverse();
        return path;
    }

    private List<Node> IndicesToNodes(List<int> indices)
    {
        var result = new List<Node>(indices.Count);
        for (var i = 0; i < indices.Count; i++)
        {
            Map.Instance.FromIndex(indices[i], out var x, out var y);
            if (Map.Instance.TryGetNode(x, y, out var node))
                result.Add(node);
        }

        return result;
    }

    private List<Vector3> IndicesToPositions(List<int> indices)
    {
        var result = new List<Vector3>(indices.Count);
        for (var i = 0; i < indices.Count; i++)
        {
            Map.Instance.FromIndex(indices[i], out var x, out var y);
            if (Map.Instance.TryGetNode(x, y, out var node))
                result.Add(node.transform.position);
        }

        return result;
    }
}
