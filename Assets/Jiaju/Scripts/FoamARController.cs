using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portalble;
using Portalble.Functions.Grab;


public class FoamARController : PortalbleGeneralController
{
	public Transform placePrefab;
	public float offset = 0.01f;
	public GameObject JUIController;
    private Animator _JUIGestureAnimator;
    public GameObject m_leftHand;
    public GameObject m_rightHand;
    public GameObject m_createMenu;
    public GameObject m_maniMenu;

    private GameObject _activeIndex;
    private GameObject _activeThumb;

    private GestureControl m_leftGC;
    private GestureControl m_rightGC;
    private JUIController m_jui;
    private GameObject m_activeHand;
    private GestureControl m_activeGC;


    //scene mgm
    public FoamDataManager m_data; 


    //state machine
    private Animator m_stateMachine;

    private int m_hash_createBool = Animator.StringToHash("CreateStateBool");
    private int m_hash_maniBool = Animator.StringToHash("ManipulationStateBool");
    private int m_hash_idleBool = Animator.StringToHash("IdleStateBool");
    private int m_hash_pinchBool = Animator.StringToHash("PinchBool");
    private int _hash_palmBool = Animator.StringToHash("PalmBool");
    private List<int> m_uiState_hashes = new List<int>();

    private int _hash_gesturePinchBool = Animator.StringToHash("gesturePinchBool");
    private int _hash_gesturePalmBool = Animator.StringToHash("gesturePalmBool");
    

    public override void OnARPlaneHit(PortalbleHitResult hit)
	{
		base.OnARPlaneHit(hit);

		//if (placePrefab != null)
		//{
		//	Instantiate(m_createMenu.transform, hit.Pose.position + hit.Pose.rotation * Vector3.up * offset, hit.Pose.rotation);
  //          Debug.Log("FOAMFILTER hit pos" + hit.Pose.position + hit.Pose.rotation * Vector3.up * offset);
  //          Debug.Log("FOAMFILTER hit rotation" + hit.Pose.rotation);
		//}
	}

    protected override void Start()
    {
        base.Start();

        // move things below to m_data
        m_leftGC = m_leftHand.GetComponent<GestureControl>();
        m_rightGC = m_rightHand.GetComponent<GestureControl>();
        m_jui = JUIController.GetComponent<JUIController>();

        m_activeGC = m_rightGC;
        m_activeHand = m_rightHand;

        _activeIndex = m_activeHand.transform.GetChild(1).GetChild(2).gameObject;
        _activeThumb = m_activeHand.transform.GetChild(0).GetChild(2).gameObject;

        //state machine
        m_stateMachine = this.GetComponent<Animator>();
        m_uiState_hashes.Add(m_hash_idleBool);
        m_uiState_hashes.Add(m_hash_createBool);
        m_uiState_hashes.Add(m_hash_maniBool);

        m_data.StateMachine = m_stateMachine;

        FoamUtils.IsExcludingSelectedObj = false;
        FoamUtils.IsGlobalGrabbing = false;

        _JUIGestureAnimator = JUIController.GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        m_maniMenu.transform.LookAt(2 * m_maniMenu.transform.position - Camera.main.transform.position);
        m_createMenu.transform.LookAt(2 * m_createMenu.transform.position - Camera.main.transform.position);

        switch (m_jui.FoamState)
        {
            case FoamState.STATE_CREATE:
                this.handleCreate();
                break;

            case FoamState.STATE_MANIPULATE:
                this.handleManipulate();
                break;

            case FoamState.STATE_IDLE:
                this.handleIdle();
                break;

            default:
                break;
        }

        ToggleGestureBools();

        //if (Input.GetKey(KeyCode.DownArrow))
        //{
        //    m_stateMachine.SetBool(m_hash_pinchBool, true);
        //}

        //if (Input.GetKey(KeyCode.UpArrow))
        //{
        //    m_stateMachine.SetBool(m_hash_pinchBool, false);
        //}

        //ManageSelectedObj();

    }


    private void handleIdle()
    {
        switchStateBool(m_hash_idleBool);

    }


    private void handleCreate()
    {
        switchStateBool(m_hash_createBool);
    }


    private void handleManipulate()
    {
        switchStateBool(m_hash_maniBool);
    }


    private void switchStateBool(int targetState)
    {
        for (int i = 0; i < m_uiState_hashes.Count; i++)
        {
            if (m_uiState_hashes[i] == targetState)
            {
                m_stateMachine.SetBool(m_uiState_hashes[i], true);

            } else
            {
                m_stateMachine.SetBool(m_uiState_hashes[i], false);

            }
        }
    }

    private void CheckIsGlobalFingerInObj()
    {
        int iter = m_data.SceneObjGCs.Count;

        for (int i = 0; i < iter; i++)
        {
            if (m_data.SceneObjGCs[i].IsFingerInObj)
            {
                FoamUtils.IsGlobalFingerInObject = true;
                return;
            }
        }

        FoamUtils.IsGlobalFingerInObject = false;
    }

    private void ToggleGestureBools()
    {


        float dis = Vector3.Distance(_activeIndex.transform.position, _activeThumb.transform.position);

        if (!Grab.Instance.IsGrabbing)
        {
            CheckIsGlobalFingerInObj();
            //Debug.Log("FoamUtils.IsGlobalFingerInObject: " + FoamUtils.IsGlobalFingerInObject);
            if (m_activeGC.bufferedGesture() == "pinch" && !FoamUtils.IsGlobalFingerInObject && dis < 0.035f)
            {
                m_stateMachine.SetBool(m_hash_pinchBool, true);
                _JUIGestureAnimator.SetBool(_hash_gesturePinchBool, true);
            }
            else
            {
                m_stateMachine.SetBool(m_hash_pinchBool, false);
                _JUIGestureAnimator.SetBool(_hash_gesturePinchBool, false);
            }


            if (m_activeGC.bufferedGesture() == "palm") // or include more gestures
            {
                m_stateMachine.SetBool(_hash_palmBool, true);
                _JUIGestureAnimator.SetBool(_hash_gesturePalmBool, true);

            }
            else
            {
                m_stateMachine.SetBool(_hash_palmBool, false);
                _JUIGestureAnimator.SetBool(_hash_gesturePalmBool, false);
            }
        }
    }

    //private void ManageSelectedObj()
    //{
    //    if (m_stateMachine.GetCurrentAnimatorStateInfo(0).IsName("ManipulationState"))
    //    {
            
    //    }
    //}
}
