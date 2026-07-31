using System;
using TW.Utility.DesignPattern.UniTaskState;
using UnityEngine;

public partial class UnitCube : MonoBehaviour
{
    private StateMachine stateMachine;

    private void Start()
    {
        stateMachine = new StateMachine();
        stateMachine.RequestTransition(UnitCubeIdleState);
        stateMachine.Run();
    }
}
