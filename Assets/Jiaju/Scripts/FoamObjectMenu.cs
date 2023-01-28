using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamObjectMenu : MonoBehaviour
{

    public FoamDataManager m_data;
    public GameObject m_optionDelete;
    public GameObject m_optionCopy;
    public GameObject m_optionClose;
    private List<FoamIconManager> _iconMgrs = new List<FoamIconManager>();


    private FoamIconManager _currentActiveIcon = null;

    private int _hash_objMenuClosedBool = Animator.StringToHash("ObjMenuClosedBool");

    // Start is called before the first frame update
    void Start()
    {
        _iconMgrs.Add(m_optionDelete.GetComponent<FoamDeleteIcon>());
        _iconMgrs.Add(m_optionCopy.GetComponent<FoamCopyIcon>());
        _iconMgrs.Add(m_optionClose.GetComponent<FoamCloseIcon>());

    }

    // Update is called once per frame
    void Update()
    {
        // find active icon
        if (!_currentActiveIcon)
        {
            for (int i = 0; i < _iconMgrs.Count; i++)
            {
                _iconMgrs[i].ActivateIcon();
            }
            //Debug.Log("ICONN 1st no active icon");

            for (int i = 0; i < _iconMgrs.Count; i++)
            {
                // if there is a collision already
                if (_iconMgrs[i].IndexColliderCount > 0)
                {
                    // activate current icon and disable other icons
                    _currentActiveIcon = _iconMgrs[i];
                    //Debug.Log("ICONN active icon: " + _currentActiveIcon.gameObject.name);

                    for (int j = 0; j < _iconMgrs.Count; j++)
                    {
                        if (j != i)
                        {
                            _iconMgrs[j].DeactivateIcon();
                        }
                    }
                    break; // end for-loop
                }
            }
        }

        if (!_currentActiveIcon) return; // if no active icon, return


        // if the current active icon is no longer active
        if (_currentActiveIcon.IndexColliderCount <= 0)
        {
            _currentActiveIcon = null;
            //Debug.Log("ICONN active icon null");

            for (int i = 0; i < _iconMgrs.Count; i++)
            {
                _iconMgrs[i].ActivateIcon();
            }
        }


        // Perform action
        if (_currentActiveIcon)
        {
            //Debug.Log("ICONN active icon dwell: " + _currentActiveIcon.IndexColliderCount);
            if (_currentActiveIcon.IsHandDwell())
            {
                _currentActiveIcon.PerformAction();
                for (int j = 0; j < _iconMgrs.Count; j++)
                {  
                    _iconMgrs[j].DeactivateIcon();
                }
                _currentActiveIcon = null;
                m_data.StateMachine.SetBool(_hash_objMenuClosedBool, true);

            }
        }
    }
}
