using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SmartObject_Chest))]
public class ChestInteraction_ToggleLock : SimpleInteraction
{
    protected SmartObject_Chest linkedChest;

    protected void Awake()
    {
        linkedChest = GetComponent<SmartObject_Chest>();
    }

    /*public override void Perform(PawnBaseClass performer, UnityAction<BaseInteractions> onCompleted)
    {
        linkedChest.toggleLocked();
        base.Perform(performer, onCompleted);
    }*/
}
