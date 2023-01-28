using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamDataManager : MonoBehaviour
{
    public Camera FirstPersonCamera;

    public GameObject CreateMenu;
    private FoamRadialMenuParent _createMenuParent;
    public GameObject ManiMenu;
    private FoamRadialMenuParent _maniMenuParent;
    public GameObject ObjMenu;

    public GameObject StateIndicator;

    public GameObject ActiveHand;
    public GameObject ActiveIndex;
    public GameObject ActivePalm;
    public GestureControl ActiveGC;
    private float _triggerRadius = 2.0f;  // size of bounding region
    private float _middleRadius = 0.030f; // middle region of the menu

    // creation menu renderer and sprite
    //public SpriteRenderer CubeRenderer;
    //public SpriteRenderer ConeRenderer;
    //public SpriteRenderer CylinderRenderer;
    //public SpriteRenderer SphereRenderer;
    //public SpriteRenderer CreationCenterRenderer;
    //private List<SpriteRenderer> _creationRenderers = new List<SpriteRenderer>();

    //public SpriteRenderer ManiUppRenderer;
    //public SpriteRenderer ManiRightRenderer;
    //public SpriteRenderer ManiLowRenderer;
    //public SpriteRenderer ManiLeftRenderer;
    //public SpriteRenderer ManiCenterRenderer;

    //private List<SpriteRenderer> _manipulateRenderers = new List<SpriteRenderer>();


    //private Color _normalColor = new Color(1f, 1f, 1f, 0.78f);
    //private Color _hoverColor = new Color(1f, 1f, 1f, 1f);
    public Material FoamMaterial;
    private Color _objManiOGColor;


    public Transform CubePrefab;
    public Transform SpherePrefab;
    public Transform CylinderPrefab;
    public Transform ConePrefab;
    public Transform ConeDummyPrefab;
    private CreateMenuItem _selected_createItem = CreateMenuItem.NULL;
    private ManiMenuItem _selected_maniItem = ManiMenuItem.NULL;


    private List<GameObject> _sceneObjs = new List<GameObject>();
    private List<Portalble.Functions.Grab.GrabCollider> _sceneObjGCs = new List<Portalble.Functions.Grab.GrabCollider>();
    private GameObject _currentSelectionObj = null;
    private Animator _stateMachine = null;

    private Vector3 _objCreatedPos;

    public FoamScaleParent FoamScaleTool;

    public GameObject UndoButton;
    public GameObject RedoButton;

    // Start is called before the first frame update
    void Start()
    {
        CreateMenu.SetActive(false);
        ManiMenu.SetActive(false);
        ObjMenu.SetActive(false);

        ActiveIndex = ActiveHand.transform.GetChild(1).GetChild(2).gameObject;
        ActivePalm = ActiveHand.transform.GetChild(5).GetChild(0).gameObject;
        ActiveGC = ActiveHand.GetComponent<GestureControl>();

        // creation renderers
        //_creationRenderers.Add(CubeRenderer);
        //_creationRenderers.Add(CylinderRenderer);
        //_creationRenderers.Add(ConeRenderer);
        //_creationRenderers.Add(SphereRenderer);
        _createMenuParent = CreateMenu.GetComponent<FoamRadialMenuParent>();

        // manipulationRenderers
        //_manipulateRenderers.Add(ManiUppRenderer);
        //_manipulateRenderers.Add(ManiRightRenderer);
        //_manipulateRenderers.Add(ManiLowRenderer);
        //_manipulateRenderers.Add(ManiLeftRenderer);
        _maniMenuParent = ManiMenu.GetComponent<FoamRadialMenuParent>();


        _objCreatedPos = Vector3.zero;

        _objManiOGColor = FoamMaterial.color;
    }

    // Update is called once per frame
    void Update()
    {
        // check state first
       _objCreatedPos = ActivePalm.transform.position + FoamUtils.ObjCreatedOffset * ActivePalm.transform.forward;
    }

    public float TriggerRadius
    {
        get { return _triggerRadius; }
    }

    public float MiddleRadius
    {
        get { return _middleRadius; }
    }


    public CreateMenuItem Selected_createItem
    {
        get { return _selected_createItem; }
        set { _selected_createItem = value; }
    }


    public ManiMenuItem Selected_maniItem
    {
        get { return _selected_maniItem; }
        set { _selected_maniItem = value; }
    }


    //public List<SpriteRenderer> CreationRenderers
    //{
    //    get { return _creationRenderers; }
    //}

    public FoamRadialMenuParent CreateMenuParent
    {
        get { return _createMenuParent; }
    }

    public FoamRadialMenuParent ManiMenuParent
    {
        get { return _maniMenuParent; }
    }


    //public List<SpriteRenderer> ManipulateRenderers
    //{
    //    get { return _manipulateRenderers; }
    //}

    public List<GameObject> SceneObjs
    {
        get { return _sceneObjs; }
    }

    public List<Portalble.Functions.Grab.GrabCollider> SceneObjGCs
    {
        get { return _sceneObjGCs; }
    }

    public GameObject CurrentSelectionObj
    {
        set { _currentSelectionObj = value; }
        get { return _currentSelectionObj; }
    }

    public Animator StateMachine
    {
        set { _stateMachine = value; }
        get { return _stateMachine; }
    }


    public Vector3 ObjCreatedPos
    {
        get { return _objCreatedPos; }
    }


    public Color ObjManiOGColor
    {
        get { return _objManiOGColor; }
    }
}
