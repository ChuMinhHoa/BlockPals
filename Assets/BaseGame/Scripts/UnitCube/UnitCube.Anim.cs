using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public partial class UnitCube
{
    [SerializeField] private Animator animator;
    [SerializeField] private UnitAnimation moveToCompleteAnimation;
    private void ChangeAnim(string animName)
    {
        animator.Play(animName);
    }

    public async UniTask OnComplete(Vector3 posComplete, Action<Vector3> callBack)
    {
        mySlot.ResetSlot();
        ChangeAnim("Complete");
        ChangeStatus(UnitCubeStatus.Complete);
        await moveToCompleteAnimation.PlayMoveAnim(posComplete);
        callBack?.Invoke(posComplete);
        gameObject.SetActive(false);
    }
}
