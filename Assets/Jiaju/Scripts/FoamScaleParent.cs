using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamScaleParent : MonoBehaviour
{
    public FoamDataManager m_data;
    public Transform m_scaleTabPrefab;
    public Transform m_linePrefab;
    private Transform m_targetTrans = null;
    [HideInInspector]
    public Vector3 m_targetTransPosDueToScaling;

    private List<FoamScaleTab> _tabs = new List<FoamScaleTab>();

    // Start is called before the first frame update
    void Start()
    {
   
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_data.StateMachine.GetCurrentAnimatorStateInfo(0).IsName("ToolScaleSelected")) return;
        if (!m_targetTrans) return;

        // if target obj is moved externally, reset tabs
        if (m_targetTransPosDueToScaling != m_targetTrans.position)
        {
            DestroyTabs();
            SetUpTabs();
        }

        // if target obj is no longer active, destroy tabs
        if (m_targetTrans.gameObject.activeInHierarchy == false || m_data.CurrentSelectionObj == null)
        {
            m_targetTrans = null;
            DestroyTabs();
        }
    }

    public void SetTarget(Transform tar)
    {
        m_targetTrans = tar;
        m_targetTransPosDueToScaling = tar.position;
    }

    public void SetUpTabs()
    {
        _tabs = new List<FoamScaleTab>();
        if (!m_targetTrans) { return; }
        Bounds curBound = m_targetTrans.GetComponent<MeshFilter>().sharedMesh.bounds; // or just mesh?

        // up
        _tabs.Add(Instantiate(m_scaleTabPrefab, m_targetTrans.position + m_targetTrans.up * (m_targetTrans.localScale[1] * curBound.size[1] / 2 + FoamUtils.ScaleTabOffset), Quaternion.identity).GetComponent<FoamScaleTab>());
        _tabs[_tabs.Count-1].SetTarget(this, 0, m_targetTrans, m_targetTrans.up, 1, 1);
        FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        // down
        _tabs.Add(Instantiate(m_scaleTabPrefab, m_targetTrans.position - m_targetTrans.up * (m_targetTrans.localScale[1] * curBound.size[1] / 2 + FoamUtils.ScaleTabOffset), Quaternion.identity).GetComponent<FoamScaleTab>());
        _tabs[_tabs.Count - 1].SetTarget(this, 1, m_targetTrans, m_targetTrans.up, 1, -1);
        FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        // right
        _tabs.Add(Instantiate(m_scaleTabPrefab, m_targetTrans.position + m_targetTrans.right * (m_targetTrans.localScale[0] * curBound.size[0]/ 2 + FoamUtils.ScaleTabOffset), Quaternion.identity).GetComponent<FoamScaleTab>());
        _tabs[_tabs.Count - 1].SetTarget(this, 2, m_targetTrans, m_targetTrans.right, 0, 1);
        FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        // left
        _tabs.Add(Instantiate(m_scaleTabPrefab, m_targetTrans.position - m_targetTrans.right * (m_targetTrans.localScale[0] * curBound.size[0] / 2 + FoamUtils.ScaleTabOffset), Quaternion.identity).GetComponent<FoamScaleTab>());
        _tabs[_tabs.Count - 1].SetTarget(this, 3, m_targetTrans, m_targetTrans.right, 0, -1);
        FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        // forward
        _tabs.Add(Instantiate(m_scaleTabPrefab, m_targetTrans.position + m_targetTrans.forward * (m_targetTrans.localScale[2] * curBound.size[2] / 2 + FoamUtils.ScaleTabOffset), Quaternion.identity).GetComponent<FoamScaleTab>());
        _tabs[_tabs.Count - 1].SetTarget(this, 4, m_targetTrans, m_targetTrans.forward, 2, 1);
        FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);

        // back
        _tabs.Add(Instantiate(m_scaleTabPrefab, m_targetTrans.position - m_targetTrans.forward * (m_targetTrans.localScale[2] * curBound.size[2] / 2 + FoamUtils.ScaleTabOffset), Quaternion.identity).GetComponent<FoamScaleTab>());
        _tabs[_tabs.Count - 1].SetTarget(this, 5, m_targetTrans, m_targetTrans.forward, 2, -1);
        FoamUtils.CreateObjData(m_data, _tabs[_tabs.Count - 1].gameObject);
    }

    public void DestroyTabs()
    {
        int num = _tabs.Count;

        for (int i = 0; i < num; i++)
        {
            _tabs[i].CleanUp();
            FoamUtils.RemoveObjData(m_data, _tabs[i].gameObject);
            GameObject.Destroy(_tabs[i].gameObject);
        }

        _tabs = new List<FoamScaleTab>();
    }

    public void UpdateTabsLocation(Bounds curBound, int index)
    {
        if (_tabs.Count != 6) return;

        if (index !=0) _tabs[0].transform.position = m_targetTrans.position + m_targetTrans.up * (m_targetTrans.localScale[1] * curBound.size[1] / 2 + FoamUtils.ScaleTabOffset);
        if (index !=1) _tabs[1].transform.position = m_targetTrans.position - m_targetTrans.up * (m_targetTrans.localScale[1] * curBound.size[1] / 2 + FoamUtils.ScaleTabOffset);
        if (index !=2) _tabs[2].transform.position = m_targetTrans.position + m_targetTrans.right * (m_targetTrans.localScale[0] * curBound.size[0] / 2 + FoamUtils.ScaleTabOffset);
        if (index !=3) _tabs[3].transform.position = m_targetTrans.position - m_targetTrans.right * (m_targetTrans.localScale[0] * curBound.size[0] / 2 + FoamUtils.ScaleTabOffset);
        if (index !=4) _tabs[4].transform.position = m_targetTrans.position + m_targetTrans.forward * (m_targetTrans.localScale[2] * curBound.size[2] / 2 + FoamUtils.ScaleTabOffset);
        if (index !=5) _tabs[5].transform.position = m_targetTrans.position - m_targetTrans.forward * (m_targetTrans.localScale[2] * curBound.size[2] / 2 + FoamUtils.ScaleTabOffset);
    }
}
