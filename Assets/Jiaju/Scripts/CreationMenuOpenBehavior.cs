using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationMenuOpenBehavior : StateMachineBehaviour
{
    private FoamDataManager _data;
    private int m_hash_itemSelectedBool = Animator.StringToHash("ItemSelectedBool");
    private CreateMenuItem m_selectedItem = CreateMenuItem.NULL;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data = GameObject.FindGameObjectWithTag("foamDM").GetComponent<FoamDataManager>();

        _data.CreateMenu.transform.position = _data.ActiveIndex.transform.position;
        _data.CreateMenu.SetActive(true);
        _data.ManiMenu.SetActive(false);

        _data.CreateMenuParent.RecordPalmPosInit(_data.ActivePalm.transform.position);

        animator.SetBool(m_hash_itemSelectedBool, false);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        MenuRegion region = _data.CreateMenuParent.RegionDetection(_data.ActivePalm.transform.position);

        switch (region)
        {
            case MenuRegion.UPPER:
                m_selectedItem = CreateMenuItem.CUBE;
                animator.SetBool(m_hash_itemSelectedBool, true);
                break;

            case MenuRegion.RIGHT:
                m_selectedItem = CreateMenuItem.CYLINDER;
                animator.SetBool(m_hash_itemSelectedBool, true);
                break;

            case MenuRegion.LOWER:
                m_selectedItem = CreateMenuItem.CONE;
                animator.SetBool(m_hash_itemSelectedBool, true);
                break;


            case MenuRegion.LEFT:
                m_selectedItem = CreateMenuItem.SPHERE;
                animator.SetBool(m_hash_itemSelectedBool, true);
                break;

            case MenuRegion.MIDDLE:
                m_selectedItem = CreateMenuItem.NULL;          
                animator.SetBool(m_hash_itemSelectedBool, false);
                break;
        }

        _data.Selected_createItem = m_selectedItem;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
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
