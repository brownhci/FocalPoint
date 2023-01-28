using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocusUIController : MonoBehaviour
{

    public GameObject Task1InputField;
    public GameObject Task2HideCylinderButton;

    private bool _isTask1 = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleTask1InputFieldVisbility()
    {
        _isTask1 = !_isTask1;
        Task1InputField.SetActive(_isTask1);
    }

    public void ToggleTask2ButtonVisbility()
    {
        if (Task2HideCylinderButton)
        {
            Task2HideCylinderButton.SetActive(!_isTask1);
        }
    }
}
