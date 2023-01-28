using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portalble;


public class Snappable : MonoBehaviour
{

    //public Transform m_testPrefab;
    public List<Vector3> m_snapPositions = new List<Vector3>();
    public List<bool> m_isPositionCapped = new List<bool>();
    private List<Vector3> _baseSnapPositions = new List<Vector3>();
    public int m_widthUnit = 12;
    public int m_depthUnit = 6;
    private float _heightIncrement = 0.0032f;

    private Dictionary<int, List<GameObject>> _snappedObjsAndPos = new Dictionary<int, List<GameObject>>();

    // Start is called before the first frame update
    void Start()
    {
        GenSnapPositions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmosSelected()
    {

    }


    public void UpdateSnapPosition(GameObject obj, int id)
    {
        // add obj to list of objs in a pos
        if (!_snappedObjsAndPos.ContainsKey(id))
        {
            List<GameObject> objs = new List<GameObject>();
            objs.Add(obj);
            _snappedObjsAndPos.Add(id, objs);
        } else
        {
            _snappedObjsAndPos[id].Add(obj);
        }

        Vector3 higherPos = m_snapPositions[id] + transform.up * _heightIncrement;
        m_snapPositions[id] = higherPos;

        // if obj is capped, this position is not available
        if (obj.GetComponent<Selectable>().IsLegoCapped)
        {
            m_isPositionCapped[id] = true;
        }
    }


    public void MoveSnapPositionAndObjs(GameObject obj)
    {
        List<int> keys = new List<int>(_snappedObjsAndPos.Keys);


        foreach (int key in keys)
        {
            // do something with entry.Value or entry.Key
            if (_snappedObjsAndPos[key].Contains(obj))
            {

                int objID = _snappedObjsAndPos[key].IndexOf(obj);
                Debug.Log("Snapsize OBJ TAKEN AWAY: " + objID);

                // if obj is capped and taken away, the pos is available
                if (obj.GetComponent<Selectable>().IsLegoCapped)
                {
                    m_isPositionCapped[key] = false;
                }

                // if obj not top of the list, move objs above it down
                if (objID != _snappedObjsAndPos[key].Count - 1)
                {
                    MoveObjsDown(_snappedObjsAndPos[key], objID);
                }

                // remove obj from list
                _snappedObjsAndPos[key].Remove(obj);

                // delete list if empty and reset snapping position
                if (_snappedObjsAndPos[key].Count == 0)
                {
                    m_snapPositions[key] = _baseSnapPositions[key];
                    _snappedObjsAndPos.Remove(key);
                } else
                {
                    m_snapPositions[key] = _snappedObjsAndPos[key][_snappedObjsAndPos[key].Count - 1].transform.position + transform.up * _heightIncrement;
                }
            }
        }
    }

    private void MoveObjsDown(List<GameObject> objs, int startID)
    {
        Debug.Log("Snapsize moving obj down");

        Vector3 storedPos;
        Vector3 destPos = objs[startID + 1].transform.position - transform.up * _heightIncrement;

        for (int i = startID; i < objs.Count - 1; i++)
        {
            storedPos = objs[i + 1].transform.position;
            objs[i + 1].transform.position = destPos;
            destPos = storedPos;
        }
    }


    public void GenSnapPositions() {

        Renderer rend = this.GetComponent<Renderer>();

        // A sphere that fully encloses the bounding box.
        Vector3 center = rend.bounds.center;
        float radius = rend.bounds.extents.magnitude;

        Mesh mesh = this.GetComponent<MeshFilter>().sharedMesh;
        if (mesh)
        {
            Vector3 min = mesh.bounds.min;
            Vector3 max = mesh.bounds.max;
            Transform transform = this.transform;
            Vector3 point1 = transform.TransformPoint(new Vector3(min.x, min.y, min.z));
            Vector3 point2 = transform.TransformPoint(new Vector3(min.x, min.y, max.z));
            //Vector3 point3 = transform.TransformPoint(new Vector3(min.x, max.y, min.z));
            //Vector3 point4 = transform.TransformPoint(new Vector3(min.x, max.y, max.z));
            Vector3 point5 = transform.TransformPoint(new Vector3(max.x, min.y, min.z));
            Vector3 point6 = transform.TransformPoint(new Vector3(max.x, min.y, max.z));
            //Vector3 point7 = transform.TransformPoint(new Vector3(max.x, max.y, min.z));
            //Vector3 point8 = transform.TransformPoint(new Vector3(max.x, max.y, max.z));

            Vector3 pointOrigin = point1;

            Vector3 dirWidth = Vector3.zero;
            Vector3 dirDepth = Vector3.zero;

            float dis12 = Vector3.Distance(point1, point2);
            float dis15 = Vector3.Distance(point1, point5);
            float width = 0f;
            float depth = 0f;

            if (dis12 < dis15)
            {
                width = dis15;
                depth = dis12;
                dirWidth = (point5 - point1).normalized;
                dirDepth = (point2 - point1).normalized;

            }
            else
            {
                width = dis12;
                depth = dis15;
                dirWidth = (point2 - point1).normalized;
                dirDepth = (point5 - point1).normalized;
            }

            float depthIncrement = depth / m_depthUnit;
            float widthIncrement = width / m_widthUnit;
            //float initialHeightOffset = this.transform.position[1] * 2 / 3;
            float initialHeightOffset = 0.005733334f;
            //Debug.Log("Snapsize: " + initialHeightOffset);

            for (int k = 0; k < 1; k++)
            {
                for (int i = 0; i < m_widthUnit; i++)
                {
                    for (int j = 0; j < m_depthUnit; j++)
                    {
                        Vector3 snapPoint = pointOrigin + dirDepth * depthIncrement * (j) + dirWidth * widthIncrement * (i) + transform.up * _heightIncrement * (k);
                        snapPoint = snapPoint + dirDepth * depthIncrement * 0.5f + dirWidth * widthIncrement * 0.5f + transform.up * initialHeightOffset; //offsets

                        m_snapPositions.Add(snapPoint);
                        _baseSnapPositions.Add(snapPoint);
                        m_isPositionCapped.Add(false);
                    }
                }
            }
        }
    }
}
