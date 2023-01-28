using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandMove : ICommand
{
    private GameObject _target;
    private Vector3 _prevPos;
    private Quaternion _prevRot;
    private Vector3 _afterPos;
    private Quaternion _afterRot;

    public CommandMove(GameObject target, Vector3 prevPos, Quaternion prevRot, Vector3 afterPos, Quaternion afterRot)
    {
        _target = target;
        _prevPos = prevPos;
        _prevRot = prevRot;
        _afterPos = afterPos;
        _afterRot = afterRot;
    }

    public void Undo()
    {
        _target.transform.position = _prevPos;
        _target.transform.rotation = _prevRot;
    }

    public void Redo()
    {
        _target.transform.position = _afterPos;
        _target.transform.rotation = _afterRot;
    }
}
