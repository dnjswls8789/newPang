using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBattleAnimator : StateMachineBehaviour
{
    CharacterBattle cb;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Idle", true);
        animator.SetInteger("Attack", 0);

        //if (cb == null)
        //{
        //    cb = animator.GetComponent<CharacterBattle>();
        //}

        //if (cb == null) return;

        //cb.isIdle = true;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Idle", false);
        //if (cb == null)
        //{
        //    cb = animator.GetComponent<CharacterBattle>();
        //}

        //if (cb == null) return;

        //cb.isIdle = false;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
