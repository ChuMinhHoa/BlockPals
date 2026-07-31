using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TW.Utility.DesignPattern.UniTaskState;
using UnityEngine;

public class UnitCubeMoveState : IState
{
    
    public interface IUnitCubeHandle
    {
        public UniTask UnitCubeMoveStateEnter(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
        public UniTask UnitCubeMoveStateUpdate(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
        public UniTask UnitCubeMoveStateExit(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
    }

    public IUnitCubeHandle handle;

    public UnitCubeMoveState(IUnitCubeHandle handle)
    {
        this.handle = handle;
    }

    public UniTask OnEnter(CancellationToken ct)
    {
        return handle.UnitCubeMoveStateEnter(ct);
    }

    public UniTask OnUpdate(CancellationToken ct)
    {
        return handle.UnitCubeMoveStateUpdate(ct);
    }

    public UniTask OnExit(CancellationToken ct)
    {
        return handle.UnitCubeMoveStateExit(ct);
    }
}

public partial class UnitCube : UnitCubeMoveState.IUnitCubeHandle
{
    private UnitCubeMoveState unitCubeMoveStateCache;
    private UnitCubeMoveState UnitCubeMoveState => unitCubeMoveStateCache ?? new UnitCubeMoveState(this);

    private Vector3 nextPoint;

    private bool onMove;

    

    [Button]
    public void SwitchMoveState()
    {
        stateMachine.RequestTransition(UnitCubeMoveState);
    }

    public UniTask UnitCubeMoveStateEnter(CancellationToken ct)
    {
        currentPathIndex = 0;
        GetPathWayPoints();
        ChangeAnim("Move");
        return UniTask.CompletedTask;
    }

    public UniTask UnitCubeMoveStateUpdate(CancellationToken ct)
    {
        if (currentPathIndex >= uniCubePathFinding.pathWaypoints.Count)
        {
            EndMoveState();
            return UniTask.CompletedTask;
        }
        
        nextPoint = uniCubePathFinding.pathWaypoints[currentPathIndex];
        
        transform.position = Vector3.MoveTowards(
            transform.position, nextPoint, moveSpeed * Time.deltaTime);
        var dir = transform.position - nextPoint;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            var look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look, rotateSpeed * 100f * Time.deltaTime);
        }
        
        if (Vector3.Distance(transform.position, nextPoint) < 0.001f)
            currentPathIndex++;
        
        return UniTask.CompletedTask;
    }

    public UniTask UnitCubeMoveStateExit(CancellationToken ct)
    {
        if (actionMoveCallBack != null)
        {
            actionMoveCallBack.Invoke();
            actionMoveCallBack = null;
        }
        return UniTask.CompletedTask;
    }

    private void EndMoveState()
    {
        stateMachine.RequestTransition(UnitCubeIdleState);
    }
}
