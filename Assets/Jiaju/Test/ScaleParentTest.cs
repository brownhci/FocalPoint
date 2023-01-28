using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleParentTest : MonoBehaviour
{
    //public FoamDataManager m_data;
    //public Transform m_scaleTabPrefab;
    public Transform m_linePrefab;
    public Transform m_targetTrans;

    public ScaleTabTest m_up;
    public ScaleTabTest m_down;
    public ScaleTabTest m_right;
    public ScaleTabTest m_left;
    public ScaleTabTest m_forward;
    public ScaleTabTest m_back;


    private List<FoamScaleTab> _tabs = new List<FoamScaleTab>();

    // Start is called before the first frame update
    void Start()
    {
        SetUpTabs();
        Debug.Log("called in start");
    }

    // Update is called once per frame
    void Update()
    {
        SetUpTabs();
        Debug.Log("called in update");

    }

    public void SetUpTabs()
    {
        _tabs = new List<FoamScaleTab>();
        if (!m_targetTrans) { return; }
        Bounds curBound = m_targetTrans.GetComponent<MeshFilter>().sharedMesh.bounds; // or just mesh?

        // up
        m_up.transform.position = m_targetTrans.position + m_targetTrans.up * (m_targetTrans.localScale[1] * curBound.size[1] / 2 + FoamUtils.ScaleTabOffset);
        m_up.SetTarget(this, 0, m_targetTrans, m_targetTrans.up, m_targetTrans.right, 1, 1);
        //FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        // down
        m_down.transform.position = m_targetTrans.position - m_targetTrans.up * (m_targetTrans.localScale[1] * curBound.size[1] / 2 + FoamUtils.ScaleTabOffset);
        m_down.SetTarget(this, 1, m_targetTrans, m_targetTrans.up, m_targetTrans.right, 1, -1);
        //FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        //// right
        m_right.transform.position = m_targetTrans.position + m_targetTrans.right * (m_targetTrans.localScale[0] * curBound.size[0] / 2 + FoamUtils.ScaleTabOffset);
        m_right.SetTarget(this, 2, m_targetTrans, m_targetTrans.right, m_targetTrans.forward, 0, 1);
        //FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        //// left
        m_left.transform.position = m_targetTrans.position - m_targetTrans.right * (m_targetTrans.localScale[0] * curBound.size[0] / 2 + FoamUtils.ScaleTabOffset);
        m_left.SetTarget(this, 3, m_targetTrans, m_targetTrans.right, m_targetTrans.forward, 0, -1);
        //FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        //// forward
        m_forward.transform.position = m_targetTrans.position + m_targetTrans.forward * (m_targetTrans.localScale[2] * curBound.size[2] / 2 + FoamUtils.ScaleTabOffset);
        m_forward.SetTarget(this, 4, m_targetTrans, m_targetTrans.forward, m_targetTrans.up, 2, 1);
        //FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        //// back
        m_back.transform.position = m_targetTrans.position - m_targetTrans.forward * (m_targetTrans.localScale[2] * curBound.size[2] / 2 + FoamUtils.ScaleTabOffset);
        m_back.SetTarget(this, 5, m_targetTrans, m_targetTrans.forward, m_targetTrans.up, 2, -1);
        //FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);
    }
}
