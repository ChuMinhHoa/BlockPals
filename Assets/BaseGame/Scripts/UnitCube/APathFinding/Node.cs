using System;
using Cysharp.Threading.Tasks;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
public class Node : MonoBehaviour
{
    
    public static readonly SubNodeOffset[] DefaultSubNodeOffsets =
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
    [SerializeField] private SpriteRenderer sprRenderer;
    [SerializeField] private Sprite[] graphics;

    private void Start()
    {
        able.Subscribe(ChangeAble).AddTo(this);
    }

    private void ChangeAble(bool ableChange)
    {
        sprRenderer.sprite = able? graphics[0] : graphics[1];
    }
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
