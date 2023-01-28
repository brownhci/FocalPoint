using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamScaleTab : MonoBehaviour
{
    private Vector3 _prevPos;
    [HideInInspector]
    public Transform m_targetTrans;
    private Vector3 _scaleDir;
    private int _coord;
    private int _dirInt;
    private Quaternion _initRot;

    private FoamScaleParent _parent;
    private bool _isBeingGrabbed = false;
    private int _index = -1;

    private LineRenderer _line = null;

    // for undo redo
    private Vector3 _targetPrevScale; 
    private Vector3 _targetPrevPos;

    // Start is called before the first frame update
    void Start()
    {
        _prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 _currPos = transform.position;

        if (_currPos != _prevPos && _line)
        {
            _line.SetPosition(0, m_targetTrans.transform.position);
            _line.SetPosition(1, this.transform.position - this.transform.up * this.transform.localScale[1] / 2);
        }

        if (_currPos != _prevPos && m_targetTrans && _isBeingGrabbed)
        {
            Bounds curBound = m_targetTrans.GetComponent<MeshFilter>().sharedMesh.bounds; // or just mesh?

            Vector3 deltaVector = _currPos - _prevPos;
            Vector3 projectedDeltaVector = Vector3.Project(deltaVector, _dirInt * _scaleDir); // project to direction normal
            float delta = projectedDeltaVector.magnitude;

            float dot = Vector3.Dot(_dirInt * _scaleDir, deltaVector); // to determine the direction of the vector
            if (dot < 0)
            {
                delta = -1 * delta;
            }

            float newY = curBound.size[_coord] * m_targetTrans.localScale[_coord] + delta; // new Height of target transform
            float ratio = newY / curBound.size[_coord]; // ratio for scale

            if (ratio > 0.01f) // only update object scale if not too small
            {
                // update target transform. need to extend this line
                Vector3 newScale = m_targetTrans.localScale;
                for (int i = 0; i < 3; i++)
                {
                    if (i == _coord)
                    {
                        newScale[i] = ratio;
                    }
                }
                m_targetTrans.localScale = newScale;
                m_targetTrans.position += _dirInt * _scaleDir * delta / 2;

                _parent.m_targetTransPosDueToScaling = m_targetTrans.position;
                // update location of other tabs
                _parent.UpdateTabsLocation(curBound, _index);
            }
        }
        _prevPos = _currPos;
    }

    public void SetTarget(FoamScaleParent p, int index, Transform tar, Vector3 scaleDir, int coord, int dirInt)
    {
        m_targetTrans = tar;
        _scaleDir = scaleDir;
        _coord = coord;
        _dirInt = dirInt;
        //transform.rotation = Quaternion.FromToRotation(transform.up, dirInt * scaleDir);

        transform.rotation = m_targetTrans.rotation;
        if (coord == 1 && dirInt == -1) { transform.Rotate(Vector3.right, 180, Space.Self); }
        if (coord == 0) { transform.Rotate(Vector3.back, 90 * dirInt, Space.Self); }
        if (coord == 2) { transform.Rotate(Vector3.right, 90 * dirInt, Space.Self); }

        _initRot = transform.rotation;
        _index = index;
        _parent = p;

        _line = Instantiate(_parent.m_linePrefab).GetComponent<LineRenderer>();
        _line.SetPosition(0, m_targetTrans.transform.position);
        _line.SetPosition(1, this.transform.position - this.transform.up * this.transform.localScale[1] / 2);
    }

    public void OnGrabStart()
    {
        _isBeingGrabbed = true;

        _targetPrevScale = m_targetTrans.localScale; // undo redo;
        _targetPrevPos = m_targetTrans.position;
    }

    public void OnGrabStop()
    {
        Bounds curBound = m_targetTrans.GetComponent<MeshFilter>().sharedMesh.bounds; // or just mesh?
        transform.position = m_targetTrans.position + _dirInt * _scaleDir * (m_targetTrans.localScale[_coord] * curBound.size[_coord] / 2 + FoamUtils.ScaleTabOffset);
        transform.rotation = _initRot;

        //_parent.UpdateTabsLocation(curBound, _index);
        _isBeingGrabbed = false;

        // undo redo
        Debug.Log("Scaling added to stack");
        if (_targetPrevPos != m_targetTrans.position && _targetPrevScale != m_targetTrans.localScale)
        {
            ICommand scaleAction = new CommandScale(m_targetTrans.gameObject, _targetPrevScale, _targetPrevPos, m_targetTrans.localScale, m_targetTrans.position);
            UndoRedoManager.AddNewAction(scaleAction);
        }
        
    }

    public void CleanUp()
    {
        if (_line && _line.gameObject)
        {
            GameObject.Destroy(_line.gameObject);
        }
    }
}
