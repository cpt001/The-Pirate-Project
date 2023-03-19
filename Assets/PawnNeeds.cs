using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnNeeds : MonoBehaviour
{
    ////Each of these stats goes from 0-100 based on fulfillment
    //[Range(0, 100)]
    ////public float happiness, fun, hunger, exhaustion, love, peace, drunkenness, adventure, cleanliness, health;     //Overall stat
    //public Dictionary<NeedValue, bool> needsMet = new Dictionary<NeedValue, bool>();
    ////public float fun;           //Resolved with tavern, fishing, dates, other off time activities
    ////public float hunger;        //Resolved at home, or by taking lunch at work
    ////public float exhaustion;    //Resolved with nap spot, or bed at home
    ////public float love;          //Resolved with target SO, or with parents
    ////public float peace;         //Resolved with lack of nearby combat
    ////public float drunkenness;   //Resolved at tavern
    ////public float adventure;     //Resolved by going to a new island, or by sailing
    ////public float cleanliness;   //Resolved by bathing
    ////public float health;        //Resolved with health potions

    private bool sleepObligation;   //Prevents the worker from going anywhere when asleep
    private bool workObligation;    //Prevents the worker from going anywhere when working

    struct NeedValue
    {
        public string name;
        public float currentValue;
        public float normalDecrement;
        public float unmetDecrement;
    }
    private Dictionary<NeedValue, float> needs = new Dictionary<NeedValue, float>();

    private void Awake()
    {
        EventsManager.StartListening("NewHour", HourToHour);

        #region Struct Definitions
        NeedValue happiness = new NeedValue();
        happiness.name = "Happiness";
        happiness.currentValue = Random.Range(0, 100);
        happiness.normalDecrement = 1;
        happiness.unmetDecrement = 2;
        needs.Add(happiness, happiness.currentValue);

        NeedValue fun = new NeedValue();
        fun.name = "Fun";
        fun.currentValue = Random.Range(0, 100);
        fun.normalDecrement = 1;
        fun.unmetDecrement = 2;
        needs.Add(fun, fun.currentValue);

        NeedValue hunger = new NeedValue();
        hunger.name = "Hunger";
        hunger.currentValue = Random.Range(0, 100);
        hunger.normalDecrement = 1;
        hunger.unmetDecrement = 2;
        needs.Add(hunger, hunger.currentValue);

        NeedValue exhaustion = new NeedValue();
        exhaustion.name = "Exhaustion";
        exhaustion.currentValue = Random.Range(0, 100);
        exhaustion.normalDecrement = 1;
        exhaustion.unmetDecrement = 2;
        needs.Add(exhaustion, exhaustion.currentValue);

        NeedValue love = new NeedValue();
        love.name = "Love";
        love.currentValue = Random.Range(0, 100);
        love.normalDecrement = 1;
        love.unmetDecrement = 2;
        needs.Add(love, love.currentValue);

        NeedValue peace = new NeedValue();
        peace.name = "Peace";
        peace.currentValue = Random.Range(0, 100);
        peace.normalDecrement = 1;
        peace.unmetDecrement = 2;
        needs.Add(peace, peace.currentValue);

        NeedValue drunkenness = new NeedValue();
        drunkenness.name = "Drunkenness";
        drunkenness.currentValue = Random.Range(0, 100);
        drunkenness.normalDecrement = 1;
        drunkenness.unmetDecrement = 2;
        needs.Add(drunkenness, drunkenness.currentValue);

        NeedValue adventure = new NeedValue();
        adventure.name = "Adventure";
        adventure.currentValue = Random.Range(0, 100);
        adventure.normalDecrement = 1;
        adventure.unmetDecrement = 2;
        needs.Add(adventure, adventure.currentValue);

        NeedValue cleanliness = new NeedValue();
        cleanliness.name = "Cleanliness";
        cleanliness.currentValue = Random.Range(0, 100);
        cleanliness.normalDecrement = 1;
        cleanliness.unmetDecrement = 2;
        needs.Add(cleanliness, cleanliness.currentValue);

        NeedValue health = new NeedValue();
        health.name = "Health";
        health.currentValue = Random.Range(0, 100);
        health.normalDecrement = 1;
        health.unmetDecrement = 2;
        needs.Add(health, health.currentValue);
        #endregion

        happiness.currentValue = fun.currentValue + hunger.currentValue + exhaustion.currentValue + love.currentValue + peace.currentValue 
                                + drunkenness.currentValue + adventure.currentValue + cleanliness.currentValue + health.currentValue;
        happiness.currentValue = happiness.currentValue / 9;

        /*needsMet.Add(happiness, true);
        needsMet.Add(hunger, true);
        needsMet.Add(exhaustion, true);
        needsMet.Add(love, true);
        needsMet.Add(peace, true);
        needsMet.Add(drunkenness, true);
        needsMet.Add(adventure, true);
        needsMet.Add(cleanliness, true);
        needsMet.Add(health, true);

        CalculateHappiness();*/
    }

    private void HourToHour()
    {
        
    }

    /*private void HourToHour()
    {
        foreach (KeyValuePair<float, bool> needStatus in needsMet)
        {
            if (needStatus.Key < 50.0f)
            {
                needsMet[needStatus.Key] = false;
            }
            else
            {
                needsMet[needStatus.Key] = true;
            }


            switch (needStatus.Value)
            {
                case needStatus.Value == false:
                    {
                        break;
                    }
            }
        }

        //Adjusts and sets the average of all combined stats
        //Some stats being higher means that other stats will decrease more slowly, and visa versa
        if (fun < 50)   //under
        {
            
        }
        else
        {
            //exhaustion++
            //hunger++
            //adventure++
        }
        if (hunger < 50)
        {

        }
        else
        {
            //fun
            //exhaustion
        }

        fun -= Random.Range(0, 3);
        hunger -= Random.Range(10, 15);
        exhaustion -= Random.Range(0, 3);
        love -= Random.Range(0, 3);
        peace -= Random.Range(0, 3);
        drunkenness -= Random.Range(0, 3);
        adventure -= Random.Range(0, 3);
        cleanliness -= Random.Range(0, 3);
        health -= Random.Range(0, 3);

        CalculateHappiness();
    }

    void CalculateHappiness()
    {
        happiness = (fun + hunger + exhaustion + love + peace + drunkenness + adventure + cleanliness + health);
        happiness = happiness / 9;
    }*/
}
