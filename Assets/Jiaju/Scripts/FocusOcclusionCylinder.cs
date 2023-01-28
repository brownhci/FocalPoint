using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Portalble.Functions.Grab;

public class FocusOcclusionCylinder : MonoBehaviour
{
    private SelectionDataManager _selectionDM = null;

    // Start is called before the first frame update
    void Start()
    {
        GameObject sDMObj = GameObject.FindGameObjectWithTag("selectionDM");
        if (sDMObj)
        {
            _selectionDM = sDMObj.GetComponent<SelectionDataManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!_selectionDM)
        {
            GameObject sDMObj = GameObject.FindGameObjectWithTag("selectionDM");
            if (sDMObj)
            {
                _selectionDM = sDMObj.GetComponent<SelectionDataManager>();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Grab.Instance.IsGrabbing) return;

        if (_selectionDM && !_selectionDM.UseSelectionAid) return;

        if (other.tag == "InteractableObj") // ignore ARPlane prefab
        {
            _selectionDM.OccludingObjects.Add(other.gameObject);
            if (_selectionDM.FocusedObjects.Contains(other.gameObject))
            {
                FocusUtils.UpdateMaterialAlpha(other.GetComponent<Renderer>(), FocusUtils.OccludingObjAlpha);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "InteractableObj") // ignore ARPlane prefab
        {
            _selectionDM.OccludingObjects.Remove(other.gameObject);

        }
    }
}
