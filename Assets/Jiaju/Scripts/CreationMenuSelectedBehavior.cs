using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Portalble.Functions.Grab;

public class CreationMenuSelectedBehavior : StateMachineBehaviour
{

    private FoamDataManager m_data;
    private int m_hash_actionBool = Animator.StringToHash("ActionPerformedBool");
    private int m_hash_itemSelectedBool = Animator.StringToHash("ItemSelectedBool");
    private Transform m_prim = null;
    private Transform m_prim_child = null;
    private bool m_isReleased =  false;

    private Renderer _primRenderer;
    private Color _primOGColor;
    private int _animCount = 0;
    private int _transStep = 0;
    private Vector3 _initalScale;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        m_data = GameObject.FindGameObjectWithTag("foamDM").GetComponent<FoamDataManager>();
        m_data.StateIndicator.GetComponent<Text>().text = "Pinch to Place";

        //Debug.Log("FOAMFILTER MENU ITEM SELECTED IS: " + m_data.Selected_createItem);

        m_prim = null;
        _animCount = 0;
        _transStep = 0;

        switch (m_data.Selected_createItem)
        {
            case CreateMenuItem.CUBE:
                //Debug.Log("FOAMFILTER CUBE ITEM CREATED");
                m_prim = Instantiate(m_data.CubePrefab, m_data.ObjCreatedPos, Quaternion.identity);
                break;

            case CreateMenuItem.SPHERE:
                //Debug.Log("FOAMFILTER SPHERE ITEM CREATED");
                m_prim = Instantiate(m_data.SpherePrefab, m_data.ObjCreatedPos, Quaternion.identity);
                break;

            case CreateMenuItem.CYLINDER:
                //Debug.Log("FOAMFILTER CYLINDER ITEM CREATED");
                m_prim = Instantiate(m_data.CylinderPrefab, m_data.ObjCreatedPos, Quaternion.identity);
                break;

            case CreateMenuItem.CONE:
                //Debug.Log("FOAMFILTER CONE ITEM CREATED");
                m_prim = Instantiate(m_data.ConeDummyPrefab, m_data.ObjCreatedPos, Quaternion.identity);
                break;

            default:
                break;
        }

        if (m_prim)
        {
            _primRenderer = m_prim.gameObject.GetComponent<Renderer>();
            _primOGColor = _primRenderer.material.color;

            _initalScale = m_prim.transform.localScale;
            m_prim.transform.localScale = Vector3.zero;
            //Debug.Log("MODELABLE obj count: " + m_data.SceneObjs.Count);
        }

        m_isReleased = false;
        m_data.CreateMenu.SetActive(false);

    }

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{

        // change transparency
        if (m_prim && !m_isReleased)
        {
            _transStep = FoamUtils.AnimateWaveTransparency(_primRenderer, _transStep);
        }

        // play scale animation first
        if (m_prim && _animCount < FoamUtils.ObjCreatedAnimTime)
        {
            _animCount = FoamUtils.AnimateGrowSize(_animCount, _initalScale, m_prim, m_data.ObjCreatedPos);
            return;
        }

        if (m_prim)
        {
            if (!m_isReleased)
            {
                m_prim.position = m_data.ObjCreatedPos;
            }

            //if (m_data.ActiveGC.bufferedGesture() == "pinch" || Input.GetKey(KeyCode.DownArrow))
            if (m_data.ActiveGC.bufferedGesture() == "pinch")
            {
                m_isReleased = true;

                m_prim.gameObject.name = m_prim.gameObject.name.Replace("(Clone)", "").Trim();
                if (m_prim.gameObject.tag == "DummyCone")
                {
                    GameObject.Destroy(m_prim.gameObject);
                    m_prim = Instantiate(m_data.ConePrefab, m_data.ObjCreatedPos, Quaternion.identity);
                }

                m_prim.gameObject.name = m_prim.gameObject.name.Replace("(Clone)", "").Trim();
                _primRenderer.material.color = _primOGColor;

                FoamUtils.CreateObjData(m_data, m_prim.gameObject);

                animator.SetBool(m_hash_actionBool, true);
            }   
        }
        else
        {
            animator.SetBool(m_hash_actionBool, true);
        }
    }

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
        //Debug.Log("EXITING CREATION MENU SELECTED STATE");

        Modelable model = m_prim.gameObject.GetComponent<Modelable>();
        if (model)
        {
            model.SetAsSelected();
        }

        // undo redo
        ICommand createAction = new CommandCreateCopy(m_prim.gameObject, m_data);
        UndoRedoManager.AddNewAction(createAction);

        animator.SetBool(m_hash_itemSelectedBool, false);
        m_prim = null;
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
