using System;
using System.Collections.Generic;

public static class PathNodeStatus
{
    public const byte None = 0;
    public const byte Open = 1;
    public const byte Closed = 2;
}

public sealed class PathMinHeap
{
    private int[] indices = Array.Empty<int>();
    private int[] fCosts = Array.Empty<int>();
    private int[] hCosts = Array.Empty<int>();
    private int count;

    public int Count => count;

    public void Clear() => count = 0;

    public void EnsureCapacity(int capacity)
    {
        if (indices.Length >= capacity)
            return;

        Array.Resize(ref indices, capacity);
        Array.Resize(ref fCosts, capacity);
        Array.Resize(ref hCosts, capacity);
    }

    public void Push(int index, int fCost, int hCost)
    {
        EnsureCapacity(count + 1);
        indices[count] = index;
        fCosts[count] = fCost;
        hCosts[count] = hCost;
        SiftUp(count);
        count++;
    }

    public bool TryPop(out int index, out int fCost, out int hCost)
    {
        if (count == 0)
        {
            index = 0;
            fCost = 0;
            hCost = 0;
            return false;
        }

        index = indices[0];
        fCost = fCosts[0];
        hCost = hCosts[0];

        count--;
        if (count > 0)
        {
            indices[0] = indices[count];
            fCosts[0] = fCosts[count];
            hCosts[0] = hCosts[count];
            SiftDown(0);
        }

        return true;
    }

    private void SiftUp(int i)
    {
        while (i > 0)
        {
            var parent = (i - 1) >> 1;
            if (!IsBetter(i, parent))
                break;

            Swap(i, parent);
            i = parent;
        }
    }

    private void SiftDown(int i)
    {
        while (true)
        {
            var left = (i << 1) + 1;
            if (left >= count)
                break;

            var right = left + 1;
            var best = right < count && IsBetter(right, left) ? right : left;
            if (!IsBetter(best, i))
                break;

            Swap(i, best);
            i = best;
        }
    }

    private bool IsBetter(int a, int b)
    {
        if (fCosts[a] != fCosts[b])
            return fCosts[a] < fCosts[b];
        return hCosts[a] < hCosts[b];
    }

    private void Swap(int a, int b)
    {
        (indices[a], indices[b]) = (indices[b], indices[a]);
        (fCosts[a], fCosts[b]) = (fCosts[b], fCosts[a]);
        (hCosts[a], hCosts[b]) = (hCosts[b], hCosts[a]);
    }
}

public sealed class PathSearchContext
{
    public int MapSize { get; private set; }
    public int CellCount => MapSize * MapSize;

    public int[] GCost = Array.Empty<int>();
    public int[] Parent = Array.Empty<int>();
    public byte[] Status = Array.Empty<byte>();
    public bool[] Walkable = Array.Empty<bool>();
    public readonly PathMinHeap Open = new();
    public readonly List<PathNeighbor> Neighbors = new(8);

    public void Reset(int mapSize)
    {
        MapSize = mapSize;
        var cellCount = mapSize * mapSize;
        EnsureCapacity(cellCount);

        Array.Fill(GCost, int.MaxValue, 0, cellCount);
        Array.Fill(Parent, -1, 0, cellCount);
        Array.Clear(Status, 0, cellCount);
        Array.Clear(Walkable, 0, cellCount);
        Neighbors.Clear();
        Open.Clear();
        Open.EnsureCapacity(cellCount);
    }

    private void EnsureCapacity(int cellCount)
    {
        if (GCost.Length < cellCount)
        {
            GCost = new int[cellCount];
            Parent = new int[cellCount];
            Status = new byte[cellCount];
            Walkable = new bool[cellCount];
        }
    }

    public int ToIndex(int x, int y) => x * MapSize + y;

    public void FromIndex(int index, out int x, out int y)
    {
        x = index / MapSize;
        y = index % MapSize;
    }
}

public static class PathSearchContextPool
{
    private static readonly object Gate = new();
    private static readonly System.Collections.Generic.Stack<PathSearchContext> Pool = new();

    public static PathSearchContext Rent()
    {
        lock (Gate)
        {
            return Pool.Count > 0 ? Pool.Pop() : new PathSearchContext();
        }
    }

    public static void Return(PathSearchContext context)
    {
        if (context == null)
            return;

        context.Open.Clear();
        lock (Gate)
        {
            Pool.Push(context);
        }
    }
}
