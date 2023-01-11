using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Vector3 sunRotation;
    private float rotationSpeed = -1.25f;
    //Midnight = 180, midday = 0
    private int hoursPassed = 0;
    [SerializeField] private int nextHour = 15;
    [SerializeField] private int currentHour;
    private bool hoursRising = true;
    // Update is called once per frame
    void Update()       //Fix the rotation, then get the angles. div by 15. That gives 24h
    {
        transform.Rotate(Vector3.left * (rotationSpeed * Time.deltaTime));
        Debug.Log("X Rotation: " + transform.localRotation.eulerAngles.x + " Next hour: " + nextHour);

        if (transform.localRotation.eulerAngles.x >= (nextHour - 0.2f) && transform.localRotation.eulerAngles.x <= (nextHour + 0.2f))
        {
            if (hoursRising == true && nextHour != 105)
            {
                nextHour = nextHour + 15;   //105 is a miss? -- It's euler. It goes to 90, then back down. 
            }
            else if (nextHour == 105)
            {
                if (hoursRising == true)
                {
                    nextHour = 75;
                }
                hoursRising = false;
                nextHour = nextHour - 15;
            }
            NewHour();
        }
        //currentHour = Mathf.RoundToInt((nextHour + 15) / 15);

        if (currentHour == 12)
        {
            newDayTriggered = false;
        }
        else
        {
            //NewDay();
            if (dayNumber > dayToYear)
            {
                NewYear();
            }
        }
    }

    void NewHour()
    {
        hoursPassed++;
        Debug.Log("Hours passed: " + hoursPassed);
        if (hoursPassed == 24)
        {
            hoursPassed = 0;
            nextHour = 15;
            NewDay();
        }
    }

    void NewDay()
    {
        Debug.Log("New day triggered!");
        dayNumber++;
        newDayTriggered = true;
    }
    void NewYear()
    {
        year++;
        dayNumber = 0;
    }
}
