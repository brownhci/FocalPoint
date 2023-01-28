using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portalble;
using Portalble.Functions.Grab;

public class LegoController : MonoBehaviour
{

    public Transform visualPrefab;
    private List<Snappable> _snappables = new List<Snappable>();
    private GameObject _objectToSnap = null;
    private GameObject _snapVisual = null;

    private int _minPosID = 0;
    private Snappable _closestSnappable = null;

    private bool _isSnapVisualCreated = false;

    public SelectionDataManager m_sDM;

    // Start is called before the first frame update
    void Start()
    {
        //_allSnapPoses = m_testSnap.m_snapPositions;

        
    }


    // Update is called once per frame
    void Update()
    {
        if (m_sDM.TaskNumber != 2) { return; }

        if (_snappables.Count != 9)
        {
            UpdateSnappables();
        }

        // update what object is being grabbed
        Grabable tar = Grab.Instance.GetGrabbingObject();
        if (tar)
        {
            _objectToSnap = tar.gameObject;
        }
        else
        {
            _objectToSnap = null;
            CleanUpSnapVisual();
        }


        // only start snapping if close enough, otherwise turn off visual
        if (_objectToSnap && IsObjCloseEnough()) {
            FindSnapSpot();
            Debug.Log("TURNON close enough");
        } else
        {
            CleanUpSnapVisual();
            Debug.Log("TURNON not close");
        }
    }


    public void UpdateSnappables()  
    {
        GameObject[] snapObjs = GameObject.FindGameObjectsWithTag("legoSnappable");
        Debug.Log("Snapsize snapObjs: " + snapObjs.Length);

        _snappables.Clear();

        for (int i = 0; i < snapObjs.Length; i++)
        {
            _snappables.Add(snapObjs[i].GetComponent<Snappable>());
        }
    }


    private void FindSnapSpot()
    {

        //Debug.Log("Snapsize: ");

        // find closest place to snap to 
        float minObjDis = 999999f;

        for (int i = 0; i < _snappables.Count; i++)
        {
            float currDis = Vector3.Distance(_objectToSnap.transform.position, _snappables[i].transform.position);
            if (currDis < minObjDis)
            {
                minObjDis = currDis;
                _closestSnappable = _snappables[i];
            }
        }


        // find cloest point in closest snappable
        float minDis = 999999f;

        for (int i = 0; i < _closestSnappable.m_snapPositions.Count; i++)
        {
            if (_closestSnappable.m_isPositionCapped[i]) continue;

            float currDis = Vector3.Distance(_objectToSnap.transform.position, _closestSnappable.m_snapPositions[i]);

            if (currDis < minDis)
            {
                minDis = currDis;
                _minPosID = i;
            }
        }

        VisualizeSnapSpot(_closestSnappable.m_snapPositions[_minPosID], _closestSnappable.transform.rotation);
    }


    public void SnapObject()
    {
        if (!_objectToSnap || !_closestSnappable || !IsObjCloseEnough()) return;
        _objectToSnap.transform.position = _closestSnappable.m_snapPositions[_minPosID];
        _objectToSnap.transform.rotation = _closestSnappable.transform.rotation;
        _objectToSnap.GetComponent<Rigidbody>().isKinematic = true;

        _closestSnappable.UpdateSnapPosition(_objectToSnap, _minPosID);
        Debug.Log("Snapsize: Snapped!!");
    }


    public void ObjectTakenAway(GameObject obj)
    {
        for (int i = 0; i < _snappables.Count; i++)
        {
            _snappables[i].MoveSnapPositionAndObjs(obj);
        }

        UpdateSnapVisual(obj);
    }


    private void UpdateSnapVisual(GameObject obj)
    {
        CleanUpSnapVisual();

        _snapVisual = Instantiate(visualPrefab.gameObject);
        _isSnapVisualCreated = true;
        Debug.Log("TURNON snap visual created");
    }



    private void VisualizeSnapSpot(Vector3 snapSpot, Quaternion rot)
    {
        if (_snapVisual && _snapVisual.activeInHierarchy)
        {
            _snapVisual.transform.position = snapSpot;
            _snapVisual.transform.rotation = rot;
        }
    }


    private void CleanUpSnapVisual()
    {
        if (_snapVisual)
        {
            GameObject.Destroy(_snapVisual);
            _snapVisual = null;
            _isSnapVisualCreated = false;
        }
    }


    private bool IsObjCloseEnough()
    {
        for (int i = 0; i < _snappables.Count; i++)
        {
            if (_snappables[i] && _objectToSnap)
            {
                if (Vector3.Distance(_objectToSnap.transform.position, _snappables[i].transform.position) < 0.05f)
                {
                    if (!_isSnapVisualCreated)
                    {
                        UpdateSnapVisual(_objectToSnap);
                    }
                    return true;
                }
            }
        }

        return false;
    }

}
