using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetInteractingBool : StateMachineBehaviour
{
    public string isGroundInteractingBool;
    public string isAirInteractingBool;
    public bool isGroundInteractingStatus;
    public bool isAirInteractingStatus;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool(isGroundInteractingBool, isGroundInteractingStatus);
        animator.SetBool(isAirInteractingBool, isAirInteractingStatus);
    }
}
