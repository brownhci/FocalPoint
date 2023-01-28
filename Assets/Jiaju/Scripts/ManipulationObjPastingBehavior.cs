using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManipulationObjPastingBehavior : StateMachineBehaviour
{

    private Transform _copiedObj;
    private FoamDataManager _data;

    private bool _isReleased = false;

    private Renderer _copiedRenderer;
    private Color _copiedOGColor;

    private int _animCount = 0;
    private int _transStep = 0;
    private Vector3 _initialScale;
    private Quaternion _initialRot;

    private int _hash_objMenuClosedBool = Animator.StringToHash("ObjMenuClosedBool");


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _data = GameObject.FindGameObjectWithTag("foamDM").GetComponent<FoamDataManager>();
        animator.SetBool(_hash_objMenuClosedBool, false);

        _data.StateIndicator.GetComponent<Text>().text = "Pinch to Place";

        _copiedObj = null;
        _animCount = 0;
        _transStep = 0;

        string currName = _data.CurrentSelectionObj.name.Replace("(Clone)", "").Trim();

        if (currName == "FoamCone")
        {
            _copiedObj = Instantiate(_data.ConeDummyPrefab.transform, _data.ObjCreatedPos, Quaternion.identity);

        }
        else
        {
            _copiedObj = Instantiate(_data.CurrentSelectionObj.transform, _data.ObjCreatedPos, Quaternion.identity);

        }

        _copiedObj.gameObject.name = _copiedObj.gameObject.name.Replace("(Clone)", "").Trim();

        _copiedRenderer = _copiedObj.gameObject.GetComponent<Renderer>();
        _copiedRenderer.material.color = _data.ObjManiOGColor;
        _copiedOGColor = _copiedRenderer.material.color;

        _initialScale = _data.CurrentSelectionObj.transform.localScale;
        _initialRot = _data.CurrentSelectionObj.transform.rotation;

        _copiedObj.transform.localScale = Vector3.zero;
        _copiedObj.transform.rotation = _initialRot;

        _isReleased = false;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (_copiedObj && !_isReleased)
        {
            _transStep = FoamUtils.AnimateWaveTransparency(_copiedRenderer, _transStep);
        }

        // play scale animation first
        if (_copiedObj && _animCount < FoamUtils.ObjCreatedAnimTime)
        {
            _animCount = FoamUtils.AnimateGrowSize(_animCount, _initialScale, _copiedObj, _data.ObjCreatedPos);
            return;
        }

        if (_copiedObj)
        {
            if (!_isReleased)
            {
                _copiedObj.position = _data.ObjCreatedPos;
            }

            if (_data.ActiveGC.bufferedGesture() == "pinch")
            {
                _isReleased = true;

                _copiedObj.gameObject.name = _copiedObj.gameObject.name.Replace("(Clone)", "").Trim();
                if (_copiedObj.gameObject.tag == "DummyCone")
                {
                    GameObject.Destroy(_copiedObj.gameObject);
                    _copiedObj = Instantiate(_data.ConePrefab, _data.ObjCreatedPos, Quaternion.identity);
                    _copiedObj.transform.localScale = _initialScale;
                    _copiedObj.transform.rotation = _initialRot;
                }

                _copiedObj.gameObject.name = _copiedObj.gameObject.name.Replace("(Clone)", "").Trim();
                _copiedRenderer.material.color = _copiedOGColor;

                //_data.SceneObjs.Add(_copiedObj.gameObject);
                FoamUtils.CreateObjData(_data, _copiedObj.gameObject);

                animator.SetBool(_hash_objMenuClosedBool, true);
            }
        }
        else
        {
            animator.SetBool(_hash_objMenuClosedBool, true);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Modelable model = _copiedObj.GetComponent<Modelable>();
        if (model)
        {
            model.SetAsSelected();
        }

        // undo redo
        ICommand copyAction = new CommandCreateCopy(_copiedObj.gameObject, _data);
        UndoRedoManager.AddNewAction(copyAction);
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
