using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Crest;

public class TimeScalar : MonoBehaviour
{
    /// <summary>
    /// This class handles how time is calculated, and divides time into day, hour, and minute ticks
    /// -This class will also handle weather events and planning
    /// 
    /// Todo:
    /// Implement water color change
    /// -> Procedural skybox colors on ocean material: 
    /// Day     [Base: 141, 217, 255 | Away from Sun: 39, 56, 136] 
    /// Night   [Base: 17, 16, 16 | Away from Sun: 12, 12, 12]
    /// Implement seasons, and weather
    /// </summary>
    /// 

    public int year = 0;
    public int dayToYear = 100;
    public int dayNumber = 0;
    private int dayTracker = 0;
    [SerializeField] private OceanRenderer oceanObject;
    private Color32 oceanDay = new Color32(144, 217, 255, 1);
    private Color32 oceanAwayDay = new Color32(29, 56, 136, 1);
    private Color32 oceanNight = new Color32(16, 16, 16, 1);
    private Color32 oceanAwayNight = new Color32(12, 12, 12, 1);

    //public bool isDayOfRest;
    public bool isNightTime;
    private float timeSpeed = 1.25f;        //Controls how fast time will progress.
    [SerializeField] private int hoursPassed = 0;
    private int nextHour = 15;
    private float fauxRotValue;
    [SerializeField] private Light sun;
    [SerializeField] private TMPro.TextMeshProUGUI HUDClock;

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
        oceanObject.OceanMaterial.SetColor("_SkyBase", oceanNight);
        oceanObject.OceanMaterial.SetColor("_SkyAwayFromSun", oceanAwayNight);
        if (hoursPassed == 0)
        {
            isNightTime = true;
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.left * (timeSpeed * Time.deltaTime));
        fauxRotValue += timeSpeed * Time.deltaTime;
        HUDClock.text = "Time: " + hoursPassed.ToString();

        if (fauxRotValue >= (nextHour - 0.2f) && fauxRotValue <= (nextHour + 0.2f))
        {
            NewHour();
        }
    }
    void NewHour()
    {
        EventsManager.TriggerEvent("NewHour");
        hoursPassed++;
        LightOverHour();
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

    void LightOverHour()
    {
        switch (hoursPassed)
        {
            case 5:
                {
                    StartCoroutine(LerpSun(1.4f, oceanDay, oceanAwayDay, 35.0f));
                    isNightTime = false;
                    EventsManager.TriggerEvent("ToggleLights");
                    break;
                }
            case 19:
                {
                    StartCoroutine(LerpSun(0, oceanNight, oceanAwayNight, 35.0f));
                    isNightTime = true;
                    EventsManager.TriggerEvent("ToggleLights");
                    break; 
                }
        }
    }

    IEnumerator LerpSun(float targetIntensity, Color32 baseColor, Color32 awayColor, float timeToChange)
    {
        //Debug.Log("Sun lerp called");
        float time = 0;
        float startValue = sun.intensity;
        Color32 startBaseColor = oceanObject.OceanMaterial.GetColor("_SkyBase");
        Color32 startAwayColor = oceanObject.OceanMaterial.GetColor("_SkyAwayFromSun");
        while (time < 35)
        {
            sun.intensity = Mathf.Lerp(sun.intensity, targetIntensity, time / timeToChange);
            oceanObject.OceanMaterial.SetColor("_SkyBase", Color32.Lerp(startBaseColor, baseColor, time / timeToChange));  //This lerp function doesnt work sadly, but it's on the right track
            oceanObject.OceanMaterial.SetColor("_SkyAwayFromSun", Color32.Lerp(startAwayColor, awayColor, time / timeToChange));
            time += Time.deltaTime;
            yield return null;
        }
        sun.intensity = targetIntensity;
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
                    EventsManager.TriggerEvent("DayOfRest");
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
