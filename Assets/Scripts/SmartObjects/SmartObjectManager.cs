using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartObjectManager : MonoBehaviour
{
   public List<SmartObject> happinessSmartObjects = new List<SmartObject>();
    public List<SmartObject> funSmartObjects = new List<SmartObject>();
    public List<SmartObject> hungerSmartObjects = new List<SmartObject>();
    public List<SmartObject> exhaustionSmartObjects = new List<SmartObject>();
    public List<SmartObject> loveSmartObjects = new List<SmartObject>();
    public List<SmartObject> peaceSmartObjects = new List<SmartObject>();
    public List<SmartObject> drunkennessSmartObjects = new List<SmartObject>();
    public List<SmartObject> adventureSmartObjects = new List<SmartObject>();
    public List<SmartObject> cleanlinessSmartObjects = new List<SmartObject>();
    public List<SmartObject> healthSmartObjects = new List<SmartObject>();
    public List<SmartObject> socialSmartObjects = new List<SmartObject>();
    
    public enum NeedFulfillmentType
    {
        happiness,
        fun,
        hunger,
        exhaustion,
        love,
        peace,
        drunkenness,
        adventure,
        cleanliness,
        health,
        social,
    }

    public static SmartObjectManager Instance { get; private set; } = null;

    public List<SmartObject> RegisteredObjects { get; private set; } = new List<SmartObject>();

    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Trying to create second SmartObjectManager on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterSmartObject(SmartObject toRegister)
    {
        RegisteredObjects.Add(toRegister);
        Debug.Log(toRegister);
    }

    public void DeregisterSmartObject(SmartObject toDeregister)
    {
        RegisteredObjects.Remove(toDeregister);
    }
}
