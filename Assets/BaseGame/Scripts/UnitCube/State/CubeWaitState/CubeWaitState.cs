using System.Threading;
using Cysharp.Threading.Tasks;
using TW.Utility.DesignPattern.UniTaskState;
using UnityEngine;

public class CubeWaitState : IState
{
    public interface IUnitCubeHandle
    {
        public UniTask CubeWaitStateEnter(CancellationToken ct);
        public UniTask CubeWaitStateUpdate(CancellationToken ct);
        public UniTask CubeWaitStateExit(CancellationToken ct);
    }

    public IUnitCubeHandle handle;

    public CubeWaitState(IUnitCubeHandle handle)
    {
        this.handle = handle;
    }

    public UniTask OnEnter(CancellationToken ct)
    {
        return handle.CubeWaitStateEnter(ct);
    }

    public UniTask OnUpdate(CancellationToken ct)
    {
        return handle.CubeWaitStateUpdate(ct);
    }

    public UniTask OnExit(CancellationToken ct)
    {
        return handle.CubeWaitStateExit(ct);
    }
}

public partial class UnitCube : CubeWaitState.IUnitCubeHandle
{
    private CubeWaitState cubeWaitStateCache;
    private CubeWaitState CubeWaitState => cubeWaitStateCache ?? new CubeWaitState(this);
    
    public UniTask CubeWaitStateEnter(CancellationToken ct)
    {
        ChangeStatus(UnitCubeStatus.Waiting);
        ChangeAnim("Wait");
        return UniTask.CompletedTask;  
    }

    public UniTask CubeWaitStateUpdate(CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }

    public UniTask CubeWaitStateExit(CancellationToken ct)
    {
        return UniTask.CompletedTask;
    }
}
