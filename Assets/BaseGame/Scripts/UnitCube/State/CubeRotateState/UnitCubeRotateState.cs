using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TW.Utility.DesignPattern.UniTaskState;
using UnityEngine;

public class UnitCubeRotateState : IState
{

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
}

public partial class UnitCube : UnitCubeRotateState.IUnitCubeHandle
{
    private UnitCubeRotateState unitCubeRotateStateCache;
    private UnitCubeRotateState UnitCubeRotateState => unitCubeRotateStateCache ?? new UnitCubeRotateState(this);

    [Button]
    private void ChangeToRotateState()
    {
        stateMachine.RequestTransition(UnitCubeRotateState);
    }


    public UniTask UnitCubeRotateStateEnter(CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }
    
    public UniTask UnitCubeRotateStateUpdate(CancellationToken ct)
    {
        var dir = Vector3.forward;
        dir.y = 0f;
        Debug.Log(dir);
        if (dir.sqrMagnitude > 0.001f)
        {
            var look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, look, rotateSpeed * 100f * Time.deltaTime);
        }

        if (transform.eulerAngles == Vector3.zero)
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
        stateMachine.RequestTransition(UnitCubeIdleState);
    }
}
