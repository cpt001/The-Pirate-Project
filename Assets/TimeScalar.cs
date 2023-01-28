using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeScalar : MonoBehaviour
{
    /// <summary>
    /// This class handles how time is calculated, and divides time into day, hour, and minute ticks
    /// </summary>
    /// 

    public int year = 0;
    public int dayToYear = 100;
    public int dayNumber = 0;

    public bool newDayTriggered;
    private float daylightSpeed = 1.25f;    //Controls how long daylight will last in a day.
    private float timeSpeed = 1.25f;        //Controls how fast time will progress.
    [SerializeField] private int hoursPassed = 0;
    private int nextHour = 15;
    private float fauxRotValue;

    private void Start()
    {
        StartCoroutine(NewDay());
    }

    void Update()       //Fix the rotation, then get the angles. div by 15. That gives 24h - 6 = sunrise
    {
        transform.Rotate(Vector3.left * (daylightSpeed * Time.deltaTime));
        fauxRotValue += timeSpeed * Time.deltaTime;
        if (fauxRotValue >= (nextHour - 0.2f) && fauxRotValue <= (nextHour + 0.2f))
        {
            NewHour();
        }
    }
    /// <summary>
    /// Hour, day, and year all need to be tied to listener events for other scripts to make better use of them. 
    /// Might also help clean this section up a bit as well
    /// </summary>
    void NewHour()
    {
        EventsManager.TriggerEvent("NewHour");
        hoursPassed++;
        if (nextHour < 360)
        {
            nextHour = nextHour + 15;
        }
        if (hoursPassed == 24)
        {
            hoursPassed = 0;
            nextHour = 15;
            fauxRotValue = 0;
            StartCoroutine(NewDay());
        }
    }

    IEnumerator NewDay()
    {
        EventsManager.TriggerEvent("NewDay");
        //Debug.Log("New day triggered!");
        dayNumber++;
        newDayTriggered = true;
        if (dayNumber > dayToYear)
        {
            NewYear();
        }
        yield return null;  //This triggers too fast for the other scripts to pick up on it, but it feels like the right track
        newDayTriggered = false;
        //yield return null;
    }

    void NewYear()
    {
        EventsManager.TriggerEvent("NewYear");
        year++;
        dayNumber = 0;
    }
}
