using System.Collections;
using System.Collections.Generic;
using Portalble;
using UnityEngine;
using UnityEngine.UI;

public class FocusTaskController : MonoBehaviour, ISaveable
{


    public SelectionDataManager m_sDM;
    public Transform planePrefab;

    public Transform housePrefab;
    public Transform bowlPrefab;
    public List<Transform> piecePrefabs;

    public LegoController m_legoController;

    // trial num and id
    public int m_numTrials = 10;
    public int m_currentTrialID = 0;

    public Camera m_firstPersonCamera;

    private List<ListVector3Wrapper> m_pilesPos = new List<ListVector3Wrapper>();
    private List<int> m_targetIndices = new List<int>();

    private Transform _levelPlane = null;

    private List<Color> _legoColors = new List<Color>();

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log(Application.persistentDataPath);
        _legoColors.Add(FocusUtils.LegoRed);
        _legoColors.Add(FocusUtils.LegoWhite);
        _legoColors.Add(FocusUtils.LegoOrange);
        _legoColors.Add(FocusUtils.LegoGreen);
        _legoColors.Add(FocusUtils.LegoPink);

        m_currentTrialID = m_sDM.CurrentTrialID;
    }


    // Update is called once per frame
    void Update()
    {

    }


    public void GenerateObjsFromHit(Transform placePrefab, Vector3 hitPosition, Quaternion hitRotation)
    {
        float plane_offset = FocusUtils.HorizontalHelperPlaneOffset;
        float obj_offset = plane_offset + placePrefab.transform.localScale[1] / 2;

        Vector3 planePos = hitPosition + hitRotation * Vector3.up * plane_offset;
        if (!_levelPlane)
        {
            _levelPlane = Instantiate(planePrefab, planePos, hitRotation);
        }
        else
        {
            _levelPlane.position = planePos;
            _levelPlane.rotation = hitRotation;
        }

        Transform cen = Instantiate(placePrefab, hitPosition + hitRotation * Vector3.up * obj_offset, hitRotation);
        cen.gameObject.SetActive(false);
        m_sDM.UtilityObjects.Add(cen.gameObject);

        FocusUtils.RemoveCurrentRecordedSceneObjects(m_sDM);
        if (m_sDM.TaskNumber == 1)
        {
            PileGeneration(cen, placePrefab);
            FocusUtils.SendGenericData("Task 1, TrialID, " + m_currentTrialID + ", created, " + FocusUtils.AddTimeStamp());
        }
        else if (m_sDM.TaskNumber == 2)
        {
            LegoGeneration(cen);
        }
        else if (m_sDM.TaskNumber == 3)
        {
            FigureObjGeneration(cen);
        }
        else if (m_sDM.TaskNumber == 4)
        {
            RobotGeneration(cen);
        }
        else if (m_sDM.TaskNumber == 5)
        {
            ModelGeneration(cen);
        }
    }


    // ############# Lego generation
    public void LegoGeneration(Transform cen)
    {
        if (!housePrefab || piecePrefabs.Count == 0) return;

        Vector3 camPos = m_firstPersonCamera.transform.position;
        Vector3 camAtCenPos = new Vector3(camPos[0], cen.position[1], camPos[2]);

        Vector3 camToCenVec = -(camAtCenPos - cen.position).normalized;

        cen.LookAt(cen.position + camToCenVec);

        Transform legoHouse = Instantiate(housePrefab, cen.position + cen.right * 0.1f, cen.rotation);
        m_sDM.UtilityObjects.Add(legoHouse.gameObject);

        Transform bowl = Instantiate(bowlPrefab, cen.position - cen.right * 0.13f, cen.rotation);
        m_sDM.UtilityObjects.Add(bowl.gameObject);

        // create required red and white round plates
        int required_plate_num = 6;
        for (int j = 0; j < 2; j++) // two colors
        {
            for (int i = 0; i < required_plate_num; i++) // required num each
            {
                Transform piece = Instantiate(piecePrefabs[0], bowl.position + bowl.up * 0.02f, Quaternion.identity);
                if (j == 1) piece.GetComponent<Portalble.Selectable>().UpdateMatColors(FocusUtils.LegoWhite);
                m_sDM.SceneObjects.Add(piece.gameObject);
            }
        }

        // round plates of other colors
        for (int j = 0; j < 3; j++) // three colors
        {
            for (int i = 0; i < 3; i++) // three pieces each
            {
                Transform piece = Instantiate(piecePrefabs[0], bowl.position + bowl.up * 0.03f, Quaternion.identity);
                piece.GetComponent<Portalble.Selectable>().UpdateMatColors(_legoColors[2+j]);
                m_sDM.SceneObjects.Add(piece.gameObject);
            }
        }


        // other kinds
        for (int j = 0; j < _legoColors.Count; j++) // five colors
        {
            for (int i = 0; i < 2; i++) // two pieces each
            {
                for (int k = 1; k < 5; k++) // four kinds
                {
                    Transform piece = Instantiate(piecePrefabs[k], bowl.position + bowl.up * 0.04f, Quaternion.identity);
                    piece.GetComponent<Portalble.Selectable>().UpdateMatColors(_legoColors[j]);
                    m_sDM.SceneObjects.Add(piece.gameObject);
                }
            }
        }        

        if (m_legoController) m_legoController.UpdateSnappables();
    }



    // ############# pile generation

    public void PileGeneration(Transform cen, Transform placePrefab)
    {
        // 50 cubes in 4 * 4 * 4
        float layerWidth = 4f * placePrefab.localScale[0] + 3f * 0.5f * placePrefab.localScale[0];
        float layer_height = placePrefab.localScale[1];
        int num = 4;

        BuildPlanesAround(cen, layerWidth, planePrefab, placePrefab.localScale[0]);

        // checks if pos data file exists
        if (FileManager.CheckIfFileExists(FocusUtils.PilePosFileName))
        {
            LoadPileGeneration(cen, placePrefab);
            return;
        }

        // if not, generate new data and save. do not directly create stuff here
        for (int j = 0; j < m_numTrials; j++)
        {
            int objNum = 0;
            List<Vector3> currPilePos = new List<Vector3>();
            for (int k = 0; k < num; k++)
            {
                for (int i = 0; i < num * num; i++)
                {
                    List<List<float>> lCoords = FocusUtils.GenerateLayerCoords(layerWidth, placePrefab.localScale[0], num);
                    objNum++;
                    currPilePos.Add(new Vector3(lCoords[0][i], lCoords[1][i], placePrefab.localScale[1] * (k)));
                }
            }

            int idx = Random.Range(0, num * num);
            m_targetIndices.Add(idx);

            // add to inner list
            ListVector3Wrapper wrapperList = new ListVector3Wrapper();
            wrapperList.InnerList = currPilePos;
            m_pilesPos.Add(wrapperList);
        }

        SaveJsonData(this);
        LoadPileGeneration(cen, placePrefab);
    }



    public void LoadPileGeneration(Transform cen, Transform placePrefab)
    {
        // need to consider prefab size
        Debug.Log("Loading pile");
        LoadJsonData(this);

        List<Transform> objsInPile = new List<Transform>();

        List<Vector3> currPilePos = m_pilesPos[m_currentTrialID].InnerList;
        for (int i = 0; i < currPilePos.Count; i++)
        {
            Transform newObj = Instantiate(placePrefab, cen.position + cen.forward * currPilePos[i][0] + cen.right * currPilePos[i][1] + cen.up * currPilePos[i][2], cen.rotation);
            newObj.gameObject.GetComponent<Portalble.Selectable>().UpdateMatColors(FocusUtils.ObjNormalColor);
            m_sDM.SceneObjects.Add(newObj.gameObject);
            objsInPile.Add(newObj);
        }

        objsInPile[m_targetIndices[m_currentTrialID]].gameObject.GetComponent<Portalble.Selectable>().SetAsTarget();
    }



    public void BuildPlanesAround(Transform cen, float layerWidth, Transform planePrefab, float placePrefabWidth)
    {
        Vector3 centerPos = cen.position + cen.forward * layerWidth / 2 + cen.right * layerWidth / 2;

        float offsetDis = layerWidth / 2f + placePrefabWidth / 1.5f;

        // front and back are correct on mobile
        Transform frontPlane = Instantiate(planePrefab, centerPos + cen.forward * offsetDis, Quaternion.identity);
        frontPlane.up = (centerPos - frontPlane.position).normalized;
        frontPlane.rotation = frontPlane.rotation * Quaternion.FromToRotation(frontPlane.forward, Vector3.up);
        m_sDM.VerticalHelperPlanes.Add(frontPlane.gameObject);

        Transform backPlane = Instantiate(planePrefab, centerPos - cen.forward * offsetDis, Quaternion.identity);
        backPlane.up = (centerPos - backPlane.position).normalized;
        backPlane.rotation = backPlane.rotation * Quaternion.FromToRotation(-backPlane.forward, Vector3.up);
        m_sDM.VerticalHelperPlanes.Add(backPlane.gameObject);


        Transform rightPlane = Instantiate(planePrefab, centerPos + cen.right * offsetDis, Quaternion.identity);
        rightPlane.up = (centerPos - rightPlane.position).normalized;
        rightPlane.rotation = rightPlane.rotation * Quaternion.FromToRotation(rightPlane.right, Vector3.up);
        m_sDM.VerticalHelperPlanes.Add(rightPlane.gameObject);


        Transform leftPlane = Instantiate(planePrefab, centerPos - cen.right * offsetDis, Quaternion.identity);
        leftPlane.up = (centerPos - leftPlane.position).normalized;
        leftPlane.rotation = leftPlane.rotation * Quaternion.FromToRotation(-leftPlane.right, Vector3.up);
        m_sDM.VerticalHelperPlanes.Add(leftPlane.gameObject);


        ResizeHelperPlanes(offsetDis / 1.5f);
    }



    private void ResizeHelperPlanes(float offsetDis)
    {
        int num = m_sDM.VerticalHelperPlanes.Count;

        if (num != 0)
        {
            for (int i = 0; i < num; i++)
            {
                m_sDM.VerticalHelperPlanes[i].transform.localScale = new Vector3(offsetDis, offsetDis, offsetDis);
            }
        }
    }


    private void FigureObjGeneration(Transform cen)
    {
        int num = 3; // change this number to change number of prefabs
        float offset_test = m_sDM.TaskOnePlacePrefab.transform.localScale[0] * 1.2f;

        List<GameObject> objs = new List<GameObject>();

        for (int k = 0; k < 4; k++)
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

    private void RobotGeneration(Transform cen)
    {
        //Transform obj = Instantiate(m_sDM.TaskOnePlacePrefab, cen.position, cen.rotation);
        //m_sDM.SceneObjects.Add(obj.gameObject);

        for (int i = 0; i < m_sDM.RobotParts.Count; i++)
        {
            Transform obj = Instantiate(m_sDM.RobotParts[i]);
            obj.position = obj.position + cen.position - cen.up * 0.51f;
            m_sDM.SceneObjects.Add(obj.gameObject);
        }

        //int children = obj.transform.childCount;
        //Debug.Log("In Robot child: " + children);
        //for (int i = 0; i < children; i++)
        //{
        //    Transform childTrans = obj.transform.GetChild(i);
        //    m_sDM.SceneObjects.Add(childTrans.gameObject);
        //    childTrans.parent = null;
        //}
        //Debug.Log("In Robot: " + m_sDM.SceneObjects.Count);
    }

    private void ModelGeneration(Transform cen)
    {
        Transform obj = Instantiate(m_sDM.TaskOnePlacePrefab, cen.position - cen.up * 0.51f, cen.rotation);

        int num = obj.transform.childCount;
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < num; i++)
        {
            children.Add(obj.transform.GetChild(i));
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].parent = null;
            m_sDM.SceneObjects.Add(children[i].gameObject);
        }

        GameObject.Destroy(obj.gameObject);
    }


    // ###################### Data Saving Section

    private static void SaveJsonData(FocusTaskController a_fTC)
    {
        SaveData sd = new SaveData();
        a_fTC.PopulateSaveData(sd);

        if (FileManager.WriteToFile(FocusUtils.PilePosFileName, sd.ToJson()))
        {
            Debug.Log("Save pile pos successful");
        }
    }

    private static void LoadJsonData(FocusTaskController a_fTC)
    {
        if (FileManager.LoadFromFile(FocusUtils.PilePosFileName, out var json))
        {
            SaveData sd = new SaveData();
            sd.LoadFromJson(json);

            a_fTC.LoadFromSaveData(sd);
            Debug.Log("Load file pos complete");
        }
    }

    public void PopulateSaveData(SaveData a_SaveData)
    {
        a_SaveData.m_pilesPos = m_pilesPos;
        a_SaveData.m_targetIndices = m_targetIndices;
    }

    public void LoadFromSaveData(SaveData a_SaveData)
    {
        m_pilesPos = a_SaveData.m_pilesPos;
        m_targetIndices = a_SaveData.m_targetIndices;
    }

    // ########### UI Utils
    public void ToggleTimeStamp(bool isStart)
    {
#if !UNITY_EDITOR
        FocusUtils.ToggleTimeStamp(!isStart);
#endif
    }


    public void UpdateTrialID(InputField userInput)
    {
        //Debug.Log("TrialID: " + userInput.text);
        bool isParsed;
        int currTrialID;

        isParsed = int.TryParse(userInput.text, out currTrialID);
        if (isParsed)
        {
            //parsed
            if (currTrialID >= 0 && currTrialID < m_numTrials)
            {
                m_currentTrialID = currTrialID;
                m_sDM.CurrentTrialID = m_currentTrialID;
                Debug.Log("TrialID: " + m_currentTrialID);
            }
        }
    }
}
