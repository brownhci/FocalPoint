using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandDelete : ICommand
{

    private GameObject _target;
    private FoamDataManager _data;

    public CommandDelete(GameObject target, FoamDataManager data)
    {
        _target = target;
        _data = data;
    }

    public void Undo()
    {
        _target.SetActive(true);
        FoamUtils.CreateObjData(_data, _target);
    }

    public void Redo()
    {
        _target.SetActive(false);
        FoamUtils.RemoveObjData(_data, _target);
    }
}
