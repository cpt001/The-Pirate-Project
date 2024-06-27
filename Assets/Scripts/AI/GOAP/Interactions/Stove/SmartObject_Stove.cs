using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObject_Stove : SmartObject
{
    public bool isOn { get; protected set; } = false;
    public bool hasSuitableIngredient;
    public bool hasFuel;
    public float burnTimeRemaining;

    //Check pawn inventory for a cookable item.
    //If not cookable item, look for nearest smart object containing food
    //If not available, ignore item until food is available
}
