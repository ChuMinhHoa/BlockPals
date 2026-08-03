using Cysharp.Threading.Tasks;
using LitMotion;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class UnitAnimation : MonoBehaviour
{
    #region Scale Animation

    public AnimationCurve curveScaleX;
    public AnimationCurve curveScaleY;
    public AnimationCurve curveScaleZ;
    public float timeScale = 1f;
    private MotionHandle motionHandleScale;
    
    public bool IsHaveScaleAnim()
    {
        return motionHandleScale.IsPlaying();
    }
    
    [Button]
    public async UniTask PlayScaleAnimation(float scaleValue = 1f)
    {
        if (motionHandleScale.IsPlaying())
        {
            motionHandleScale.TryCancel();
        }
        
        if (!this || !transform)
            return;
        
        motionHandleScale = LMotion.Create(0f, 1f, timeScale).Bind(x =>
        {
            if (!this || !transform) return;
            var scaleX = scaleValue * curveScaleX.Evaluate(x);
            var scaleY = scaleValue * curveScaleY.Evaluate(x);
            var scaleZ = scaleValue * curveScaleZ.Evaluate(x);
            transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }).AddTo(this);
        await motionHandleScale;
    }

    #endregion

    #region Move Animation
    
    public AnimationCurve curveMoveX;
    public AnimationCurve curveMoveY;
    public AnimationCurve curveMoveZ;

    public AnimationCurve curveMove;
    
    public float timeMove = 1f;
    private MotionHandle motionHandleMoveX;
    private MotionHandle motionHandleMoveY;
    private MotionHandle motionHandleMoveZ;
    
    public float2 magnitudeX;
    public float2 magnitudeY;
    public float2 magnitudeZ;
    
    public bool fixMagnitudeX;
    public bool fixMagnitudeY;
    public bool fixMagnitudeZ;

    [Button]
    public async UniTask PlayMoveAnim(Vector3 targetPos)
    {
        if (motionHandleMoveZ.IsActive())
        {
            motionHandleMoveX.TryCancel();
            motionHandleMoveY.TryCancel();
            motionHandleMoveZ.TryCancel();
        }

        if (!this || !transform)
            return;
        
        var currentX = transform.position.x;
        var currentY = transform.position.y;
        var currentZ = transform.position.z;
      
        var targetX = targetPos.x;
        var targetY = targetPos.y;
        var targetZ = targetPos.z;

        var mx = fixMagnitudeX ? magnitudeX.x : Random.Range(magnitudeX.x, magnitudeX.y) * (Random.Range(0, 2) == 0 ? -1 : 1);
        var my = fixMagnitudeY ? magnitudeY.x :Random.Range(magnitudeY.x, magnitudeY.y) * (Random.Range(0, 2) == 0 ? -1 : 1);
        var mz = fixMagnitudeZ ? magnitudeZ.x :Random.Range(magnitudeZ.x, magnitudeZ.y) * (Random.Range(0, 2) == 0 ? -1 : 1);
      
        var currentPos = transform.position;

        motionHandleMoveX = LMotion.Create(0f, 1f, timeMove).WithEase(curveMove).Bind(t =>
        {
            if (!this || !transform) return;
            var x = Mathf.Lerp(currentX, targetX, t);
            var evaluateX = curveMoveX.Evaluate(t) * mx;
            currentPos.x = x + evaluateX;
            transform.position = currentPos;
        }).AddTo(this);
        
        motionHandleMoveY = LMotion.Create(0f, 1f, timeMove).WithEase(curveMove).Bind(t =>
        {
            if (!this || !transform) return;
            var y = Mathf.Lerp(currentY, targetY, t);
            var evaluateY = curveMoveY.Evaluate(t) * my;
            currentPos.y = y + evaluateY;
            transform.position = currentPos;
        }).AddTo(this);
      
        motionHandleMoveZ = LMotion.Create(0f, 1f, timeMove).WithEase(curveMove).Bind(t =>
        {
            if (!this || !transform) return;
            var z = Mathf.Lerp(currentZ, targetZ, t);
            var evaluateZ = curveMoveZ.Evaluate(t) * mz;
            currentPos.z = z + evaluateZ;
            transform.position = currentPos;
        }).AddTo(this);

        await motionHandleMoveZ;
    }
    
    
    public async UniTask PlayMoveAnimLocal(Vector3 targetPos)
    {
        if (motionHandleMoveZ.IsActive())
        {
            motionHandleMoveX.TryCancel();
            motionHandleMoveY.TryCancel();
            motionHandleMoveZ.TryCancel();
        }
        
        if (!this || !transform)
            return;
        
        var currentX = transform.localPosition.x;
        var currentY = transform.localPosition.y;
        var currentZ = transform.localPosition.z;
      
        var targetX = targetPos.x;
        var targetY = targetPos.y;
        var targetZ = targetPos.z;

        var mx = Random.Range(magnitudeX.x, magnitudeX.y) * (Random.Range(0, 2) == 0 ? -1 : 1);
        var my = Random.Range(magnitudeY.x, magnitudeY.y) * (Random.Range(0, 2) == 0 ? -1 : 1);
        var mz = Random.Range(magnitudeZ.x, magnitudeZ.y) * (Random.Range(0, 2) == 0 ? -1 : 1);
      
        var currentPos = transform.position;

        motionHandleMoveX = LMotion.Create(0f, 1f, timeMove).WithEase(curveMove).Bind(t =>
        {
            if (!this || !transform) return;
            var x = Mathf.Lerp(currentX, targetX, t);
            var evaluateX = curveMoveX.Evaluate(t) * mx;
            currentPos.x = x + evaluateX;
            transform.localPosition = currentPos;
        }).AddTo(this);
        
        motionHandleMoveY = LMotion.Create(0f, 1f, timeMove).WithEase(curveMove).Bind(t =>
        {
            if (!this || !transform) return;
            var y = Mathf.Lerp(currentY, targetY, t);
            var evaluateY = curveMoveY.Evaluate(t) * my;
            currentPos.y = y + evaluateY;
            transform.localPosition = currentPos;
        }).AddTo(this);
      
        motionHandleMoveZ = LMotion.Create(0f, 1f, timeMove).WithEase(curveMove).Bind(t =>
        {
            if (!this || !transform) return;
            var z = Mathf.Lerp(currentZ, targetZ, t);
            var evaluateZ = curveMoveZ.Evaluate(t) * mz;
            currentPos.z = z + evaluateZ;
            transform.localPosition = currentPos;
        }).AddTo(this);

        await motionHandleMoveZ;
    }

    #endregion

    public void ClearAnim()
    {
        if (motionHandleMoveZ.IsActive())
        {
            motionHandleMoveX.TryCancel();
            motionHandleMoveY.TryCancel();
            motionHandleMoveZ.TryCancel();
        }
        
        if (motionHandleScale.IsActive())
            motionHandleScale.TryCancel();
        
    }
}
