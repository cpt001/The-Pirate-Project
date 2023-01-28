using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Usage:
/// [TimeScalar]
/// EventsManager.TriggerEvent("XYZ") - creates a new event, and sends the signal outward
/// [Structure]
/// EventsManager.StartListening("XYZ", localUpdateName); - Listens for the named event when it's fired
/// EventsManager.StopListening("XYZ", localUpdateName); - Stops listening for named event
/// </summary>

public class EventsManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent> eventDictionary;
    private static EventsManager eventManager;

    //Ensures the eventmanager exists
    public static EventsManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(EventsManager)) as EventsManager;
                if (!eventManager)
                {
                    Debug.LogError("Missing EventManager in scene");
                }
                else
                {
                    eventManager.Init();
                }
            }
            return eventManager;
        }
    }

    //Creates dictionary with events
    private void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, UnityEvent>();
        }
    }

    public static void StartListening(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, UnityAction listener)
    {
        if (eventManager == null) return;
        UnityEvent thisevent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisevent))
        {
            thisevent.RemoveListener(listener);
        }
    }

    public static void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}
