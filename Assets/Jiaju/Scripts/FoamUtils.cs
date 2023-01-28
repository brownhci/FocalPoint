using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuRegion
{
    UPPER,
    RIGHT,
    LOWER,
    LEFT,
    MIDDLE
}

public enum CreateMenuItem
{
    CUBE,
    CYLINDER,
    CONE,
    SPHERE,
    NULL
}

public enum ManiMenuItem
{
    SELECT,
    MOVE,
    SCALE,
    ONE,
    NULL
}

public static class FoamUtils
{
    public static readonly Color ObjManiSelectedColor = new Color(247f/255f, 238f/255f, 144f/255f);
    public static readonly Color ObjManiOriginalColor = new Color(0.8431373f, 0.9882353f, 0.9764706f);

    public static readonly Color RadialIconBGNormalColor = new Color(0.1372549f, 0.1215686f, 0.1254902f, 0.80f); // #231F20
    public static readonly Color RadialIconBGHighlightColor = new Color(0.2470588f, 0.2470588f, 0.2470588f, 0.80f); // #3f3f3f
    public static readonly Color IconNormalColor = new Color(1f, 1f, 1f, 0.80f); // for radial centers. To rename

    public static readonly float ObjCreatedOffset = 0.08f;
    public static readonly int ObjCreatedAnimTime = 40;

    public static bool IsGlobalGrabbing = false;

    // to stop pinch bool from updating if finger in collider
    public static bool IsGlobalFingerInObject = false;

    //public static int CurrentSelectionObjID = 0;
    public static bool IsExcludingSelectedObj = false;

    public static float ScaleTabOffset = 0.06f;

    public static bool isInsideTri(Vector3 s, Vector3 a, Vector3 b, Vector3 c)
    {
        float as_x = s.x - a.x;
        float as_y = s.y - a.y;

        bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

        if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab) return false;
        if ((c.x - b.x) * (s.y - b.y) - (c.y - b.y) * (s.x - b.x) > 0 != s_ab) return false;

        return true;
    }

    //public static bool isInsideTri(Vector3 P, Vector3 A, Vector3 B, Vector3 C)
    //{
    //    bool b0 = Vector3.Dot(new Vector3(P.x - A.x, P.y - A.y), new Vector3(A.y - B.y, B.x - A.x)) > 0;
    //    bool b1 = Vector3.Dot(new Vector3(P.x - B.x, P.y - B.y), new Vector3(B.y - C.y, C.x - B.x)) > 0;
    //    bool b2 = Vector3.Dot(new Vector3(P.x - C.x, P.y - C.y), new Vector3(C.y - A.y, A.x - C.x)) > 0;
    //    return (b0 == b1 && b1 == b2);                                                            
    //}


    public static MenuRegion checkMenuRegion(Vector3 currPos, Vector3 initPos, Vector3 UppL, Vector3 UppR, Vector3 LowL, Vector3 LowR, float middleRadius)
	{
        if (Vector3.Distance(currPos, initPos) > middleRadius)
        {
            if (isInsideTri(currPos, UppL, UppR, initPos))
            {
                //Debug.Log("FOAMFILTER INSIDE UPPER TRI");
                return MenuRegion.UPPER;


                // right tri
            }
            else if (isInsideTri(currPos, UppR, LowR, initPos))
            {
                //Debug.Log("FOAMFILTER INSIDE RIGHT TRI");
                return MenuRegion.RIGHT;


                // lower tri
            }
            else if (isInsideTri(currPos, LowR, LowL, initPos))
            {
                //Debug.Log("FOAMFILTER INSIDE LOWER TRI");
                return MenuRegion.LOWER;


                // left tri
            }
            else
            {
                //Debug.Log("FOAMFILTER INSIDE LEFT TRI");
                return MenuRegion.LEFT;
            }
        }

        return MenuRegion.MIDDLE;
    }



    public static MenuRegion checkMenuRegionTri(Vector3 currPos, Vector3 initPos, Vector3 UppL, Vector3 UppR, Vector3 Low, float middleRadius)
    {
        if (Vector3.Distance(currPos, initPos) > middleRadius)
        {
            if (isInsideTri(currPos, UppL, UppR, initPos))
            {
                //Debug.Log("FOAMFILTER INSIDE UPPER TRI");
                return MenuRegion.UPPER;

            }
            else if (isInsideTri(currPos, UppR, Low, initPos))
            {
                //Debug.Log("FOAMFILTER INSIDE RIGHT TRI");
                return MenuRegion.RIGHT;

            }
            else
            {
                //Debug.Log("FOAMFILTER INSIDE LOWER TRI");
                return MenuRegion.LOWER;
            }
        }

        return MenuRegion.MIDDLE;
    }




    public static float LinearMap(float input, float ogMin, float ogMax, float tarMin, float tarMax)
    {
        float t = Mathf.Abs(input-ogMin) / Mathf.Abs(ogMax - ogMin);
        return Mathf.Lerp(tarMin, tarMax, t);
    }


    public static float LinearMapReverse(float input, float ogMin, float ogMax, float tarMin, float tarMax)
    {
        float t = Mathf.Abs(ogMax - input) / Mathf.Abs(ogMax - ogMin);
        return Mathf.Lerp(tarMin, tarMax, t);
    }


    public static float SinWave(int step)
    {
        float angle = (2f * Mathf.PI / 45f) * (float)step;
        return LinearMap(Mathf.Sin(angle), -1f, 1f, 0.5f, 1.0f);
    }


    public static int AnimateWaveTransparency(Renderer primRenderer, int transStep)
    {
        Color curC = primRenderer.material.color;
        float newA = SinWave(transStep);
        primRenderer.material.color = new Color(curC.r, curC.g, curC.b, newA);
        return transStep + 1;
    }

    public static int AnimateGrowSize(int animCount, Vector3 initalScale, Transform prim, Vector3 ObjCreatedPos)
    {
        float newX = FoamUtils.LinearMap(animCount, 0, FoamUtils.ObjCreatedAnimTime, 0.0f, initalScale.x);
        float newY = FoamUtils.LinearMap(animCount, 0, FoamUtils.ObjCreatedAnimTime, 0.0f, initalScale.y);
        float newZ = FoamUtils.LinearMap(animCount, 0, FoamUtils.ObjCreatedAnimTime, 0.0f, initalScale.z);

        prim.localScale = new Vector3(newX, newY, newZ);
        prim.position = ObjCreatedPos;
        return animCount + 1;
    }

    public static void CreateObjData(FoamDataManager DM, GameObject obj)
    {
        DM.SceneObjs.Add(obj);
        DM.SceneObjGCs.Add(obj.transform.GetChild(0).GetComponent<Portalble.Functions.Grab.GrabCollider>());
    }


    public static void RemoveObjData(FoamDataManager DM, GameObject obj)
    {
        DM.SceneObjs.Remove(obj);
        DM.SceneObjGCs.Remove(obj.transform.GetChild(0).GetComponent<Portalble.Functions.Grab.GrabCollider>());

    }


    public static bool ShouldStopGrabCollider(GameObject obj)
    {
        if (IsExcludingSelectedObj && obj.tag != "ScaleTab") // should chnage to only allow the six tabs to be grabbed
        {
            return true;
        }

        if (!FoamUtils.IsGlobalGrabbing) return true;

        return false;
    }
}
