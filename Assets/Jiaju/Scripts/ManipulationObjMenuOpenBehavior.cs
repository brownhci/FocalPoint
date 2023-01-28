using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManipulationObjMenuOpenBehavior : StateMachineBehaviour
{
    private FoamDataManager _data;
    private int _hash_dwellBool = Animator.StringToHash("DwellBool");
    //private int _hash_objMenuClosedBool = Animator.StringToHash("ObjMenuClosedBool");


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data = GameObject.FindGameObjectWithTag("foamDM").GetComponent<FoamDataManager>();

        animator.SetBool(_hash_dwellBool, false);

        if (_data.CurrentSelectionObj)
        {
            _data.CurrentSelectionObj.GetComponent<Renderer>().material.color = FoamUtils.ObjManiSelectedColor;
        }

        Debug.Log("ICONN Object menu state");

    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_data.CurrentSelectionObj)
        {
            _data.ObjMenu.transform.position = _data.CurrentSelectionObj.transform.position - _data.FirstPersonCamera.transform.forward * 0.11f; // need to make this offset variable
            _data.ObjMenu.transform.LookAt(Camera.main.transform);
            _data.ObjMenu.SetActive(true);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data.ObjMenu.SetActive(false);
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
