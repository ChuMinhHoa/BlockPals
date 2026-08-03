using System;
using TW.Utility.DesignPattern.UniTaskState;
using UnityEngine;

public enum UnitCubeStatus
{
    None,
    Able,
    Waiting,
    MovingOut,
    ReadyCheck,
    Complete
}

public partial class UnitCube : MonoBehaviour, IClickAble
{
    private StateMachine stateMachine;
    [SerializeField] private UnitCubeStatus unitCubeStatus;
    public SlotSpace mySlot;
    public int colorIndex;

    private void Start()
    {
        stateMachine = new StateMachine();
        stateMachine.RequestTransition(CubeWaitState);
        stateMachine.Run();
    }

    private void ChangeStatus(UnitCubeStatus status) => unitCubeStatus = status;
    
    public void OnClick()
    {
        //Debug.Log("Unit cube on click");
        if (unitCubeStatus ==  UnitCubeStatus.Able)
        {
            SpaceManager.Instance.RegisterClickOnCube(this, ActionGetSpaceCallBack);
        }
    }

    public void ActionGetSpaceCallBack(SlotSpace slotSpace)
    {
        if (slotSpace != null)
        {
            //Debug.Log($"Slot space {slotSpace.transform.GetSiblingIndex()}");
            mySlot = slotSpace;
            mySlot.SetUnitCube(this);
            ChangeStatus(UnitCubeStatus.MovingOut);
            WithTarget(slotSpace.transform.position).WithActionMoveCallBack(SetReadyCheck).SwitchMoveState();
        }

        SpaceManager.Instance.OnGetSpaceSlotDone();
    }

    private void SetReadyCheck()
    {
        unitCubeStatus = UnitCubeStatus.ReadyCheck;
    }

    public bool AbleToCheck()
    {
        return unitCubeStatus == UnitCubeStatus.ReadyCheck;
    }

    public void SetSlot(SlotSpace slotSpace)
    {
        mySlot = slotSpace;
    }
}
