using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is utilized on individual objects. 
/// -When a need is too high, the AI will search for the nearest of these objects that has the same type as their need to fulfill. 
/// -> Priority, look in inventory, then look in world
/// -The 
/// </summary>
public class SmartObject : MonoBehaviour
{
    public bool isInventoryItem;
    public bool isConsumedOnUse;
    public enum NeedFulfillmentType
    {

    }
    public int fulfillmentAmount;
    public int interactionTime;
}
