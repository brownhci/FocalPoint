using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portalble;

public class EvalStuff : MonoBehaviour
{

    private GameObject cylinder;
    public GameObject obj;
    public SelectionDataManager m_sDM;
    public Canvas canvas;
    public Transform planePrefab;

    public FocusTaskController m_fTC;

    public Transform roundPrefab;

    //testing
    public Transform groundLEGO;

    public bool IsCreatingFigures;

    private bool isPlaced = false;

    private Transform _cen = null;

    // Start is called before the first frame update
    void Start()
    {
        cylinder = GameObject.Find("focusCylinder(Clone)");
        //if(cylinder) CastToGround();
    }


    // Update is called once per frame
    void Update()
    {
        if (!cylinder)
        {
            cylinder = GameObject.Find("focusCylinder(Clone)");
            //CastToGround();
        }

        if(cylinder && !isPlaced)
        {
            Debug.Log("Did Place");
            //CastToGround();
            isPlaced = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (IsCreatingFigures)
            {
                GenObjsForFigure();
            }
            else
            {
                CastToGround();
            }
            
        }

        //testGround();
    }

    void CastToGround()
    {
        RaycastHit hit;

        int layerMask = 1 << 14;
        layerMask = ~layerMask;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (cylinder)
        {
            ray = new Ray(cylinder.transform.position, cylinder.transform.TransformDirection(Vector3.up));
        }

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("Did Hit");

            if (m_sDM.TaskOnePlacePrefab != null)
            {
                m_fTC.GenerateObjsFromHit(m_sDM.TaskOnePlacePrefab, hit.point, Quaternion.identity);
            }
        }
    }



    private void GenObjsForFigure()
    {

        RaycastHit hit;

        int layerMask = 1 << 14;
        layerMask = ~layerMask;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (cylinder)
        {
            ray = new Ray(cylinder.transform.position, cylinder.transform.TransformDirection(Vector3.up));
        }

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            Debug.Log("Did Hit");

            if (m_sDM.TaskOnePlacePrefab != null)
            {
                Transform cen = Instantiate(m_sDM.TaskOnePlacePrefab, hit.point + Vector3.up * FocusUtils.HorizontalHelperPlaneOffset, Quaternion.identity);
                cen.gameObject.GetComponent<Renderer>().material.color = FocusUtils.ObjNormalColor;
                m_sDM.SceneObjects.Add(cen.gameObject);

                int num = 6; // change this number to change number of prefabs
                float offset_test = m_sDM.TaskOnePlacePrefab.transform.localScale[0] * 1.4f;

                List<GameObject> objs = new List<GameObject>();

                for (int k = 0; k < 3; k++)
                {
                    for (int i = -num; i < num + 1; i++)
                    {
                        for (int j = -num; j < num + 1; j++)
                        {
                            //if (i == 0 && j == 0) continue;
                            Transform obj = Instantiate(m_sDM.TaskOnePlacePrefab, cen.position + i * offset_test * cen.right - j * offset_test * cen.forward + cen.localScale[1] * k * cen.up, cen.rotation);
                            obj.gameObject.GetComponent<Portalble.Selectable>().UpdateMatColors(FocusUtils.ObjNormalColor);
                            objs.Add(obj.gameObject);
                            m_sDM.SceneObjects.Add(obj.gameObject);
                        }
                    }
                }
                int idx = Random.Range(0, objs.Count);
                objs[idx].GetComponent<Portalble.Selectable>().SetAsTarget();
            }
        }
    }

    //private void testGround()
    //{
    //    Debug.DrawRay(groundLEGO.position, groundLEGO.up, Color.green);
    //}


    //private void GenLegoPiece()
    //{
    //    if (roundPrefab)
    //    {
    //        RaycastHit hit;

    //        int layerMask = 1 << 14;
    //        layerMask = ~layerMask;

    //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //        if (cylinder)
    //        {
    //            ray = new Ray(cylinder.transform.position, cylinder.transform.TransformDirection(Vector3.up));
    //        }

    //        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
    //        {
    //            Transform cen = Instantiate(roundPrefab, hit.point + Vector3.up * 0.05f, Quaternion.identity);
    //            //m_sDM.SceneObjects.Add(cen.gameObject);
    //            cen.gameObject.SetActive(false); //maybe need to destroy it

    //            m_fTC.LegoGeneration(cen);
    //        }
    //    }
    //}

}