using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Portalble;
using Portalble.Functions.Grab;

public class BaseController : PortalbleGeneralController
{

    public Transform m_bigFocusObj;
    public Transform m_smallFocusObj;
    public Transform m_editorHolder;
    public FocusTaskController m_fTC;

    public SelectionDataManager m_sDM;

    private GameObject m_objToSelect;

    public float offset = 0.08f;

    public Canvas m_canvas;

    private bool m_isSnapped = false;

    public GameObject WSManager;
    public string websocketPort = "8765";

    public Transform m_preHighlightPrefab;
    private Transform m_preHighlight;


    public bool IsSnapped
    {
        set { m_isSnapped = value; }
        get { return m_isSnapped; }
    }



    protected override void Start()
    {
        base.Start();
#if !UNITY_EDITOR
        SetupServer();
#endif
        FoamUtils.IsExcludingSelectedObj = false;
        FoamUtils.IsGlobalGrabbing = true;

        m_preHighlight = Instantiate(m_preHighlightPrefab);
        m_preHighlight.gameObject.SetActive(false);
    }

    private void SetupServer()
    {
        // Create web socket
        Debug.Log("Connecting" + WSManager.GetComponent<WSManager>().websocketServer);
        string url = "ws://" + WSManager.GetComponent<WSManager>().websocketServer + ":" + websocketPort;

        Jetfire.Open2(url);
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        UpdateActiveHandData();

        AidSelection();
        ResetSelectionAid();
    }

    private void LateUpdate()
    {
        if (Grab.Instance.IsGrabbing)
        {
            m_preHighlight.gameObject.SetActive(false);
        }
        else
        {
            m_preHighlight.gameObject.SetActive(true);
        }
    }

    private void AidSelection()
    {

        if (Grab.Instance.IsGrabbing) return;

        m_objToSelect = FocusUtils.BaseObjectToSelect(m_sDM, m_sDM.MaxSnapDis);

        if (!m_objToSelect)
        {
            m_preHighlight.gameObject.SetActive(false);
            return;
        }

        // pre-selection highlight
        m_preHighlight.position = m_objToSelect.transform.position;
        m_preHighlight.rotation = m_objToSelect.transform.rotation;
        m_preHighlight.gameObject.SetActive(true);

        // snap target object to hand if close enough
        Vector3 indexThumbPos = FocusUtils.GetIndexThumbPos(m_sDM);

        // snapping object to hand
        if (Vector3.Distance(m_objToSelect.transform.position, indexThumbPos) < m_sDM.MaxSnapDis)
        {
            if (ActiveHandGesture == "pinch")
            {
                if (!Grab.Instance.IsGrabbing && !m_isSnapped && FocusUtils.IsFingersCloseEnough(m_sDM))
                {
                    m_objToSelect.GetComponent<Selectable>().SetSnapped(indexThumbPos);
                }
            }
        }
    }



    private void ResetSelectionAid()
    {
        if (!Grab.Instance.IsGrabbing && m_sDM.IsSnappedObejctReleased)
        {
            m_isSnapped = false;
        }
    }



    public void RecordGrabLoc(GameObject grabbedObj)
    {
        Vector3 grabPos = grabbedObj.GetComponent<Selectable>().GetSnappedPosition();
        Vector2 newPos = FocusUtils.WorldToUISpace(m_canvas, grabPos);

        Debug.Log("bc RecordGrabLoc running");
        FocusUtils.SendSelectionData(grabPos, grabbedObj, m_sDM, SceneManager.GetActiveScene().name);
    }


    private void UpdateActiveHandData()
    {
        HandManager activeHM = this.ActiveHandManager;
        if (activeHM)
        {
            m_sDM.ActiveHand = activeHM.gameObject;
        }
        else
        {
            m_sDM.ActiveHand = null;
        }

        m_sDM.updateActiveObjects();
    }


    public override void OnARPlaneHit(PortalbleHitResult hit)
    {
        base.OnARPlaneHit(hit);

        if (m_sDM.TaskOnePlacePrefab != null)
        {
            m_fTC.GenerateObjsFromHit(m_sDM.TaskOnePlacePrefab, hit.Pose.position, hit.Pose.rotation);
        }
    }

}
