using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ScaleTabTest : MonoBehaviour
{
    private Vector3 _prevPos;
    [HideInInspector]
    public Transform m_targetTrans;
    private Vector3 _scaleDir;
    private Vector3 _perpDir;
    private int _coord;
    private int _dirInt;
    private Quaternion _initRot;

    private ScaleParentTest _parent;
    private bool _isBeingGrabbed = false;
    private int _index = -1;

    private LineRenderer _line = null;

    // Start is called before the first frame update
    void Start()
    {
        _prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 _currPos = transform.position;

        //    if (_currPos != _prevPos && _line)
        //    {
        //        _line.SetPosition(0, m_targetTrans.transform.position);
        //        _line.SetPosition(1, this.transform.position - this.transform.up * this.transform.localScale[1] / 2);
        //    }

        if (_currPos != _prevPos)
        {
            //transform.right = _dirInt * _perpDir;
            //transform.RotateAround(transform.up, Vector3.Angle(transform.right, _dirInt * _perpDir));

            //transform.up = _dirInt * _scaleDir;

            //        Bounds curBound = m_targetTrans.GetComponent<MeshFilter>().sharedMesh.bounds; // or just mesh?

            //        Vector3 deltaVector = _currPos - _prevPos;
            //        Vector3 projectedDeltaVector = Vector3.Project(deltaVector, _dirInt * _scaleDir); // project to direction normal
            //        float delta = projectedDeltaVector.magnitude;

            //        float dot = Vector3.Dot(_dirInt * _scaleDir, deltaVector); // to determine the direction of the vector
            //        if (dot < 0)
            //        {
            //            delta = -1 * delta;
            //        }

            //        float newY = curBound.size[_coord] * m_targetTrans.localScale[_coord] + delta; // new Height of target transform
            //        float ratio = newY / curBound.size[_coord]; // ratio for scale

            //        // update target transform. need to extend this line
            //        Vector3 newScale = m_targetTrans.localScale;
            //        for (int i = 0; i < 3; i++)
            //        {
            //            if (i == _coord)
            //            {
            //                newScale[i] = ratio;
            //            }
            //        }
            //        m_targetTrans.localScale = newScale;
            //        m_targetTrans.position += _dirInt * _scaleDir * delta / 2;

            //        // update location of other tabs
            //        _parent.UpdateTabsLocation(curBound, _index);
        }
        _prevPos = _currPos;
    }

    public void SetTarget(ScaleParentTest p, int index, Transform tar, Vector3 scaleDir, Vector3 perpDir, int coord, int dirInt)
    {
        m_targetTrans = tar;
        _scaleDir = scaleDir;
        _perpDir = perpDir;
        _coord = coord;
        _dirInt = dirInt;

        //Quaternion rot = Quaternion.AngleAxis(Vector3.Angle(transform.right, _perpDir), transform.up) * m_targetTrans.rotation;

        transform.rotation = m_targetTrans.rotation;
        if (coord == 1 && dirInt == -1) { transform.Rotate(Vector3.right, 180, Space.Self);  }
        if (coord == 0) { transform.Rotate(Vector3.back, 90 * dirInt, Space.Self); }
        if (coord == 2) { transform.Rotate(Vector3.right, 90 * dirInt, Space.Self);  }

        //_initRot = transform.rotation;
        _index = index;
        _parent = p;

        //_line = Instantiate(_parent.m_linePrefab).GetComponent<LineRenderer>();
        //_line.SetPosition(0, m_targetTrans.transform.position);
        //_line.SetPosition(1, this.transform.position - this.transform.up * this.transform.localScale[1] / 2);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 direction = transform.up * 0.1f;
        Gizmos.DrawRay(transform.position, direction);

        Gizmos.color = Color.green;
        Vector3 direction2 = transform.right * 0.1f;
        Gizmos.DrawRay(transform.position, direction2);
    }
}
