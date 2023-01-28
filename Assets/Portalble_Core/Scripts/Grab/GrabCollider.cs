﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portalble.Functions.Grab {
    /// <summary>
    /// For grabbing collider
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class GrabCollider : MonoBehaviour {
        /// <summary>
        /// Collider Scalor, when fingers are in collider, the collider should expand;
        /// </summary>
        public float m_expandScalor = 1.2f;

        public float ExpandScalor {
            set {
                DeExpand();
                if (value >= 0.5f)
                    m_expandScalor = value;
                else
                    m_expandScalor = 1.2f;
            }
            get {
                return m_expandScalor;
            }
        }
        /// <summary>
        /// Whether or not use automatic expand scalor.
        /// </summary>
        public bool m_automaticExpand = true;
        /// <summary>
        /// Corresponding grabable object.
        /// </summary>
        public Grabable m_grabObj;
        /// <summary>
        /// The number of left fingers (actually it's finger bones) are in collider
        /// </summary>
        private int m_leftHandFingerIn = 0;
        /// <summary>
        /// The number of right fingers (actually it's finger bones) are in collider
        /// </summary>
        private int m_rightHandFingerIn = 0;
        /// <summary>
        /// The threshold of the number of fingers.
        /// </summary>
        private const int FINGER_THRESHOLD = 3;
        /// <summary>
        /// Flag, if it's ready for grabbing, it's true.
        /// </summary>
        private bool m_entered = false;
        /// <summary>
        /// Flag, if it's expanded, it's true.
        /// </summary>
        private bool m_expanded = false;
        /// <summary>
        /// Whether it's able to expand itself.
        /// </summary>
        private bool m_ableToExpand = true;

        // Jiaju change
        [HideInInspector]
        public bool IsFingerInObj;
        private GameObject _parentGameObject;

        // Use this for initialization
        void Start() {
            // Set kinematic in order to avoid physics
            Rigidbody rb = GetComponent<Rigidbody>();
            if (!rb.isKinematic)
                rb.isKinematic = true;

            // If the collider is mesh collider, make sure to use convex one.
            Collider collider = GetComponent<Collider>();
            if (collider.isTrigger == false) {
                collider.isTrigger = true;
            }
            if (collider is MeshCollider) {
                MeshCollider mcd = (MeshCollider)collider;
                if (mcd.convex == false) {
                    mcd.convex = true;
                }
            }

            if (m_grabObj == null && transform.parent != null) {
                m_grabObj = transform.parent.GetComponent<Grabable>();
            }

            _parentGameObject = this.transform.parent.gameObject;
        }

        void Update()
        {
            if (m_leftHandFingerIn > 0 || m_rightHandFingerIn > 0)
            {
                IsFingerInObj = true;

            }
            else
            {
                IsFingerInObj = false;

            }
        }


        void OnTriggerEnter(Collider other) {

            // Don't want palm
            if (other.name == "palm")
                return;
            if (other.transform.parent == null || other.transform.parent.parent == null)
                return;

            if (other.transform.parent.parent.name == "Hand_l") {
                if (other.transform.parent.name != "ring" && other.transform.parent.name != "pinky")
                {
                    m_leftHandFingerIn++;
                }

            }
            else if (other.transform.parent.parent.name == "Hand_r") {
                if (other.transform.parent.name != "ring" && other.transform.parent.name != "pinky")
                {
                    m_rightHandFingerIn++;
                }

            }

            // meaningless if it's already entered.
            if (m_entered)
                return;

            // if it's locked, don't change anything
            if (m_ableToExpand == false)
                return;

            // Jiaju FoamAR addition
            //if (!FoamUtils.IsGlobalGrabbing) return;
            if (FoamUtils.ShouldStopGrabCollider(this.transform.parent.gameObject)) return;


            if (m_leftHandFingerIn >= FINGER_THRESHOLD) {

                // Jiaju
                Modelable model = _parentGameObject.GetComponent<Modelable>();
                if (model)
                {
                    model.SetAsSelected();
                }
            }
            else if (m_rightHandFingerIn >= FINGER_THRESHOLD) {

                // Jiaju
                Modelable model = _parentGameObject.GetComponent<Modelable>();
                if (model)
                {
                    model.SetAsSelected();  
                }

                // Tell it to be grabbed
                if (m_grabObj != null) {
                    m_grabObj.OnGrabTriggerEnter(this, false);
                    m_entered = true;
                    Expand(other.transform.parent.parent);
                }
            }
        }

        void OnTriggerExit(Collider other) {

            if (other.name == "palm")
                return;

            if (other.transform.parent == null || other.transform.parent.parent == null)
                return;

            if (other.transform.parent.parent.name == "Hand_l") {
                if (other.transform.parent.name != "ring" && other.transform.parent.name != "pinky")
                {
                    m_leftHandFingerIn--;
                }
            }
            else if (other.transform.parent.parent.name == "Hand_r") {
                if (other.transform.parent.name != "ring" && other.transform.parent.name != "pinky")
                {
                    m_rightHandFingerIn--;
                }
            }

            // Only exit when already entered
            if (!m_entered)
                return;

            // Jiaju FoamAR addition
            //if (!FoamUtils.IsGlobalGrabbing) return;
            if (FoamUtils.ShouldStopGrabCollider(this.transform.parent.gameObject)) return;


            if (m_leftHandFingerIn < FINGER_THRESHOLD && m_rightHandFingerIn < FINGER_THRESHOLD) {
                DeExpand();
                if (m_grabObj != null) {
                    m_grabObj.OnGrabTriggerExit();
                }
                m_entered = false;
            }
        }

        void Expand(Transform hand) {
            if (m_expanded)
                return;

            if (!m_automaticExpand) {
                transform.localScale = transform.localScale * m_expandScalor;
                m_expanded = true;
            }
            else {
                if (hand != null) {
                    Transform palm = hand.Find("palm");
                    if (palm != null) {
                        Vector3 rayDir = transform.position - palm.position;
                        float finalDistance = rayDir.magnitude;
                        // Collider cd = GetComponent<Collider>();

                        Ray r = new Ray(palm.position, rayDir);
                        RaycastHit[] rh = Physics.RaycastAll(r, finalDistance + 1.0f);
                        m_expandScalor = 1.2f;
                        foreach (RaycastHit h in rh) {
                            if (h.collider.gameObject == gameObject) {
                                m_expandScalor = finalDistance / (finalDistance - h.distance);
                            }
                        }
                    }
                    transform.localScale = transform.localScale * m_expandScalor;
                    m_expanded = true;
                }
            }
        }

        void DeExpand() {
            if (m_expanded) {
                transform.localScale = transform.localScale / m_expandScalor;
                m_expanded = false;
            }
        }

        public void SetLock(bool lock_flag) {
            m_ableToExpand = !lock_flag;

            if (m_expanded && lock_flag) {
                DeExpand();
            }
        }

        // Jiaju FoamAR change
        public void DeGrab()
        {
            DeExpand();
            if (m_grabObj != null)
            {
                m_grabObj.OnGrabTriggerExit();
            }
            m_entered = false;
        }
    }
}