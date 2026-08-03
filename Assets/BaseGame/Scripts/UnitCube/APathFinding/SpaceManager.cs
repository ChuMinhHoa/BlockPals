using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using TW.Utility.DesignPattern;
using UnityEngine;

public class SpaceManager : Singleton<SpaceManager>
{
    public List<SlotSpace> slotSpaces = new();

    public List<UnitCubeAction> unitCubeActions = new();

    private bool onGetSpaceSlot = false;

    private void Start()
    {
        this.UpdateAsObservable().Subscribe(OnUpdateForAction).AddTo(this);
    }

    private void OnUpdateForAction(Unit _)
    {
        UpdateForAction().Forget();
    }

    private async UniTask UpdateForAction()
    {
        while (unitCubeActions.Count > 0)
        {
            await UniTask.WaitUntil(() => !onGetSpaceSlot);
            GetSpaceSlot(unitCubeActions[0].callBack, unitCubeActions[0].unit);
            unitCubeActions.Remove(unitCubeActions[0]);
        }
    }

    public void RegisterClickOnCube(in UnitCube unitCube, in Action<SlotSpace> callBack)
    {
        for (var i = 0; i < unitCubeActions.Count; i++)
        {
            if (unitCubeActions[i].unit != unitCube) continue;
            unitCubeActions.Remove(unitCubeActions[i]);
            CreateNewAction(unitCube, callBack);
            return;
        }

        CreateNewAction(unitCube, callBack);
    }

    private void CreateNewAction(in UnitCube unitCube, in Action<SlotSpace> callBack)
    {
        var e = new UnitCubeAction
        {
            unit = unitCube,
            callBack = callBack
        };
        unitCubeActions.Add(e);
    }

    private void GetSpaceSlot(Action<SlotSpace> callBack, UnitCube cube)
    {
        onGetSpaceSlot = true;
        var isFullSlot = IsFullSlot();
        if (isFullSlot)
        {
            callBack?.Invoke(null);
            return;
        }
        
        if (CheckSameColorWithOtherSlotSpace(cube.colorIndex,  out var colorIndex))
        {
            if (colorIndex >= slotSpaces.Count)
            {
                callBack?.Invoke(null);
                return;
            }
            MoveOtherCubeToNextSpace(colorIndex);
            slotSpaces[colorIndex].ChangeState(SlotSpaceState.Disable);
            callBack?.Invoke(slotSpaces[colorIndex]);
            return;
        }
        
        for (var i = 0; i < slotSpaces.Count; i++)
        {
            if (!slotSpaces[i].IsCanAble()) continue;
            slotSpaces[i].ChangeState(SlotSpaceState.Disable);
            callBack?.Invoke(slotSpaces[i]);
            return;
        }

        callBack?.Invoke(null);
    }

    private bool IsFullSlot()
    {
        for (var i = 0; i < slotSpaces.Count; i++)
        {
            if (slotSpaces[i].IsCanAble())
                return false;
        }

        return true;
    }

    private bool CheckSameColorWithOtherSlotSpace(int colorIndex, out int indexSame)
    {
        for (var i = slotSpaces.Count - 1; i >= 0; i--)
        {
            if (slotSpaces[i].unitCube==null)continue;
            if (slotSpaces[i].unitCube.colorIndex == colorIndex)
            {
                indexSame = i + 1;
                return true;
            }
        }

        indexSame = -1;
        return false;
    }

    public void OnGetSpaceSlotDone()
    {
        onGetSpaceSlot = false;
    }

    public void OnCheckSpaceSlot()
    {
        var indexCheck = 0;
        while (indexCheck < slotSpaces.Count - 2)
        {
            var isReadyCheck = IsReadyToCheck(slotSpaces[indexCheck].unitCube, slotSpaces[indexCheck + 1].unitCube,
                slotSpaces[indexCheck + 2].unitCube);

            var isSameColor = ThreeSameColor(slotSpaces[indexCheck].unitCube, slotSpaces[indexCheck + 1].unitCube,
                slotSpaces[indexCheck + 2].unitCube);

            if (!isReadyCheck || !isSameColor)
            {
                indexCheck++;
                continue;
            }

            var completeData = new CompleteActionData
            {
                unitCubes = new[]
                {
                    slotSpaces[indexCheck].unitCube, slotSpaces[indexCheck + 1].unitCube,
                    slotSpaces[indexCheck + 2].unitCube
                },
                posComplete = slotSpaces[indexCheck + 1].transform.position + new Vector3(0, 1, 0)
            };
            CompleteAction.Instance.OnComplete(completeData);
            indexCheck += 3;
        }
    }

    private bool ThreeSameColor(UnitCube a, UnitCube b, UnitCube c)
    {
        if (a == null || b == null || c == null) return false;
        return a.colorIndex == b.colorIndex && a.colorIndex == c.colorIndex;
    }

    private bool IsReadyToCheck(UnitCube a, UnitCube b, UnitCube c)
    {
        if (a == null || b == null || c == null) return false;
        return a.AbleToCheck() && b.AbleToCheck() && c.AbleToCheck();
    }

    public void SortUnitCubes()
    {
        for (var i = 0; i < slotSpaces.Count - 1; i++)
        {
            if (slotSpaces[i].unitCube != null) continue;
            
            var unitCube = TryGetOtherUnitCube(i+1);
            if (unitCube == null) continue;
            unitCube.mySlot.ChangeState(SlotSpaceState.Able);
            unitCube.mySlot.SetUnitCube(null);
            slotSpaces[i].ChangeState(SlotSpaceState.Disable);
            unitCube.ActionGetSpaceCallBack(slotSpaces[i]);
        }
    }

    private UnitCube TryGetOtherUnitCube(int index)
    {
        for (var i = index; i < slotSpaces.Count; i++)
        {
            if (slotSpaces[i].unitCube != null)
            {
                return slotSpaces[i].unitCube;
            }
        }
        return null;
    }

    private void MoveOtherCubeToNextSpace(int indexEnd)
    {
        var slotIndex = slotSpaces.Count - 2;
        while (slotIndex >= indexEnd)
        {
            if (slotSpaces[slotIndex].unitCube == null)
            {
                slotIndex--;
            }
            else
            {
                var isNextSlotFree = slotSpaces[slotIndex + 1].IsCanAble();
                if (!isNextSlotFree)
                {
                    slotIndex++;
                }
                else
                {
                    var cube = slotSpaces[slotIndex].unitCube;
                    slotSpaces[slotIndex].SetUnitCube(null);
                    slotSpaces[slotIndex].ChangeState(SlotSpaceState.Able);
                    slotSpaces[slotIndex + 1].ChangeState(SlotSpaceState.Disable);
                    cube.ActionGetSpaceCallBack(slotSpaces[slotIndex + 1]);
                    slotIndex--;
                } 
            }
        }
    }
}

[Serializable]
public class UnitCubeAction
{
    public UnitCube unit;
    public Action<SlotSpace> callBack;
}
