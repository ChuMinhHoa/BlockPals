using System;

[Serializable]
public class LevelData
{
    public UnitCubeData[] unitCubeData;
}

[Serializable]
public class UnitCubeData
{
    public int colorIndex;
    public int x;
    public int y;
}