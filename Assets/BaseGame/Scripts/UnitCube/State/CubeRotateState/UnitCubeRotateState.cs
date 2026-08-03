using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TW.Utility.DesignPattern.UniTaskState;
using Unity.Android.Gradle;
using UnityEngine;

public class UnitCubeRotateState : IState
{
    
    public interface IUnitCubeHandle
    {
        UniTask UnitCubeRotateStateEnter(CancellationToken ct);
        UniTask UnitCubeRotateStateUpdate(CancellationToken ct);
        UniTask UnitCubeRotateStateExit(CancellationToken ct);
    }
    
    public IUnitCubeHandle handle;

    public UnitCubeRotateState(IUnitCubeHandle handle)
    {
        this.handle = handle;
    }

    public UniTask OnEnter(CancellationToken ct)
    {
        return handle.UnitCubeRotateStateEnter(ct);
    }

    public UniTask OnUpdate(CancellationToken ct)
    {
        return handle.UnitCubeRotateStateUpdate(ct);
    }

    public UniTask OnExit(CancellationToken ct)
    {
        return handle.UnitCubeRotateStateExit(ct);
    }
}

public partial class UnitCube : UnitCubeRotateState.IUnitCubeHandle
{
    private UnitCubeRotateState unitCubeRotateStateCache;
    private UnitCubeRotateState UnitCubeRotateState => unitCubeRotateStateCache ?? new UnitCubeRotateState(this);
    private bool rotateDone = false;
    private Vector3 directionForward;
    private Quaternion lookTarget;
    
    [Button]
    private void ChangeToRotateState()
    {
        stateMachine.RequestTransition(UnitCubeRotateState);
    }


    public UniTask UnitCubeRotateStateEnter(CancellationToken ct)
    {
        rotateDone = false;
        directionForward = Vector3.forward;
        directionForward.y = 0f;
        lookTarget =  Quaternion.LookRotation(directionForward);
        return UniTask.CompletedTask;
    }
    
    public UniTask UnitCubeRotateStateUpdate(CancellationToken ct)
    {
        if (directionForward.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, lookTarget, rotateSpeed * 100f * Time.deltaTime);
        }
        else
        {
            BackToIdleState();
            return UniTask.CompletedTask;
        }
        
        if (Quaternion.Dot(transform.rotation, lookTarget) >= 1f)
        {
            BackToIdleState();
        }

        return UniTask.CompletedTask;
    }
    
    public UniTask UnitCubeRotateStateExit(CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }

    private void BackToIdleState()
    {
        rotateDone = true;
        if (actionMoveCallBack != null)
        {
            actionMoveCallBack.Invoke();
            actionMoveCallBack = null;
        }
        stateMachine.RequestTransition(UnitCubeIdleState);
    }
}
