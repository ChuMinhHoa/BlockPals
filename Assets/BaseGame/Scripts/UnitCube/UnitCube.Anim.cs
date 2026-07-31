using UnityEngine;

public partial class UnitCube
{
    [SerializeField] private Animator animator;

    private void ChangeAnim(string animName)
    {
        animator.Play(animName);
    }
}
