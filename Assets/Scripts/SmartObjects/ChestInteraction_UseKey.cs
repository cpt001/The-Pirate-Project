using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is part of the complex interaction chain.
/// The AI uses a key on the chest, trigging the lock toggle, and opening the chest.
/// </summary>
[RequireComponent(typeof(SmartObject_Chest))]
public class ChestInteraction_UseKey : SimpleInteraction
{
    [SerializeField] protected SmartObject_Chest linkedChest;
    //[SerializeField] protected GameObject key;

    protected void Awake()
    {
        linkedChest = GetComponent<SmartObject_Chest>();
    }

    /*public override bool Usable()
    {
        return base.Usable() && linkedChest.isOpen;
    }*/
}
