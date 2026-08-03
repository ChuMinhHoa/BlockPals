using LitMotion;
using TW.Utility.DesignPattern;
using UnityEngine;

public class CompleteAction : Singleton<CompleteAction>
{
    [SerializeField] private Transform trsEffect;
    public void OnComplete(CompleteActionData data)
    {
        for (var i = 0; i < data.unitCubes.Length; i++)
        {
            _ = data.unitCubes[i].OnComplete(data.posComplete, i == 1 ? ActionCallBack : null);
        }
        
        SpaceManager.Instance.SortUnitCubes();
    }

    private void ActionCallBack(Vector3 posComplete)
    {
        var effect = Instantiate(trsEffect);
        effect.position = posComplete;
    }
}

public struct CompleteActionData
{
    public UnitCube[] unitCubes;
    public Vector3 posComplete;
}
