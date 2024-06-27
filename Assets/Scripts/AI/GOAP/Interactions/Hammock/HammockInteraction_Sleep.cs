using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SmartObject_Hammock))]
public class HammockInteraction_Sleep : SimpleInteraction
{
    protected SmartObject_Hammock LinkedHammock;

    protected void Awake()
    {
        LinkedHammock = GetComponent<SmartObject_Hammock>();
    }

    public override bool CanPerform()
    {
        return base.CanPerform() && LinkedHammock.isFolded;
    }
}
