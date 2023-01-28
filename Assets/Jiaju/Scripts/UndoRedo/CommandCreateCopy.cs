using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandCreateCopy : ICommand
{

    private GameObject _target;
    private FoamDataManager _data;

    public CommandCreateCopy(GameObject target, FoamDataManager data)
    {
        _target = target;
        _data = data;
    }

    // *delete* the object
    public void Undo()
    {
        if (_data.CurrentSelectionObj && _target.GetInstanceID() == _data.CurrentSelectionObj.GetInstanceID())
        {
            _target.GetComponent<Modelable>().Deselect();
            _data.CurrentSelectionObj = null;
            //FoamUtils.CurrentSelectionObjID = 0;
        }

        _target.SetActive(false);
        FoamUtils.RemoveObjData(_data, _target);
    }

    public void Redo()
    {
        _target.GetComponent<Modelable>().Deselect();
        _target.SetActive(true);
        FoamUtils.CreateObjData(_data, _target);
    }
}
