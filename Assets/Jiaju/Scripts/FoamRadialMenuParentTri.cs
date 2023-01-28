using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamRadialMenuParentTri : FoamRadialMenuParent
{
    private Vector3 m_bound_UppLeft;
    private Vector3 m_bound_UppRight;
    private Vector3 m_bound_Low;

    // Start is called before the first frame update
    protected override void Start()
    {
        m_options.Add(this.transform.GetChild(0).GetComponent<FoamRadialManager>());
        m_options.Add(this.transform.GetChild(1).GetComponent<FoamRadialManager>());
        m_options.Add(this.transform.GetChild(2).GetComponent<FoamRadialManager>());
        m_optionCenter = this.transform.GetChild(3).GetComponent<FoamRadialCenterManager>();

        if (IsToolMenu)
        {
            m_toolOptionInUse = m_options[m_defaultOption];
        }
    }

    public override MenuRegion RegionDetection(Vector3 palmPos_curr)
    {
        Vector3 local_curr = Vector3.ProjectOnPlane(this.transform.InverseTransformPoint(palmPos_curr), Vector3.forward) - m_palmPos_init;

        MenuRegion region = FoamUtils.checkMenuRegionTri(local_curr, m_menu_center, m_bound_UppLeft, m_bound_UppRight, m_bound_Low, inner_radius);

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

            case MenuRegion.MIDDLE:
                this.HighlightSprite(-1);

                break;
        }

        return region;
    }

    public override void RecordPalmPosInit(Vector3 init)
    {
        m_palmPos_init = Vector3.ProjectOnPlane(this.transform.InverseTransformPoint(init), Vector3.forward);
        m_menu_center = Vector3.ProjectOnPlane(this.transform.InverseTransformPoint(this.transform.position), Vector3.forward);

        //location detection
        m_bound_UppLeft = m_menu_center + Vector3.up * outer_radius * Mathf.Sin(30.0f * Mathf.Deg2Rad) - Vector3.right * outer_radius * Mathf.Cos(30.0f * Mathf.Deg2Rad);

        m_bound_UppRight = m_menu_center + Vector3.up * outer_radius * Mathf.Sin(30.0f * Mathf.Deg2Rad) + Vector3.right * outer_radius * Mathf.Cos(30.0f * Mathf.Deg2Rad);

        m_bound_Low = m_menu_center - Vector3.up * outer_radius;
    }
}
