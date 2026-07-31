using System.Collections.Generic;
using Sirenix.OdinInspector;
using TW.Utility.DesignPattern;
using UnityEngine;

public class Map : Singleton<Map>
{
    public int mapSize;

    [SerializeField] private Node nodePref;
    [SerializeField] private Node[,] mapNodes;

    private readonly Dictionary<Node, Vector2Int> nodeCoords = new();

    private void Awake()
    {
        SpawnMapNode();
    }

    [Button]
    private void SpawnMapNode()
    {
        ClearMapNodes();
        mapNodes = new Node[mapSize, mapSize];
        nodeCoords.Clear();

        for (var x = 0; x < mapSize; x++)
        {
            for (var y = 0; y < mapSize; y++)
            {
                var pos = GetPosNode(x, y);
                var node = Instantiate(nodePref, pos, Quaternion.identity, transform);
                mapNodes[x, y] = node;
                nodeCoords[node] = new Vector2Int(x, y);
            }
        }
    }

    private void ClearMapNodes()
    {
        if (mapNodes == null)
            return;

        for (var x = 0; x < mapNodes.GetLength(0); x++)
        {
            for (var y = 0; y < mapNodes.GetLength(1); y++)
            {
                var node = mapNodes[x, y];
                if (node == null)
                    continue;

                if (Application.isPlaying)
                    Destroy(node.gameObject);
                else
                    DestroyImmediate(node.gameObject);
            }
        }
    }

    private Vector3 GetPosNode(int i, int j)
    {
        var offset = (mapSize - 1) * 0.5f;
        return new Vector3(i - offset, 0f, j - offset);
    }

    public bool TryGetNode(int x, int y, out Node node)
    {
        if (mapNodes == null || x < 0 || y < 0 || x >= mapSize || y >= mapSize)
        {
            node = null;
            return false;
        }

        node = mapNodes[x, y];
        return node != null;
    }

    public Node GetNode(int x, int y)
    {
        return TryGetNode(x, y, out var node) ? node : null;
    }

    public bool TryGetCoords(Node node, out int x, out int y)
    {
        if (node != null && nodeCoords.TryGetValue(node, out var coords))
        {
            x = coords.x;
            y = coords.y;
            return true;
        }

        x = 0;
        y = 0;
        return false;
    }

    public Node GetNode(Vector3 worldPos) => GetNearestNode(worldPos);

    public Node GetNearestNode(Vector3 worldPos)
    {
        if (mapNodes == null || mapSize <= 0)
            return null;

        var offset = (mapSize - 1) * 0.5f;
        var x = Mathf.Clamp(Mathf.RoundToInt(worldPos.x + offset), 0, mapSize - 1);
        var y = Mathf.Clamp(Mathf.RoundToInt(worldPos.z + offset), 0, mapSize - 1);
        return mapNodes[x, y];
    }

    public void BuildWalkableSnapshot(bool[] walkable)
    {
        var cellCount = mapSize * mapSize;
        if (walkable == null || walkable.Length < cellCount || mapNodes == null)
            return;

        for (var x = 0; x < mapSize; x++)
        {
            for (var y = 0; y < mapSize; y++)
            {
                var node = mapNodes[x, y];
                walkable[ToIndex(x, y)] = node != null && node.able;
            }
        }
    }

    public void GetNeighbors(int x, int y, bool[] walkable, List<PathNeighbor> buffer)
    {
        buffer.Clear();
        if (!IsInBounds(x, y))
            return;

        var offsets = Node.DefaultSubNodeOffsets;
        for (var i = 0; i < offsets.Length; i++)
        {
            var ox = offsets[i].X;
            var oy = offsets[i].Y;
            var nx = x + ox;
            var ny = y + oy;
            if (!IsInBounds(nx, ny))
                continue;

            var nIndex = ToIndex(nx, ny);
            if (!walkable[nIndex])
                continue;

            var isDiagonal = ox != 0 && oy != 0;
            if (isDiagonal)
            {
                var sideA = ToIndex(x + ox, y);
                var sideB = ToIndex(x, y + oy);
                if (!IsInBounds(x + ox, y) || !IsInBounds(x, y + oy) || !walkable[sideA] || !walkable[sideB])
                    continue;
            }

            buffer.Add(new PathNeighbor(nx, ny, isDiagonal ? PathCosts.Diagonal : PathCosts.Straight));
        }
    }

    public int ToIndex(int x, int y) => x * mapSize + y;

    public void FromIndex(int index, out int x, out int y)
    {
        x = index / mapSize;
        y = index % mapSize;
    }

    public bool IsInBounds(int x, int y) => x >= 0 && y >= 0 && x < mapSize && y < mapSize;
}

public readonly struct PathNeighbor
{
    public readonly int X;
    public readonly int Y;
    public readonly int Cost;

    public PathNeighbor(int x, int y, int cost)
    {
        X = x;
        Y = y;
        Cost = cost;
    }
}

public static class PathCosts
{
    public const int Straight = 10;
    public const int Diagonal = 14;
}
