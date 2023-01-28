using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulationMenuOpenBehavior : StateMachineBehaviour
{
    private FoamDataManager _data;
    private int _hash_toolSelectedInt = Animator.StringToHash("ToolSelectedInt");


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data = GameObject.FindGameObjectWithTag("foamDM").GetComponent<FoamDataManager>();

        _data.ManiMenu.transform.position = _data.ActiveIndex.transform.position;
        _data.ManiMenu.SetActive(true);
        _data.CreateMenu.SetActive(false);

        _data.ManiMenuParent.InitiateIcons();
        _data.ManiMenuParent.RecordPalmPosInit(_data.ActivePalm.transform.position);

        Debug.Log("FOAMFILTER Mani Menu Open State entered");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        MenuRegion region = _data.ManiMenuParent.RegionDetection(_data.ActivePalm.transform.position);

        switch (region)
        {
            case MenuRegion.UPPER:
                animator.SetInteger(_hash_toolSelectedInt, 0);
                break;

            case MenuRegion.RIGHT:
                animator.SetInteger(_hash_toolSelectedInt, 1);
                break;

            case MenuRegion.LOWER:
                animator.SetInteger(_hash_toolSelectedInt, 2);
                break;

            case MenuRegion.LEFT:
                break;

            case MenuRegion.MIDDLE:
                break;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data.ManiMenu.SetActive(false);
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
