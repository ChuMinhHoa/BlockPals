using System;
using Sirenix.OdinInspector;
using UnityEngine;

public partial class UnitCube
{
    [Header("Movement")]
    
    [Range(0, 20f)]
    [SerializeField] public float moveSpeed;
    
    [Range(0, 10f)]
    [SerializeField] public float rotateSpeed;

    public Vector3 targetPosition;

    [SerializeField] private UniCubePathFinding uniCubePathFinding;
    
    private int currentPathIndex = 0;
    
    private Action actionMoveCallBack;
    
    
    [Button]
    public UnitCube WithTarget(Vector3 target)
    {
        targetPosition = target;
        return this;
    }

    [Button]
    public void GetPathWayPoints()
    {
        uniCubePathFinding.GetPathWaypoints(targetPosition);
    }

    public UnitCube WithActionMoveCallBack(Action action)
    {
        actionMoveCallBack = action;
        return this;
    }
}