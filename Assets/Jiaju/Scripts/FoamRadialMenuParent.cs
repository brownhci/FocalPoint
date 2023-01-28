using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamRadialMenuParent : MonoBehaviour
{

    public bool IsToolMenu;
    public int m_defaultOption; // maybe don't need this

    protected FoamRadialCenterManager m_optionCenter;
    protected List<FoamRadialManager> m_options = new List<FoamRadialManager>();

    //protected FoamRadialManager m_previousSelectedOption = null;
    protected FoamRadialManager m_currentSelectedIcon = null; // for highlighting purposes
    protected MenuRegion m_currentRegion = MenuRegion.MIDDLE;

    protected FoamRadialManager m_toolOptionInUse = null;

    protected Vector3 m_palmPos_init = Vector3.zero; // projected space
    protected Vector3 m_menu_center = Vector3.zero;

    protected float inner_radius = 0.027f;
    protected float outer_radius = 1.0f;

    private Vector3 m_bound_UppL;
    private Vector3 m_bound_UppR;
    private Vector3 m_bound_LowL;
    private Vector3 m_bound_LowR;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        m_options.Add(this.transform.GetChild(0).GetComponent<FoamRadialManager>());
        m_options.Add(this.transform.GetChild(1).GetComponent<FoamRadialManager>());
        m_options.Add(this.transform.GetChild(2).GetComponent<FoamRadialManager>());
        m_options.Add(this.transform.GetChild(3).GetComponent<FoamRadialManager>());
        m_optionCenter = this.transform.GetChild(4).GetComponent<FoamRadialCenterManager>();

        if (IsToolMenu)
        {
            m_toolOptionInUse = m_options[m_defaultOption];
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitiateIcons()
    {
        if (!IsToolMenu) return;

        if (m_toolOptionInUse && m_toolOptionInUse.IsToolOption)
        {
            m_toolOptionInUse.SetIconInUse();
        }
    }

    public virtual MenuRegion RegionDetection(Vector3 palmPos_curr)
    {
        Vector3 local_curr = Vector3.ProjectOnPlane(this.transform.InverseTransformPoint(palmPos_curr), Vector3.forward) - m_palmPos_init;

        MenuRegion region = FoamUtils.checkMenuRegion(local_curr, m_menu_center, m_bound_UppL, m_bound_UppR, m_bound_LowL, m_bound_LowR, inner_radius);

        if (region == m_currentRegion) { return region; }
        m_currentRegion = region;

        switch (region)
        {
            case MenuRegion.UPPER:
                this.HighlightSprite(0);
                break;


            case MenuRegion.RIGHT:
                this.HighlightSprite(1);
                break;


            case MenuRegion.LOWER:
                this.HighlightSprite(2);
                break;


            case MenuRegion.LEFT:
                this.HighlightSprite(3);
                break;


            case MenuRegion.MIDDLE:
                this.HighlightSprite(-1);

                break;
        }

        return region;
    }


    // will only be called if region changes
    public void HighlightSprite(int which)
    {
        Debug.Log("--------region changed");
        if (m_currentSelectedIcon)
        {
            m_currentSelectedIcon.DeHighlightIcon();
        }

        if (which == -1)
        {
            m_optionCenter.DeHighlightCenter();
            return;
        }

        m_currentSelectedIcon = m_options[which];
        m_currentSelectedIcon.HightlightIcon();
    }



    public virtual void RecordPalmPosInit(Vector3 init)
    {
        m_palmPos_init = Vector3.ProjectOnPlane(this.transform.InverseTransformPoint(init), Vector3.forward);
        m_menu_center = Vector3.ProjectOnPlane(this.transform.InverseTransformPoint(this.transform.position), Vector3.forward);

        //location detection
        m_bound_UppL = m_menu_center + Vector3.up * outer_radius - Vector3.right * outer_radius;
        m_bound_UppR = m_menu_center + Vector3.up * outer_radius + Vector3.right * outer_radius;
                                             
        m_bound_LowL = m_menu_center - Vector3.up * outer_radius - Vector3.right * outer_radius;
        m_bound_LowR = m_menu_center - Vector3.up * outer_radius + Vector3.right * outer_radius;
    }

    public void SetToolOptionInUse(int which)
    {
        if (m_toolOptionInUse) m_toolOptionInUse.SetIconOG();
        if (m_options.Count == 0) this.Start();
        m_toolOptionInUse = m_options[which];
    }
}