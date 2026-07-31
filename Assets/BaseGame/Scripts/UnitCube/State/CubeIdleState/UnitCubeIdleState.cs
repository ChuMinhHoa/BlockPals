using System.Threading;
using Cysharp.Threading.Tasks;
using TW.Utility.DesignPattern.UniTaskState;
using UnityEngine;

public class UnitCubeIdleState : IState
{

    public UniTask OnEnter(CancellationToken ct)
    {
        return handle.UnitCubeIdleStateEnter(ct);
    }

    public UniTask OnUpdate(CancellationToken ct)
    {
        return handle.UnitCubeIdleStateUpdate(ct);
    }

    public UniTask OnExit(CancellationToken ct)
    {
        return handle.UnitCubeIdleStateExit(ct);
    }
    
    public interface IUnitCubeHandle
    {
        public UniTask UnitCubeIdleStateEnter(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
        public UniTask UnitCubeIdleStateUpdate(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
        public UniTask UnitCubeIdleStateExit(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
    }
    
    public IUnitCubeHandle handle;

    public UnitCubeIdleState(IUnitCubeHandle handle)
    {
        this.handle = handle;
    }
}


public partial class UnitCube : UnitCubeIdleState.IUnitCubeHandle
{
    private UnitCubeIdleState unitCubeIdleStateCache;
    private UnitCubeIdleState UnitCubeIdleState => unitCubeIdleStateCache ?? new UnitCubeIdleState(this);
    
    public UniTask UnitCubeIdleStateEnter(CancellationToken ct)
    {
        ChangeAnim("Idle");
        return UniTask.CompletedTask;
    }

    public UniTask UnitCubeIdleStateUpdate(CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }

    public UniTask UnitCubeIdleStateExit(CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }
}
