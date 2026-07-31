using System;
using R3;
using UnityEngine;

public enum SlotSpaceState
{
    None,
    Able,
    Disable
}

public class SlotSpace : MonoBehaviour
{
    public Reactive<SlotSpaceState> slotSpaceState = new();
    [SerializeField] private SlotSpaceAnim animController;

    private void Start()
    {
        slotSpaceState.Subscribe(ChangeSlotSpaceState).AddTo(this);
    }

    private void ChangeSlotSpaceState(SlotSpaceState state)
    {
        switch (state)
        {
            case SlotSpaceState.Able:
                AbleMode();
                break;
            case SlotSpaceState.Disable:
                DisableMode();
                break;
            case SlotSpaceState.None:
            default:
                break;
        }  
    }

    private void DisableMode()
    {
        animController.DisableAnim();
    }

    private void AbleMode()
    {
        animController.AbleAnim();
    }
}
