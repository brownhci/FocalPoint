using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kalman;
using Portalble;
using Portalble.Functions.Grab;

public class SelectionController : PortalbleGeneralController
{

    public Transform m_bigFocusObj;
    public Transform m_smallFocusObj;
    public Transform m_editorHolder;
    public FocusTaskController m_fTC;

    public float offset = 0.08f;

    public Canvas m_canvas;
    public GameObject prefab_marker;
    public GameObject prefab_centerMarker;
    private GameObject m_centerMarker;

    // selection specific
    //private bool m_isRecorded = false;
    private List<GameObject> m_markers = new List<GameObject>();
    private Queue<KeyValuePair<Vector2, long>> m_markers_screenPos = new Queue<KeyValuePair<Vector2, long>>();
    private int m_markerQueueLimit = 10;
    private bool m_isMarkerDisplayed = false;
    public SelectionDataManager m_sDM;
    private LineRenderer m_guideLine;    // disabled
    private GameObject m_highestRankedObjMarker;
    private GameObject m_highestRankedObj;
    private bool m_isSnapped = false;
    //private float m_maxSnapDis = 0.025f;


    // websocket
    public GameObject WSManager;
    public string websocketPort = "8765";

    // selection cylinder
    public Transform m_focusCylinderPrefab;
    public Transform m_occludingGeoPrefab;
    private Transform m_focusCylinder;
    private Transform m_occludingGeo;
    private Renderer m_focusCylinderRenderer;
    private Vector3 m_focusCylinderCenterPos = Vector3.zero;
    private Vector3 m_focusCylinderCenterPos_noOffset = Vector3.zero;
    private Vector2 m_focusCylinderCenterPos_screenPos;
    private float m_v = 0f;
    private float m_h = 0f;

    // Kalman
    private SimpleKalmanWrapper m_kalmanSelector;

    // test distance calculation
    //private GameObject m_marker2;
    private Portalble.Functions.Grab.Grabable _prevLastSelectedObj = null;

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

        //placePrefab = m_smallFocusObj;

        // set up focus cylinder
        //m_focusCylinder = Instantiate(m_focusCylinderPrefab, m_FirstPersonCamera.transform.position + 0.2f * m_FirstPersonCamera.transform.forward, Quaternion.identity);
        m_focusCylinderCenterPos_screenPos = new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);

        m_focusCylinder = Instantiate(m_focusCylinderPrefab);
        m_focusCylinderRenderer = m_focusCylinder.GetComponent<Renderer>();
        m_focusCylinder.gameObject.GetComponent<Collider>().attachedRigidbody.useGravity = false;
        float initial_width = Mathf.Sqrt(Mathf.Pow(Camera.main.pixelWidth, 2) + Mathf.Pow(Camera.main.pixelHeight, 2)) / (2 * 10000f);
        // float initial_width = Camera.main.pixelWidth / (3.2f * 10000f); // small width for testing

        m_focusCylinder.localScale = new Vector3(initial_width, m_focusCylinder.localScale[1], initial_width);

        m_centerMarker = Instantiate(prefab_centerMarker, Vector3.zero, Quaternion.identity, m_canvas.transform);
        m_centerMarker.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        m_occludingGeo = Instantiate(m_occludingGeoPrefab);

        m_guideLine = Instantiate(m_sDM.FocusInkPrefab).GetComponent<LineRenderer>();
        m_guideLine.positionCount = 2;
        m_guideLine.gameObject.SetActive(false);

        // marks where the highestRankedObj is
        m_highestRankedObjMarker = Instantiate(prefab_marker, Vector3.zero, Quaternion.identity, m_canvas.transform);
        m_highestRankedObjMarker.SetActive(false);

        FoamUtils.IsExcludingSelectedObj = false;
        FoamUtils.IsGlobalGrabbing = true;

        // kalman selector
        m_kalmanSelector = new SimpleKalmanWrapper();
    }

    private void SetupServer()
    {
        // Create web socket
        Debug.Log("Connecting" + WSManager.GetComponent<WSManager>().websocketServer);
        string url = "ws://" + WSManager.GetComponent<WSManager>().websocketServer + ":" + websocketPort;

        Jetfire.Open2(url);
    }

    protected override void Update()
    {
        base.Update();

   
        UpdateActiveHandData();

        if (!m_sDM.UseSelectionAid) return;

        UpdateFocusCylinder();
        UpdateDepthCues();

        AdjustSelectionAid();

        AidSelection();
        //RecordGrabLoc();

        ResetSelectionAid();

    }

    /// <summary>
    /// Turn on the visuals of selection aid
    /// </summary>
    private void TurnOnSelectionAid()
    {
        if (!m_focusCylinderRenderer.enabled)
        {
            m_focusCylinderRenderer.enabled = true;
            m_centerMarker.SetActive(true);
        }
        //FocusUtils.SetObjsToAlpha(m_sDM.SceneObjects, FocusUtils.FarAlpha);
    }

    /// <summary>
    /// Turn off the visuals of selection aid
    /// </summary>
    private void TurnOffSelectionAid()
    {
        if (m_focusCylinderRenderer.enabled)
        {
            m_focusCylinderRenderer.enabled = false;
            m_centerMarker.SetActive(false);
        }

        FocusUtils.SetObjsToAlpha(m_sDM.SceneObjects, FocusUtils.NearAlpha);
        DecontourAllObjects();
    }

    
    private void AdjustSelectionAid()
    {
        if (!m_sDM.UseSelectionAid)
        {
            FocusUtils.SetObjsToAlpha(m_sDM.SceneObjects, FocusUtils.NearAlpha);
        }
    }

    /// <summary>
    /// updates the location, scale, and rotation of the focus cylinder
    /// </summary>
    private void UpdateFocusCylinder()
    {
        if (m_focusCylinder == null)
            return;

        float fac = m_focusCylinder.transform.localScale[1] * 2f; //1.2f if y is 0.5. 1.31f is y is 0.3. 2.f is y is 0.1

        m_focusCylinderCenterPos = m_FirstPersonCamera.transform.position + fac * m_FirstPersonCamera.transform.forward + m_h * m_FirstPersonCamera.transform.right + m_v * m_FirstPersonCamera.transform.up;
        m_focusCylinderCenterPos_noOffset = m_FirstPersonCamera.transform.position + m_h * m_FirstPersonCamera.transform.right + m_v * m_FirstPersonCamera.transform.up;

        m_focusCylinder.position = m_focusCylinderCenterPos;
        m_focusCylinder.LookAt(m_focusCylinder.position - (m_FirstPersonCamera.transform.position - m_focusCylinder.position));
        m_focusCylinder.Rotate(90.0f, 0.0f, 0.0f, Space.Self);

        // update center marker
        m_centerMarker.GetComponent<RectTransform>().anchoredPosition = FocusUtils.WorldToUISpace(m_canvas, m_focusCylinderCenterPos);

        // occluding object update
        Vector3 handCamVec = FocusUtils.GetPosForOccludingGeo(m_sDM, m_FirstPersonCamera.transform.forward) - m_FirstPersonCamera.transform.position;
        Vector3 projVec = Vector3.Project(handCamVec, m_FirstPersonCamera.transform.forward);
        m_occludingGeo.rotation = m_focusCylinder.rotation;
        m_occludingGeo.position = m_FirstPersonCamera.transform.position + projVec;

        // disabled cylinder radius adjustment
        UpdateCylinderRadius(0f);
    }




    private void UpdateCylinderCenter()
    {
        if (m_markers_screenPos.Count == m_markerQueueLimit)
        {
          //  Debug.Log("CYLINDERR q count: " + m_markers_screenPos.Count);
            Vector2 center = new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
            Vector2 focusPos = FocusUtils.CalcFocusCenter(m_markers_screenPos, m_markerQueueLimit);
            m_focusCylinderCenterPos_screenPos = focusPos;

            Vector2 distance_vec = focusPos - center;

            m_v = distance_vec.y / 10000f;
            m_h = distance_vec.x / 10000f;
        }
    }




    private void UpdateCylinderRadius(float maxWidth)
    {
        if (m_markers_screenPos.Count == m_markerQueueLimit)
        {
            float maxDis = -9999999f;

            IEnumerator<KeyValuePair<Vector2, long>> marker_enum = m_markers_screenPos.GetEnumerator();

            while (marker_enum.MoveNext())
            {
                float dis = Vector2.Distance(m_focusCylinderCenterPos_screenPos, marker_enum.Current.Key);

                if (dis > maxDis)
                {
                    maxDis = dis;
                }
            }

            // an arbitrary minimum for now
            //if (maxDis < Camera.main.pixelWidth / 4) maxDis = Camera.main.pixelWidth / 4;

            float width = maxDis / 10000f;

            m_focusCylinder.localScale = new Vector3(width, m_focusCylinder.localScale[1], width);
            m_centerMarker.SetActive(true);
        }
        else
        {
            m_centerMarker.SetActive(false);

        }
        //if (!ActiveHandManager) return;

        //float dis = Vector3.Distance(m_sDM.ActiveIndex.transform.position, m_sDM.ActiveThumb.transform.position);
        //if (dis > maxWidth) dis = maxWidth;
        //m_focusCylinder.localScale = new Vector3(dis, m_focusCylinder.localScale[1], dis);

    }

    //private void CheckGrabbingAndSnappingStatus()
    //{
    //    if (Grab.Instance.IsGrabbing && m_isSnapped)
    //    {
    //        Grab.Instance.SelectObject.GetComponent<Selectable>().ConfirmSnapped();
    //    }
    //}


    private void AidSelection()
    {

        if (Grab.Instance.IsGrabbing) return;

        UpdatePeripheralObjects();

        m_highestRankedObj = FocusUtils.RankFocusedObjects(m_focusCylinderCenterPos, m_sDM, m_canvas, m_kalmanSelector);
        
        if (!m_highestRankedObj || !ActiveHandManager)
        {
            m_guideLine.gameObject.SetActive(false);
            m_highestRankedObjMarker.SetActive(false);
            return;
        }

        m_highestRankedObj.GetComponent<Selectable>().SetHighestRankContour();
        DecontourOtherFocusedObjects(m_highestRankedObj.GetInstanceID());

        // snap target object to hand if close enough
        Vector3 indexThumbPos = FocusUtils.GetIndexThumbPos(m_sDM);

        // snapping object to hand
        if (Vector3.Distance(m_highestRankedObj.transform.position, indexThumbPos) < m_sDM.MaxSnapDis)
        {
            if (ActiveHandGesture == "pinch")
            {
                if(!Grab.Instance.IsGrabbing && !m_isSnapped && FocusUtils.IsFingersCloseEnough(m_sDM))
                {
                    m_highestRankedObj.GetComponent<Selectable>().SetSnapped(indexThumbPos);
                    // move these to after confirmed
                    //m_isSnapped = true;
                    //RecordGrabLoc(m_highestRankedObj);
                }
            }
        }
    }


    private void DecontourOtherFocusedObjects(int RankedObjID)
    {
        for (int i = 0; i < m_sDM.SceneObjects.Count; i++)
        {
            GameObject curr = m_sDM.SceneObjects[i];
            if (curr.GetInstanceID() != RankedObjID)
            {
                curr.GetComponent<Selectable>().RemoveHighestRankContour();
            }
        }
    }

    private void DecontourAllObjects()
    {
        for (int i = 0; i < m_sDM.SceneObjects.Count; i++)
        {
            GameObject curr = m_sDM.SceneObjects[i];
            curr.GetComponent<Selectable>().RemoveHighestRankContour();
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
        Debug.Log("snap loc recorded");
        Vector3 grabPos = grabbedObj.GetComponent<Selectable>().GetSnappedPosition();
        Vector3 grabPos_screenSpace = FocusUtils.WorldToScreenSpace(grabPos); // (bottom left is 0, 0)

        // if outside of the screen, then don't record
        if (grabPos_screenSpace[0] < 0 || grabPos_screenSpace[0] > Camera.main.pixelWidth || grabPos_screenSpace[1] < 0 || grabPos_screenSpace[1] > Camera.main.pixelHeight)
        {
            return;
        }

        // generate new marker
        Vector2 newPos = FocusUtils.WorldToUISpace(m_canvas, grabPos);
        GameObject new_marker = Instantiate(prefab_marker, Vector3.zero, Quaternion.identity, m_canvas.transform);
        new_marker.GetComponent<RectTransform>().anchoredPosition = newPos;
        m_markers.Add(new_marker);
        new_marker.SetActive(false);


        // add grabpos to queue
        if (m_markers_screenPos.Count < m_markerQueueLimit)
        {
            m_markers_screenPos.Enqueue(new KeyValuePair<Vector2, long>(grabPos_screenSpace, System.DateTimeOffset.Now.ToUnixTimeMilliseconds()));
        }
        else
        {
            m_markers_screenPos.Dequeue();
            m_markers_screenPos.Enqueue(new KeyValuePair<Vector2, long>(grabPos_screenSpace, System.DateTimeOffset.Now.ToUnixTimeMilliseconds()));
        }

        Debug.Log("GRABPOS: " + FocusUtils.WorldToScreenSpace(grabPos));

        // only show the most recent markers
        for (int i = 0; i < m_markers.Count; i++)
        {
            if (i >= m_markers.Count - m_markerQueueLimit)
            {
                m_markers[i].SetActive(m_isMarkerDisplayed);
            }
            else
            {
                m_markers[i].SetActive(false);
            }

        }

        UpdateCylinderCenter();
        FocusUtils.SendSelectionData(grabPos, grabbedObj, m_sDM, SceneManager.GetActiveScene().name);
    }


    private void UpdateDepthCues()
    {

        if (!m_sDM.UseSelectionAid) return;

        List<GameObject> cuedObjs = m_sDM.FocusedObjects;

        int numIter = cuedObjs.Count;

        Vector3 visPos = m_focusCylinderCenterPos_noOffset;

        for (int i = 0; i < numIter; i++)
        {
            GameObject curr = cuedObjs[i];

            if (m_sDM.OccludingObjects.Contains(curr)) continue;
            if (!curr) continue;

            Renderer currRenderer = curr.GetComponent<Renderer>();
            float objVisDis = Vector3.Distance(curr.transform.position, visPos);

            if (objVisDis > m_sDM.FarDis) // far from far dis
            {
                FocusUtils.UpdateMaterialAlpha(currRenderer, FocusUtils.FarAlpha);

            }
            else if (objVisDis < m_sDM.FarDis && objVisDis > m_sDM.NearDis) // in between far and near dis
            {
                float alpha = FocusUtils.LinearMapReverse(objVisDis, m_sDM.NearDis, m_sDM.FarDis, FocusUtils.FarAlpha, FocusUtils.NearAlpha); // mapping from m_sDM.NearDis - m_sDM.FarDis to m_sDM.FarAlpha - 1.0f to

                // hand distance from obj is usually from 0.09 to 0.20
                alpha += AddHandDistanceAlpha(curr, alpha);

                FocusUtils.UpdateMaterialAlpha(currRenderer, alpha);

            }
            else
            {
                FocusUtils.UpdateMaterialAlpha(currRenderer, FocusUtils.NearAlpha);

            }
        }
    }



    private float AddHandDistanceAlpha(GameObject curr, float alpha)
    {
        float objHandDis = FocusUtils.FarHandDis;

        if (ActiveHandManager) objHandDis = Vector3.Distance(ActiveHandTransform.position, curr.transform.position);
        if (objHandDis > FocusUtils.FarHandDis) objHandDis = FocusUtils.FarHandDis;

        return FocusUtils.LinearMapReverse(objHandDis, FocusUtils.NearHandDis, FocusUtils.FarHandDis, 0.0f, FocusUtils.NearAlpha - alpha); // add hand distance into consideration
    }


    private void UpdatePeripheralObjects()
    {
        for (int i = 0; i < m_sDM.PeripheralObjects.Count; i++)
        {
            Selectable sel = m_sDM.PeripheralObjects[i].GetComponent<Selectable>();
            sel.DeHighlight();
            sel.RemoveHighestRankContour();
        }

        m_sDM.PeripheralObjects.Clear();


        int num = m_sDM.SceneObjects.Count;

        for (int i = 0; i < num; i++)
        {
            GameObject curr = m_sDM.SceneObjects[i];

            float disToCylinder = FocusUtils.CalcPointToRayDistance(m_focusCylinder.gameObject, curr.transform.position);

            // hard cutoff. need to change to radius of all grab pos
            if (disToCylinder <= m_focusCylinder.localScale[0])
            {
                if (!m_sDM.FocusedObjects.Contains(curr)) //if not in focused objects
                {
                    m_sDM.PeripheralObjects.Add(curr);
                    //FocusUtils.TestColorChange(curr.GetComponent<Renderer>());
                }
                
            }
        }
    }




    // functional functions
    public void ToggleMarkerVisibility()
    {
        m_isMarkerDisplayed = !m_isMarkerDisplayed;
        for (int i = 0; i < m_markers.Count; i++)
        {
            if (i >= m_markers.Count - m_markerQueueLimit)
            {
                m_markers[i].SetActive(m_isMarkerDisplayed);
            } else
            {
                m_markers[i].SetActive(false);
            }
           
        }
    }




    public void ToggleUseSelectionAid()
    {
        m_sDM.UseSelectionAid = !m_sDM.UseSelectionAid;

        if (!Grab.Instance.IsGrabbing && m_sDM.UseSelectionAid) // if not grabbing, enable focus cylinder
        {
            TurnOnSelectionAid();
        }
        else // if grabbing. do not detect candidate objects
        {
            TurnOffSelectionAid();
        }
    }



    public override void OnARPlaneHit(PortalbleHitResult hit)
    {
        base.OnARPlaneHit(hit);

        if (m_sDM.TaskOnePlacePrefab != null)
        {
            m_fTC.GenerateObjsFromHit(m_sDM.TaskOnePlacePrefab, hit.Pose.position, hit.Pose.rotation);
        }
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
}