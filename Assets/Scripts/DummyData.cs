using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DummyData : MonoBehaviour
{
    [SerializeField]
    private int beginReplayIdx = 500;

    private List<string> cacheMsg = new List<string>();
    string[] tlist;
    private bool cacheLoaded = false;
    private int idx = 100;
    private WSManager wsmanager;
    public bool DummyDataReplay = false;
    // Start is called before the first frame update
    void Start()
    {
        wsmanager = GameObject.Find("WebsocketManager").GetComponent<WSManager>();
        if (DummyDataReplay)
        {
            GetComponent<WSManager>().enableDummyDataReplay();
            loadCache();
            idx = beginReplayIdx;
        }
    }

    // Update is called once per frame
    void Update()
    {
 
        // fetch message to portalble
        if (cacheLoaded)
        {
            wsmanager.OnWebSocketUnityReceiveMessage(tlist[idx]);
            idx++;
        }

        if (idx > tlist.Length - 1)
            idx = beginReplayIdx;
    }

    public void saveCache()
    {
        string path = "Assets/StreamingAssets/TestingSketchandGrab1.txt";
        Debug.Log("saving......");
        //Write some text to the test.txt file

        StreamWriter writer = new StreamWriter(path, true);
        for (int i = 0; i < cacheMsg.Count; i++)
            writer.WriteLine(cacheMsg[i]);

        writer.Close();
    }

    public void loadCache()
    {
        //string path = "Assets/StreamingAssets/TestingSketchandGrab5.txt";
        string path = "Assets/StreamingAssets/TestingSketchandGrab1.txt";

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        //Debug.Log();
        string t = reader.ReadToEnd();
        tlist = t.Split('\n');

        //Debug.Log(tlist.Length);
        reader.Close();

        cacheLoaded = true;

    }

    public void AddEntry(string message) {
        cacheMsg.Add(message);
    }
}
