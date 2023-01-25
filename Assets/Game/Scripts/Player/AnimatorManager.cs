using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Animator animator;

    [Header("Animation")]
    private readonly int FreeLookSpeedHash = Animator.StringToHash("FreeLookSpeed");
    private readonly int FreeLookBlendTreeHash = Animator.StringToHash("FreeLookBlendTree");
    private const float AnimatorDampTime = 0.1f;
    private const float CrossFadeDuration = 0.1f;

     private readonly int AttackHash = Animator.StringToHash("Attack");

    private void Update() 
    {
        if(inputReader.MovementValue == Vector2.zero) 
        {
            animator.SetFloat(FreeLookSpeedHash, 0, AnimatorDampTime, Time.deltaTime);
            return;
        }

        animator.SetFloat(FreeLookSpeedHash, 1, AnimatorDampTime, Time.deltaTime);
    }

    public void SetOnAttackAnimation()
    {
        animator.CrossFadeInFixedTime(AttackHash, CrossFadeDuration);
    }

    public void SetFreeLookAnimation()
    {
        animator.CrossFadeInFixedTime(FreeLookBlendTreeHash, CrossFadeDuration);
    }

}
