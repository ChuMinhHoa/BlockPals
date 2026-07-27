using UnityEngine;

public partial class UnitCube
{
    [Header("Movement")]
    
    [Range(0, 10f)]
    [SerializeField] public float moveSpeed;
    
    [Range(0, 10f)]
    [SerializeField] public float rotateSpeed;

    private Vector3 targetPosition;
    
    public UnitCube WithTarget(Vector3 target)
    {
        targetPosition = target;
        return this;
    }

    public void StartMove()
    {
        
    }
}