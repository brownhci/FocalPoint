using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoamCopyIcon : FoamIconManager
{
    private int _hash_copySelectedBool = Animator.StringToHash("CopySelectedBool");

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void PerformAction()
    {
        base.PerformAction();
        m_data.StateMachine.SetBool(_hash_copySelectedBool, true);

    }
}
