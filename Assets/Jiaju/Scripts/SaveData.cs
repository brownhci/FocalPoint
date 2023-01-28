using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ListVector3Wrapper
{
    public List<Vector3> InnerList;
}

[System.Serializable]
public class SaveData
{
    public List<ListVector3Wrapper> m_pilesPos = new List<ListVector3Wrapper>();
    public List<int> m_targetIndices = new List<int>();

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string a_Json)
    {
        JsonUtility.FromJsonOverwrite(a_Json, this);
    }
}

public interface ISaveable
{
    void PopulateSaveData(SaveData a_SaveData);
    void LoadFromSaveData(SaveData a_SaveData);
}