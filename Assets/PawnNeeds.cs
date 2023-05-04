using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Current tasks:
/// Create stacked ordering of priority needs.
/// Look for local smart objects that fulfill the need the best
/// Set destination, check work/sleep condition before
/// 
/// ToDo:
/// Script is still fully non-functional.
/// -Needs will decrement with time on a log scale
/// -Needs will affect mood
/// -Low needs assign priority, allowing pawn to check for smart objects
/// -Needs to communicate with new navigation script. 
/// </summary>

public class PawnNeeds : MonoBehaviour
{
    ////Each of these stats goes from 0-100 based on fulfillment
    [Range(0, 100)]
    public float happiness, fun, hunger, exhaustion, love, peace, drunkenness, adventure, cleanliness, health, social;     //Overall stat
    //Restroom?
    private Dictionary<string, float> stats = new Dictionary<string, float>();

    private bool sleepObligation;   //Prevents the worker from going anywhere when asleep
    private bool workObligation;    //Prevents the worker from going anywhere when working

    public Transform prioritizedDestination;

    private int hourCount;
    public PawnNeeds partner;

    private void Awake()
    {
        EventsManager.StartListening("NewHour", HourToHour);
        fun = Random.Range(0, 100);
        hunger = Random.Range(0, 100);
        exhaustion = Random.Range(0, 100);
        love = Random.Range(0, 100);
        peace = Random.Range(0, 100);
        drunkenness = Random.Range(0, 100);
        adventure = Random.Range(0, 100);
        cleanliness = Random.Range(0, 100);
        health = Random.Range(0, 100);

        stats.Add("Fun", fun);
        stats.Add("Hunger", hunger);
        stats.Add("Exhaustion", exhaustion);
        stats.Add("Love", love);
        stats.Add("Peace", peace);
        stats.Add("Drunkenness", drunkenness);
        stats.Add("Adventure", adventure);
        stats.Add("Cleanliness", cleanliness);
        stats.Add("Health", health);

        CalculateHappiness();
    }

    private void HourToHour()
    {
        if (hourCount < 24)
        {
            hourCount++;
        }
        else
        {
            hourCount = 0;
        }
        //CalculateHappiness();


        /*CalculateFunImpact(fun);
        /*CalculateHungerImpact(hunger);
        CalculateExhaustionImpact(exhaustion);
        CalculateLoveImpact(love);
        CalculatePeaceImpact(peace);
        CalculateDrunkennessImpact(drunkenness);
        CalculateAdventureImpact(adventure);
        CalculateCleanlinessImpact(cleanliness);
        CalculateHealthImpact(health);*/
    }

    /*void HourlyDecrement()
    {
        foreach (KeyValuePair<string, float> kvp in stats)
        {
            if (kvp.Key == "Fun" || kvp.Key == "Exhaustion"|| kvp.Key == "Love"|| kvp.Key == || kvp.Key == || kvp.Key == || kvp.Key == )
            {

            }
            else if(kvp.Key == "Hunger" || kvp.Key == "")
            {

            }
        }
    }*/


    void CalculateHappiness()
    {
        happiness = happiness + ((fun + hunger + exhaustion + love + peace + drunkenness + adventure + cleanliness + health) / 9);
        //happiness = happiness / 9;
    }

    //Calculates the effect that hunger has on happiness non-linearly
    float CalculateFunImpact(float fun_Need)
    {
        float funNeedAfterPlay = Mathf.Max(fun_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(funNeedAfterPlay) - CalculateHungerImpact(fun_Need);
        return -1 * Mathf.Pow(fun_Need / 10, 2);
    }
    float CalculateHungerImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateExhaustionImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateLoveImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculatePeaceImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateDrunkennessImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateAdventureImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateCleanlinessImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
    float CalculateHealthImpact(float eat_Need)
    {
        float eatNeedAfterMeal = Mathf.Max(eat_Need - 10, 0);
        float happinessIncrease = CalculateHungerImpact(eatNeedAfterMeal) - CalculateHungerImpact(eat_Need);
        return -1 * Mathf.Pow(eat_Need / 10, 2);
    }
}
