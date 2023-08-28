using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Smart objects are technically containers, as I understand it. They hold the interactions possible on an object.
/// This script is utilized on individual objects. 
/// -When a need is too high, the AI will search for the nearest of these objects that has the same type as their need to fulfill. 
/// -> Priority, look in inventory, then look in world
/// -The 
/// </summary>
public class SmartObject : MonoBehaviour
{
    [SerializeField] protected string _DisplayName;
    public List<BaseInteractions> CachedInteraction = null;
    [SerializeField] protected Transform _interactionMarker;
    public Vector3 interactionPoint => _interactionMarker != null ? _interactionMarker.position : transform.position;   //If its not null, use that position, otherwise use transform
    public string DisplayName => _DisplayName;
    public List<BaseInteractions> Interactions
    {
        get
        {
            if (CachedInteraction == null)
                CachedInteraction = new List<BaseInteractions>(GetComponents<BaseInteractions>());
                return CachedInteraction;
        }
    }

    void Start()
    {
        SmartObjectManager.Instance.RegisterSmartObject(this);
    }

    private void OnDestroy()
    {
        SmartObjectManager.Instance.DeregisterSmartObject(this);
    }
}
