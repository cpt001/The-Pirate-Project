using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObject_Chest : SmartObject
{
    public bool isOpen { get; protected set; } = false;
    
    public void toggleLocked()
    {
        isOpen = !isOpen;
        Debug.Log($"Chest is now {(isOpen ? "ON" : "OFF")}");
    }
}
