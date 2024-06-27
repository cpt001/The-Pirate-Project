using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObject_Hammock : SmartObject
{
    public bool isFolded;
    public string assignedUser;

    public void ToggleFoldState()
    {
        isFolded = !isFolded;
    }
}
