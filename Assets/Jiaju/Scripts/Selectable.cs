using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portalble.Functions.Grab;

namespace Portalble
{
    [RequireComponent(typeof(Portalble.Functions.Grab.Grabable))]
    public class Selectable : MonoBehaviour
    {

        private bool _isTarget = false;
        private Renderer _renderer;
        private Material _outline_mat;

        //private Material[] _normal_mats;
        private Material[] _outline_mats;

        public float _outline_width = 0.003f; // for regularly sized

        private Vector3 _preSnapPos = new Vector3(0.0f, 0.0f, 0.0f);
        //private bool _isSnapped = false;

        public Material m_vertOutline; // to make more permanent
        private Color _normalColor;
        private Color _highlightColor;

        public bool IsSmallObj = false;
        public bool IsLegoCapped = false;
        private bool _isColliderReset = true;

        private Transform _grabCollider;
        private Vector3 _grabColliderOGScale;

        private bool _isDuringGrabbingProcess = false;

        // cool down
        private int _coolDownCounter = 0;
        private int _coolDownThreshold = 60;
        private bool _isCoolingDown = false;

        private SelectionDataManager _sDM;

        // Start is called before the first frame update
        void Start()
        {
            _renderer = GetComponent<Renderer>();
            UpdateMatColors(_renderer.material.color);
            //_renderer.material.color = FocusUtils.ObjNormalColor;

            // outline material
            _outline_mat = Instantiate(m_vertOutline);
            _outline_mat.SetFloat("_BodyAlpha", 0.0f);
            _outline_mat.SetFloat("_OutlineWidth", _outline_width);
            _outline_mat.SetColor("_OutlineColor", FocusUtils.ObjRankedColor);

            _outline_mats = new Material[2];
            _outline_mats[0] = _renderer.materials[0];
            _outline_mats[1] = _outline_mat;

            _grabCollider = this.transform.GetChild(0);
            _grabColliderOGScale = _grabCollider.localScale;
            //_grabColliderOGScale = new Vector3(1.0f, 1.0f, 1.0f);

            _sDM = GameObject.FindGameObjectWithTag("selectionDM").GetComponent<SelectionDataManager>();
        }


        // Update is called once per frame
        void Update()
        {
            if (GetComponent<Grabable>().IsBeingGrabbed())
            {
                RemoveHighestRankContour();
            }

            if (_isCoolingDown)
            {
                _coolDownCounter++;
                Debug.Log(_coolDownCounter);

                if (_coolDownCounter > _coolDownThreshold)
                {
                    _isCoolingDown = false;
                    _coolDownCounter = 0;
                }
            }
        }



        public void UpdateMatColors(Color color)
        {
            Renderer renderer = GetComponent<Renderer>();   

            _normalColor = color;
            _highlightColor = Color.Lerp(_normalColor, Color.white, 0.5f);

            FocusUtils.ChangeMaterialColor(renderer, color);
        }



        public void SetAsTarget()
        {
            _isTarget = true;
            UpdateMatColors(FocusUtils.ObjTargetColor);
        }



        public void Highlight(int fadeoutType)
        {
            FocusUtils.ChangeMaterialColor(_renderer, _highlightColor);
        }



        public void DeHighlight()
        {
            FocusUtils.ChangeMaterialColor(_renderer, _normalColor);
        }



        public bool IsTarget
        {
            get { return _isTarget; }
        }



        public void SetHighestRankContour()
        {
            _outline_mats[0] = _renderer.materials[0];

            if (_renderer.materials.Length <= 1) // if only one material, add
            {
                //Debug.Log("DOTT new added");
                _renderer.materials = _outline_mats;
                return;
            }

            // if two material and second is not dotted, skip
            if (_renderer.materials[1].HasProperty("_OutlineColor") && !_renderer.materials[1].HasProperty("_OutlineDot")) // if there is already an outline (finger enter or grabbing)
            {
                //Debug.Log("DOTT skipped");
                return;
            }

            // if two material and second is dotted, update
            //Debug.Log("DOTT last called");
            _outline_mats[1].SetFloat("_OutlineWidth", _outline_width);
            _renderer.materials = _outline_mats;
        }


        public bool SetSnapped(Vector3 snapToPos)
        {
            if (_isCoolingDown) return false;

            _preSnapPos = this.transform.position;

            this.transform.position = snapToPos;

            if (IsSmallObj)
            {
                Debug.Log("SNAPPINNNGGG");
                _grabCollider.localScale = new Vector3(0.05f / this.transform.localScale[0], 0.05f / this.transform.localScale[1], 0.05f / this.transform.localScale[2]);
                _isColliderReset = false;
                Debug.Log("SNAPPINNNGGG " + _grabCollider.localScale);
            }

            return true;
        }


        public void ConfirmSnapped()
        {
            Debug.Log("SNAPPINNNGGG Confirmed");
            //_isSnapped = true;
            _isDuringGrabbingProcess = true;
            _sDM.IsSnappedObejctReleased = false;

            GameObject scObj = GameObject.FindGameObjectWithTag("selectionController");
            if (scObj)
            {
                SelectionController sc = scObj.GetComponent<SelectionController>();
                sc.IsSnapped = true;
                sc.RecordGrabLoc(this.gameObject);

                // for modeling temp
                //if (_sDM.CurrentSelectedObj)
                //{
                //    _sDM.CurrentSelectedObj.GetComponent<Selectable>().UpdateMatColors(FoamUtils.ObjManiOriginalColor);
                //}
                //UpdateMatColors(FoamUtils.ObjManiSelectedColor);
                //_sDM.CurrentSelectedObj = this.gameObject;

            } else
            {
                GameObject bcObj = GameObject.FindGameObjectWithTag("baseController");
                if (bcObj)
                {
                    BaseController bc = bcObj.GetComponent<BaseController>();
                    bc.IsSnapped = true;
                    Debug.Log("bc RecordGrabLoc called");
                    bc.RecordGrabLoc(this.gameObject);
                }
            }
        }


        public void RemoveHighestRankContour()
        {
            if (_renderer.materials.Length > 1)
            {
                if (_renderer.materials[1].HasProperty("_OutlineColor") && !_renderer.materials[1].HasProperty("_OutlineDot")) // if there is already an outline (finger enter or grabbing)
                {
                    return;
                }

                Material mat = _renderer.materials[0];
                _renderer.materials = new Material[] { mat };
            }
        }

        public Vector3 GetSnappedPosition()
        {
            return _preSnapPos;
        }

        public void ResetColliderSizeToOG()
        {
            if (IsSmallObj)
            {
                //_grabCollider.localScale = _grabColliderOGScale;

                Vector3 scale = _grabCollider.localScale;
                scale.Set(_grabColliderOGScale[0], _grabColliderOGScale[1], _grabColliderOGScale[2]);
                _grabCollider.localScale = scale;


                _isDuringGrabbingProcess = false;
                _isColliderReset = true;
                _sDM.IsSnappedObejctReleased = true;

                Debug.Log("LETS DE EXPAND: " + _grabCollider.localScale + "  " + _grabCollider.transform.localScale);

                _isCoolingDown = true;
            }
        }
    }

}