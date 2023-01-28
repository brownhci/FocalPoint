using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolScaleSelectedBehavior : StateMachineBehaviour
{
    private FoamDataManager _data;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data = GameObject.FindGameObjectWithTag("foamDM").GetComponent<FoamDataManager>();

        _data.ManiMenuParent.SetToolOptionInUse(2);
        _data.ManiMenu.SetActive(false);
        _data.StateIndicator.GetComponent<Text>().text = "Scale";

        FoamUtils.IsGlobalGrabbing = true;
        FoamUtils.IsExcludingSelectedObj = true;

        if (!_data.CurrentSelectionObj) return;
        _data.FoamScaleTool.SetTarget(_data.CurrentSelectionObj.transform);
        _data.FoamScaleTool.SetUpTabs();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data.FoamScaleTool.DestroyTabs();
        FoamUtils.IsGlobalGrabbing = false;
        FoamUtils.IsExcludingSelectedObj = false;
        if (_data.CurrentSelectionObj)
        {
            Portalble.Functions.Grab.GrabCollider curGC = _data.CurrentSelectionObj.transform.GetChild(0).GetComponent<Portalble.Functions.Grab.GrabCollider>();
            if (curGC)
            {
                Debug.Log("____exiting selection tool degrabbed");
                curGC.DeGrab();
            }
        }
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
