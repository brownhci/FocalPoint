using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneMgr : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GoToSelection()
    {
        SceneManager.LoadScene("focus");
    }

    public void GoToModelling()
    {
        SceneManager.LoadScene("Jiaju");
    }

    public void GoToFocusScene()
    {
        SceneManager.LoadScene("focus_task_12");
    }

    public void GoToBaseScene()
    {
        SceneManager.LoadScene("base_task_12");
    }

}
