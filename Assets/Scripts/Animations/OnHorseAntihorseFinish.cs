using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHorseAntihorseFinish : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animator->Mesh->Troop
        var go = animator.transform.parent.gameObject;

        //Get the troopcontroller
        var controller = go.GetComponent<TroopController>();

        //Let the controller know all the animations have been played
        controller.HorseAntiHorseFinished();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Animator->Mesh->Troop
        var go = animator.transform.parent.gameObject;

        //Get the troopcontroller
        var controller = go.GetComponent<TroopController>();

        //Reset the defending property
        controller.IsDefending = false;
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
