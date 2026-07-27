using System;
using Sirenix.OdinInspector;
using UnityEngine;



public class Node : MonoBehaviour
{
    
    private static readonly SubNodeOffset[] DefaultSubNodeOffsets =
    {
        new(0, 1),
        new(0, -1),
        new(-1, 0),
        new(1, 0),
        new(1, 1),
        new(1, -1),
        new(-1, 1),
        new(-1, -1),
    };
    
    public Reactive<bool> able = new(true);
}

[Serializable]
public readonly struct SubNodeOffset : IEquatable<SubNodeOffset>
{
    [SerializeField] private readonly byte packed;

    public SubNodeOffset(sbyte x, sbyte y)
    {
        packed = Pack(x, y);
    }

    public byte Packed => packed;
    [ShowInInspector] public sbyte X => UnpackX(packed);
    [ShowInInspector] public sbyte Y => UnpackY(packed);

    public static SubNodeOffset FromPacked(byte value) => new(value);

    private SubNodeOffset(byte packedValue)
    {
        packed = packedValue;
    }

    public static byte Pack(sbyte x, sbyte y)
    {
        return (byte)(((x + 1) << 4) | (y + 1));
    }

    public static sbyte UnpackX(byte value) => (sbyte)(((value >> 4) & 0x03) - 1);

    public static sbyte UnpackY(byte value) => (sbyte)((value & 0x03) - 1);

    public Vector2 ToOffset(float step = 0.5f) => new(X * step, Y * step);

    public bool Equals(SubNodeOffset other) => packed == other.packed;

    public override bool Equals(object obj) => obj is SubNodeOffset other && Equals(other);

    public override int GetHashCode() => packed;
}
