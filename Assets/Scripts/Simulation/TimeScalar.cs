using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimeScalar : MonoBehaviour
{
    /// <summary>
    /// This class handles how time is calculated, and divides time into day, hour, and minute ticks
    /// -This class will also handle weather events and planning
    /// 
    /// Todo:
    /// Implement seasons, and weather
    /// </summary>
    /// 

    public int year = 0;
    public int dayToYear = 100;
    public int dayNumber = 0;
    private int dayTracker = 0;

    //public bool isDayOfRest;
    private float daylightSpeed = 1.25f;    //Controls how long daylight will last in a day.
    private float timeSpeed = 1.25f;        //Controls how fast time will progress.
    [SerializeField] private int hoursPassed = 0;
    private int nextHour = 15;
    private float fauxRotValue;

    private enum OverarchingSeason
    {
        Spring,
        Summer,
        Fall,
        Winter,
    }
    private OverarchingSeason currentSeason;   //Div by 5
    private enum DayName
    {
        Sun,
        Moon,
        Stone,
        Sky,
        Rest,
    }
    private DayName dayName;

    private enum SeaWeather
    {
        Doldrums,
        Calm_Sea,
        Default,
        Rough_Sea,
        Haze,
        Fog,
        Ghostly_Fog,
        S_Ash,
        S_Boiling_Sea,

    }
    private SeaWeather weatherPlan;
    private enum SkyWeather
    {
        Clear_Sky,
        Idyllic_Clouds,
        Heavy_Clouds,
        Thunderheads,
        Sprinkle,
        Drizzle,
        Rain,
        Thunderstorm,
        Typhoon,
        S_Volcanic_Thunderstorm,
        S_VolcanicRain,
    }
    private SkyWeather skyPlan;

    private void Start()
    {
        StartCoroutine(NewDay());
        EventsManager.TriggerEvent("NewHour");
    }

    void Update()
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
        if (dayNumber > dayToYear)
        {
            NewYear();
        }
        SetSeason();
        SetDayOfWeek();
        yield return null;
    }

    void NewYear()
    {
        EventsManager.TriggerEvent("NewYear");
        year++;
        dayNumber = 0;
    }

    void SetDayOfWeek()
    {
        switch (dayTracker)
        {
            case 0:
                {
                    dayName = DayName.Sun;
                    break;
                }
            case 1:
                {
                    dayName = DayName.Moon;
                    break;
                }
            case 2:
                {
                    dayName = DayName.Stone;
                    break;
                }            
            case 3:
                {
                    dayName = DayName.Sky;
                    break;
                }            
            case 4:
                {
                    EventsManager.TriggerEvent("DayOfRest");
                    dayName = DayName.Rest;
                    break;
                }            
            case 5:
                {
                    dayTracker = 0;
                    break;
                }

        }
    }

    void SetSeason()
    {
        switch (dayNumber)  //Every 25 days a new season triggers
        {
            case 11:
                {
                    currentSeason = OverarchingSeason.Spring;
                    break;
                }
            case 36:
                {
                    currentSeason = OverarchingSeason.Summer;
                    break;
                }
            case 61:
                {
                    currentSeason = OverarchingSeason.Fall;
                    break;
                }
            case 86:
                {
                    currentSeason = OverarchingSeason.Winter;
                    break;
                }
        }

        switch (currentSeason)
        {
            case OverarchingSeason.Spring:
                {
                    //Change light color, scene fog, sun rotation speed, weather probabilities
                    break;
                }
            case OverarchingSeason.Summer:
                {
                    break;
                }
            case OverarchingSeason.Fall:
                {
                    break;
                }
            case OverarchingSeason.Winter:
                {
                    break;
                }
        }
    }
}
