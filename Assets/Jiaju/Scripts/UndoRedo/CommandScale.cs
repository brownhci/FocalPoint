using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandScale : ICommand
{
    private GameObject _target;
    private Vector3 _prevScale;
    private Vector3 _prevPos;
    private Vector3 _afterScale;
    private Vector3 _afterPos;

    public CommandScale(GameObject target, Vector3 prevScale, Vector3 prevPos, Vector3 afterScale, Vector3 afterPos)
    {
        _target = target;
        _prevScale = prevScale;
        _prevPos = prevPos;
        _afterScale = afterScale;
        _afterPos = afterPos;
    }

    public void Undo()
    {
        // need to redraw the tabs
        _target.transform.localScale = _prevScale;
        _target.transform.position = _prevPos;

    }

    public void Redo()
    {
        _target.transform.localScale = _afterScale;
        _target.transform.position = _afterPos;
    }
}
