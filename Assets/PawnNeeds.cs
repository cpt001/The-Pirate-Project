using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Current tasks:
/// Create stacked ordering of priority needs.
/// Look for local smart objects that fulfill the need the best
/// Set destination, check work/sleep condition before
/// </summary>

public class PawnNeeds : MonoBehaviour
{
    ////Each of these stats goes from 0-100 based on fulfillment
    [Range(0, 100)]
    public float happiness, fun, hunger, exhaustion, love, peace, drunkenness, adventure, cleanliness, health;     //Overall stat
    //Restroom?

    private bool sleepObligation;   //Prevents the worker from going anywhere when asleep
    private bool workObligation;    //Prevents the worker from going anywhere when working

    public Transform prioritizedDestination;

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
        CalculateHappiness();
    }

    private void HourToHour()
    {
        CalculateHappiness();


        CalculateFunImpact(fun);
        CalculateHungerImpact(hunger);
        CalculateExhaustionImpact(exhaustion);
        CalculateLoveImpact(love);
        CalculatePeaceImpact(peace);
        CalculateDrunkennessImpact(drunkenness);
        CalculateAdventureImpact(adventure);
        CalculateCleanlinessImpact(cleanliness);
        CalculateHealthImpact(health);
    }




    void CalculateHappiness()
    {
        happiness = happiness + (CalculateFunImpact(fun) + CalculateHungerImpact(hunger) + CalculateHungerImpact(hunger) + CalculateExhaustionImpact(exhaustion) + CalculateLoveImpact(love) 
            + CalculatePeaceImpact(peace) + CalculateDrunkennessImpact(drunkenness) + CalculateAdventureImpact(adventure) + CalculateCleanlinessImpact(cleanliness) + CalculateHealthImpact(health));
        happiness = happiness / 9;
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
