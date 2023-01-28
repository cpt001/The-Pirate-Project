using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (TimeScalar))]
public class Climatizer : MonoBehaviour
{
    /// <summary>
    /// This system isn't a priority. 
    /// -On each hour, the system rolls for a random weather, weighted based on season.
    /// -The system will roll it to occupy a block of x hours.
    /// -When something rolls that isn't default, it starts a chain of weather that logically leads to the end goal.
    /// The weather can be overridden by nearby affectors, like volcanic and ghostly events.
    /// </summary>
    private enum OverarchingSeason
    {
        Spring,
        Summer,
        Fall,
        Winter,
    }
    private OverarchingSeason season;
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

    private bool weatherOverride = false;

    private void WeatherBasedOnSeason()
    {
        switch (season)
        {
            case OverarchingSeason.Spring:
                {

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
