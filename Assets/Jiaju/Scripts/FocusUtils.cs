using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kalman;
using Portalble;
using UnityEngine;


public static class FocusUtils
{

    public static readonly Vector3 NullVector3 = new Vector3(-99999f, -99999f, -99999f);

    /// <summary>
    /// Constants
    /// </summary>
    // Depth cue
    public static readonly float NearHandDis = 0.11f; // to be adjusted based on user preference.
    public static readonly float FarHandDis = 0.21f;  // to be adjusted based on user preference.
    public static readonly float FarAlpha = 0.2f;
    public static readonly float NearAlpha = 1.0f;
    public static readonly float OccludingObjAlpha = 0.1f;

    // Object colors for user task and selection aid
    public static readonly Color ObjNormalColor = new Color(1f, 200f / 255f, 0f, 1.0f);
    public static readonly Color ObjTargetColor = new Color(85f / 255f, 180f / 255f, 1f, 1.0f);
    //public static readonly Color ObjFocusedColor = new Color(1f, 200f / 255f, 0f, 1.0f);
    //public static readonly Color ObjTargetFocusedColor = new Color(85f / 255f, 180f / 255f, 1f, 1.0f);

    // Lego Colors
    public static readonly Color LegoOrange = new Color(0.84f, 0.47f, 0.14f, 1.0f);
    public static readonly Color LegoGreen = new Color(0.35f, 0.67f, 0.25f, 1.0f);
    public static readonly Color LegoRed = new Color(0.71f, 0.0f, 0.0f);
    public static readonly Color LegoWhite = new Color(1.0f, 1.0f, 1.0f);
    public static readonly Color LegoPink = new Color(1.0f, 0.5047f, 0.8778f);

    //public static readonly Color ObjRankedColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    public static readonly Color ObjRankedColor = new Color(0.9f, 0.0f, 0.0f, 1.0f);

    public static readonly Vector3 SmallObjSnapExpandedScale = new Vector3(0.06f / 0.01f, 0.06f / 0.007f, 0.06f / 0.01f);

    public static readonly float HorizontalHelperPlaneOffset = 0.05f;

    //File IO
    public static readonly string PilePosFileName = "PilePos.dat";

    public static Color GetColor(Renderer r, string color_name = "")
    {
        Color c = new Color(0, 0, 0);

        if (color_name == "")
        {
            if (r.material.HasProperty("_AlbedoColor"))
                return r.material.GetColor("_AlbedoColor");
            else if (r.material.color != null)
                return r.material.color;
        }
        return c;
    }

    public static Vector3 WorldToScreenSpace(Vector3 worldPos)
    {
        return Camera.main.WorldToScreenPoint(worldPos);
    }


    public static Vector2 WorldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, null, out movePos);

        //Convert the local point to world point
        //return parentCanvas.transform.TransformPoint(movePos);
        return movePos;
    }


    public static Vector2 CalcFocusCenter(Queue<KeyValuePair<Vector2, long>> markers, int markerQueueLimit)
    {
        if (markers.Count < markerQueueLimit) return new Vector2(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2); //default


        float x_num_sum = 0f;
        float y_num_sum = 0f;
        float num = 0;

        // average center
        IEnumerator<KeyValuePair<Vector2, long>> marker_enum = markers.GetEnumerator();

        while (marker_enum.MoveNext())
        {
            num++;
            x_num_sum += marker_enum.Current.Key.x;
            y_num_sum += marker_enum.Current.Key.y;
        }

        return new Vector2(x_num_sum / num, y_num_sum / num);

        // time weighted center

        //float x_num_sum = 0f;
        //float y_num_sum = 0f;
        //float weight_sum = 0f;

        //IEnumerator<KeyValuePair<Vector2, long>> marker_enum = markers.GetEnumerator();

        //while (marker_enum.MoveNext())
        //{
        //    float weight = (marker_enum.Current.Value - markers.Peek().Value) + 1.0f;

        //    weight_sum += weight;

        //    x_num_sum += marker_enum.Current.Key.x * weight;
        //    y_num_sum += marker_enum.Current.Key.y * weight;
        //}

        //return new Vector3(x_num_sum / weight_sum, y_num_sum / weight_sum, 0f);
    }


    public static GameObject RankFocusedObjects(List<GameObject> focusedObjects, Vector3 cylinderPosition)
    {

        if (focusedObjects.Count == 0)
        {
            return null;
        }

        GameObject num_one = null;
        float min_dis = 9999999f;

        for (int i = 0; i < focusedObjects.Count; i++)
        {
            float dis = Vector3.Distance(WorldToScreenSpace(focusedObjects[i].transform.position), WorldToScreenSpace(cylinderPosition));

            if (dis < min_dis)
            {
                min_dis = dis;
                num_one = focusedObjects[i];
            }
        }

        return num_one;
    }


    public static GameObject RankFocusedObjects(Vector3 cylinderPosition, SelectionDataManager sDM, Canvas canvas, SimpleKalmanWrapper kalmanSelector)
    {
        // merge focused objects and peripheral objects together
        List<GameObject> candidateObjects = sDM.FocusedObjects.Concat(sDM.PeripheralObjects).ToList();

        if (candidateObjects.Count == 0)
        {
            return null;
        }

        GameObject num_one = null;
        float max_score = -9999999f;
        float max_objCylPortion = 0f;
        float max_objHandPortion = 0f;

        for (int i = 0; i < candidateObjects.Count; i++)
        {

            if (sDM.OccludingObjects.Contains(candidateObjects[i])) continue;

            Vector3 objPos = candidateObjects[i].transform.position;

            // cylinder dis should be vector 2. measured in pixel size (so iphone Xs Max). 0 to cylinder radius (around 600)
            float objCylinderDis = Vector2.Distance(WorldToUISpace(canvas, objPos), WorldToUISpace(canvas, cylinderPosition));

            float objHandDis = 0.0f; // measured in meter. 0.0 to 0.20 (20cm ish)

            Vector3 handPos = GetIndexThumbPos(sDM);
            if (handPos != NullVector3) objHandDis = Vector3.Distance(objPos, handPos);

            //float objCylPortion = (1.0f / objCylinderDis) * 20000f;
            //float objHandPortion = 1.0f / Mathf.Pow(objHandDis, 3.0f);

            float objCylPortion = (1.0f / (objCylinderDis));
            float objHandPortion = 1.0f / (objHandDis * 7400);
            //float objHandPortion = 1.0f / (objHandDis * 600000);

            float score = objCylPortion + objHandPortion;

            //Debug.Log("DATAA cy obj score: " + objCylinderDis + " , " + objHandDis + " , " + WorldToUISpace(canvas, objPos));


            if (score > max_score)
            {
                max_score = score;
                max_objCylPortion = objCylPortion;
                max_objHandPortion = objHandPortion;

                num_one = candidateObjects[i];
            }
        }

        //Debug.Log("DATAA cy obj score: " + max_objCylPortion + " , " + max_objHandPortion + " , " + max_score);

        ////kalman
        //Vector3 kalman_ranking_vector = kalmanSelector.Update(new Vector3((float)sDM.SceneObjects.IndexOf(num_one), 0f, 0f));
        //int kalman_ranking_index = (int)kalman_ranking_vector[0];
        //num_one = sDM.SceneObjects[kalman_ranking_index];

        return num_one;
    }


    //public static void UpdateLinePos(LineRenderer line, Collider other, GameObject ActivePalm)
    //{
    //    line.SetPosition(0, ActivePalm.transform.position);
    //    line.SetPosition(1, other.gameObject.transform.position);
    //}


    public static void ToggleTimeStamp(bool isStart)
    {
        if (Jetfire.IsConnected2())
        {
            string message = "";

            if (isStart)
            {
                message += "Time start, ";
            }
            else
            {
                message += "Time end, ";
            }

            message += AddTimeStamp();

            Jetfire.SendMsg2(message);
            //Debug.Log("JETFIRE start/end");
        }

    }


    public static string AddTimeStamp()
    {
        return System.DateTime.Now.ToString("MM / dd / yyyy hh: mm: ss") + " , " + System.DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
    }


    public static float LinearMapReverse(float input, float ogMin, float ogMax, float tarMin, float tarMax)
    {
        float t = Mathf.Abs(ogMax - input) / Mathf.Abs(ogMax - ogMin);
        return Mathf.Lerp(tarMin, tarMax, t);
    }


    public static void UpdateMaterialAlpha(Renderer renderer, float alpha)
    {
        if (isUnityDefaultMat(renderer.material.shader))
        {
            Color currColor = renderer.material.color;
            renderer.material.color = new Vector4(currColor.r, currColor.g, currColor.b, alpha);
        }
        else
        {
            Color currColor = renderer.material.GetColor("_AlbedoColor");
            renderer.material.SetColor("_AlbedoColor", new Vector4(currColor.r, currColor.g, currColor.b, alpha));
        }
    }



    public static bool isUnityDefaultMat(Shader s)
    {
        return !s.name.Contains("Dither");
    }



    public static Vector3 GetIndexThumbPos(SelectionDataManager sDM)
    {
        if (!sDM.ActiveHand) return NullVector3;

        Vector3 indexPos = sDM.ActiveIndex.transform.position;
        Vector3 thumbPos = sDM.ActiveThumb.transform.position;

        float factor = 0.5f;

        float x = (indexPos.x + thumbPos.x) * factor;
        float y = (indexPos.y + thumbPos.y) * factor;
        float z = (indexPos.z + thumbPos.z) * factor;

        //Debug.Log("IndexThumbDis: " + Vector3.Distance(indexPos, thumbPos).ToString("F7"));

        return new Vector3(x, y, z);
    }


    public static Vector3 GetPosForOccludingGeo(SelectionDataManager sDM, Vector3 camForward)
    {
        if (!sDM.ActiveHand) return NullVector3;

        Vector3 indexThumbPos = GetIndexThumbPos(sDM);
        Vector3 res = indexThumbPos - camForward * sDM.MaxSnapDis;
        //Vector3 res = indexThumbPos - camForward * sDM.MaxSnapDis * 10f;
        //Vector3 palmPos = sDM.ActivePalm.transform.position;

        //float factor = 0.5f;

        //float x = (indexThumbPos.x + palmPos.x) * factor;
        //float y = (indexThumbPos.y + palmPos.y) * factor;
        //float z = (indexThumbPos.z + palmPos.z) * factor;

        return res;
        //return GetIndexThumbPos(sDM);
    }


    public static bool IsFingersCloseEnough(SelectionDataManager sDM)
    {
        if (!sDM.ActiveHand) return false;

        Vector3 indexPos = sDM.ActiveIndex.transform.position;
        Vector3 thumbPos = sDM.ActiveThumb.transform.position;

        if (Vector3.Distance(indexPos, thumbPos) < 0.035f)
        {
            return true;
        }

        return false;
    }


    public static void SetObjsToAlpha(List<GameObject> objs, float alpha)
    {
        for (int i = 0; i < objs.Count; i++)
        {
            Color curr_color = objs[i].GetComponent<Renderer>().material.color;
            curr_color = new Color(curr_color.r, curr_color.g, curr_color.b, alpha);
            objs[i].GetComponent<Renderer>().material.color = curr_color;
        }
    }

    public static Vector3 GetDimensions(GameObject obj)
    {
        Vector3 min = Vector3.one * Mathf.Infinity;
        Vector3 max = Vector3.one * Mathf.NegativeInfinity;

        Mesh mesh = obj.GetComponent<MeshFilter>().mesh;

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 vert = mesh.vertices[i];
            min = Vector3.Min(min, vert);
            max = Vector3.Max(max, vert);
        }

        // the size is max-min multiplied by the object scale:
        return Vector3.Scale(max - min, obj.transform.localScale);
    }

    public static float CalcPointToRayDistance(GameObject obj, Vector3 point)
    {
        //Vector3 direction = obj.GetComponent<Rigidbody>().velocity.normalized;
        //Vector3 startingPoint = obj.transform.position;

        Ray ray = new Ray(obj.transform.position, obj.transform.up);
        float distance = Vector3.Cross(ray.direction, point - ray.origin).magnitude;

        return distance;
    }

    public static void TestColorChange(Renderer _renderer)
    {
        _renderer.material.color = Color.Lerp(FocusUtils.ObjNormalColor, Color.white, 0.9f);
        
    }


    public static List<List<float>> GenerateLayerCoords(float layerWidth, float objWidth, int num)
    {
        List<float> rand_x = new List<float>();
        List<float> rand_z = new List<float>();

        for (int k = 0; k < num; k++)
        {
            for (int i = 0; i < num; i++)
            {
                float interval = layerWidth / num;
                float val = Random.Range(i * interval, (i + 1) * interval - objWidth);
                rand_x.Add(val);
            }
        }

        for (int k = 0; k < num; k++)
        {
            for (int i = 0; i < num; i++)
            {
                float interval = layerWidth / num;
                float val = Random.Range(k * interval, (k + 1) * interval - objWidth);
                rand_z.Add(val);
            }
        }

        List<List<float>> res = new List<List<float>>();

        res.Add(rand_x);
        res.Add(rand_z);

        return res;
    }

    public static GameObject BaseObjectToSelect(SelectionDataManager sDM, float maxSnapDis)
    {
        for (int i = 0; i < sDM.SceneObjects.Count; i++)
        {
            Vector3 objPos = sDM.SceneObjects[i].transform.position;
            Vector3 handPos = GetIndexThumbPos(sDM);
            float objHandDis = 999999f;

            if (handPos != NullVector3) objHandDis = Vector3.Distance(objPos, handPos);

            if (objHandDis < maxSnapDis)
            {
                return sDM.SceneObjects[i];
            }
        }

        return null;
    }


    public static void ChangeMaterialColor(Renderer renderer, Color color)
    {
        if (isUnityDefaultMat(renderer.material.shader))
        {
            float currAlpha = renderer.material.color.a;
            renderer.material.color = new Color(color.r, color.g, color.b, currAlpha);
        }
        else
        {
            Color c = renderer.material.GetColor("_AlbedoColor");
            renderer.material.SetColor("_AlbedoColor", new Color(color.r, color.g, color.b, c.a));
        }
    }


    public static void RemoveCurrentRecordedSceneObjects(SelectionDataManager sDM)
    {
        int num = sDM.SceneObjects.Count;
        if (num != 0)
        {
            for (int i = 0; i < num; i++)
            {
                GameObject.Destroy(sDM.SceneObjects[i]);
            }
        }

        int num2 = sDM.UtilityObjects.Count;
        if (num2 != 0)
        {
            for (int i = 0; i < num2; i++)
            {
                GameObject.Destroy(sDM.UtilityObjects[i]);
            }
        }

        RemoveCurrentPlanes(sDM);

        sDM.SceneObjects.Clear();
        sDM.PeripheralObjects.Clear();
        sDM.FocusedObjects.Clear();
        sDM.UtilityObjects.Clear();
    }


    public static void RemoveCurrentPlanes(SelectionDataManager sDM)
    {
        int num = sDM.VerticalHelperPlanes.Count;

        if (num != 0)
        {
            for (int i = 0; i < num; i++)
            {
                GameObject.Destroy(sDM.VerticalHelperPlanes[i]);
            }
        }

        sDM.VerticalHelperPlanes.Clear();
    }


    //public static List<Vector3> GetMeshFourCorners(GameObject obj)
    //{
    //    List<Vector3> res = new List<Vector3>();

    //    Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
    //    if (mesh)
    //    {
    //        Vector3 min = mesh.bounds.min;
    //        Vector3 max = mesh.bounds.max;
    //        Transform transform = obj.transform;
    //        res.Add(transform.TransformPoint(new Vector3(min.x, min.y, min.z)));
    //        res.Add(transform.TransformPoint(new Vector3(min.x, min.y, max.z)));
    //        //Vector3 point3 = transform.TransformPoint(new Vector3(min.x, max.y, min.z));
    //        //Vector3 point4 = transform.TransformPoint(new Vector3(min.x, max.y, max.z));
    //        res.Add(transform.TransformPoint(new Vector3(max.x, min.y, min.z)));
    //        res.Add(transform.TransformPoint(new Vector3(max.x, min.y, max.z)));
    //        //Vector3 point7 = transform.TransformPoint(new Vector3(max.x, max.y, min.z));
    //        //Vector3 point8 = transform.TransformPoint(new Vector3(max.x, max.y, max.z));

    //    }

    //    return res;
    //}

    public static void SendSelectionData(Vector3 grabPos, GameObject grabbedObj, SelectionDataManager sDM, string sceneName)
    {
#if !UNITY_EDITOR
        // transmit data
        if (Jetfire.IsConnected2())
        {
            string message = "";
            message += sceneName;
            message += ", TrialID: " + sDM.CurrentTrialID;

            message += ", selected," + FocusUtils.WorldToScreenSpace(grabPos) + "," + FocusUtils.AddTimeStamp();


            Color objColor = grabbedObj.GetComponent<Renderer>().material.color;

            if (grabbedObj.GetComponent<Selectable>().IsTarget)
            {
                message += ", target obj";
            }
            else
            {
                message += ", normal obj";
            }

            Jetfire.SendMsg2(message);
        }
#endif
    }

    public static void SendGenericData(string message)
    {
#if !UNITY_EDITOR
        // transmit data
        if (Jetfire.IsConnected2())
        {
            Jetfire.SendMsg2(message);
        }
#endif
    }
}
