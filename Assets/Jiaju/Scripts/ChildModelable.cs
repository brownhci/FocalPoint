using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildModelable : MonoBehaviour
{
    //// Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}

    void OnTriggerEnter(Collider other)
    {
        this.transform.GetComponentInParent<Modelable>().OnChildTriggerEnter(other);
    }


    void OnTriggerStay(Collider other)
    {
        this.transform.GetComponentInParent<Modelable>().OnChildTriggerStay(other);
    }


    void OnTriggerExit(Collider other)
    {
        this.transform.GetComponentInParent<Modelable>().OnChildTriggerExit(other);
    }
}
