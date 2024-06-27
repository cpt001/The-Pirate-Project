using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObject_Capstan : SmartObject
{
    public bool IsRaised { get; protected set; } = false;

    public void ToggleRaiseState()
    {
        IsRaised = !IsRaised;
    }
}
