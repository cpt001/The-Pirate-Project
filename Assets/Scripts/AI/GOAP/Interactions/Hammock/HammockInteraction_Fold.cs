using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SmartObject_Hammock))]
public class HammockInteraction_Fold : SimpleInteraction
{
    protected SmartObject_Hammock LinkedHammock;

    protected void Awake()
    {
        LinkedHammock = GetComponent<SmartObject_Hammock>();
    }

    public override bool Perform(CommonAIBase performer, UnityAction<BaseInteraction> onCompleted)
    {
        LinkedHammock.ToggleFoldState();
        return base.Perform(performer, onCompleted);
    }
}
