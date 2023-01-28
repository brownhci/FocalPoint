using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionDataManager : MonoBehaviour
{
    public GameObject ActiveHand;
    public Camera FirstPersonCamera;
    public GameObject FocusInkPrefab;

    public int TaskNumber = 1;
    public Transform TaskOnePlacePrefab;
    private int _currentTrialID;
    public List<Transform> RobotParts;
    //public GameObject CurrentSelectedObj = null;

    private List<GameObject> _sceneObjects = new List<GameObject>();
    private List<GameObject> _focusedObjects = new List<GameObject>();
    private List<GameObject> _peripheralObjects = new List<GameObject>();
    private List<GameObject> _occludingObjects = new List<GameObject>();
    private List<GameObject> _verticalHelperPlanes = new List<GameObject>();
    private List<GameObject> _utilityObjects = new List<GameObject>();

    private GameObject _activeIndex;
    private GameObject _activeThumb;
    private GameObject _activePalm;
    private GestureControl _activeGC;
    private bool _useSelectionAid = true;

    private float _maxSnapDis = 0.006f;

    // snap switch
    private bool _isSnappedObejctReleased;


    // depth cue related;
    private float _farDis = 0.3f;       // to be adjusted based on user preference. far distance to be reached
    private float _nearDis = 0.1f;      // to be adjusted based on user preference. near distance to be reached
    // 0.4 and 0.3 for foam cam model

    // Start is called before the first frame update
    void Start()
    {
        _activeIndex = ActiveHand.transform.GetChild(1).GetChild(2).gameObject;
        _activeThumb = ActiveHand.transform.GetChild(0).GetChild(2).gameObject;
        _activePalm = ActiveHand.transform.GetChild(5).GetChild(0).gameObject;
        _activeGC = ActiveHand.GetComponent<GestureControl>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void updateActiveObjects()
    {
        if (!ActiveHand) // if no hand
        {
            _activeIndex = null;
            _activeThumb = null;
            _activePalm = null;
            _activeGC = null;
            return;
        }

        _activeIndex = ActiveHand.transform.GetChild(1).GetChild(2).gameObject;
        _activeThumb = ActiveHand.transform.GetChild(0).GetChild(2).gameObject;
        _activePalm = ActiveHand.transform.GetChild(5).GetChild(0).gameObject;
        _activeGC = ActiveHand.GetComponent<GestureControl>();
    }

    public List<GameObject> FocusedObjects
    {
        get { return _focusedObjects; }
    }

    public List<GameObject> PeripheralObjects
    {
        get { return _peripheralObjects; }
    }

    public GameObject ActiveIndex
    {
        get { return _activeIndex; }
    }

    public GameObject ActiveThumb
    {
        get { return _activeThumb; }
    }

    public GameObject ActivePalm
    {
        get { return _activePalm; }
    }

    public GestureControl ActiveGC
    {
        get { return _activeGC; }
    }

    public bool UseSelectionAid
    {
        get { return _useSelectionAid; }
        set { _useSelectionAid = value; }
    }

    public List<GameObject> SceneObjects
    {
        get { return _sceneObjects; }
    }

    //public HashSet<int> TargetObjIDs
    //{
    //    get { return _targetObjIDs; }
    //}

    public float NearDis
    {
        get { return _nearDis; }
    }

    public float FarDis
    {
        get { return _farDis; }
    }

    public bool IsSnappedObejctReleased
    {
        set { _isSnappedObejctReleased = value; }
        get { return _isSnappedObejctReleased; }
    }

    public List<GameObject> VerticalHelperPlanes
    {
        get { return _verticalHelperPlanes; }
    }

    public float MaxSnapDis
    {
        set { _maxSnapDis = value; }
        get { return _maxSnapDis; }
    }

    public List<GameObject> OccludingObjects
    {
        get { return _occludingObjects; }
    }

    public List<GameObject> UtilityObjects
    {
        get { return _utilityObjects; }
    }


    public void ToggleTaskNumber()
    {
        if (TaskNumber == 1) {
            TaskNumber = 2;
        }
        else if (TaskNumber == 2)
        {
            TaskNumber = 1;
        }

        FocusUtils.RemoveCurrentRecordedSceneObjects(this);
    }

    public int CurrentTrialID
    {
        set { _currentTrialID = value; }
        get { return _currentTrialID; }
    }
}
