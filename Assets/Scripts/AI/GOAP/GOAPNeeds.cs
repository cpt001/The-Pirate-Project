using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GOAPAI
{
    //This needs a call of some sort for the event system. Can probably call it from the timescalar script?
    /*public class AIStatConfiguration
    {
        [field: SerializeField] public AIStat linkedStat { get; private set; }
        [field: SerializeField] public bool OverrideDefaults { get; private set; } = false;
        [field: SerializeField, Range(0, 1f)] public float Override_InitialValue { get; protected set; } = 0.5f;
        [field: SerializeField, Range(0, 1f)] public float Override_DecayRate { get; protected set; } = 0.005f;

    }*/


    public class GOAPNeeds : MonoBehaviour
    {
        //[field: SerializeField] AIStatConfiguration[] Stats;



        private float Hunger;           //
        private float HungerDecay => Random.Range(12, 19);   //Should result in them wanting a meal once every 3-5 hours, and starving 
        private float Energy;           //
        private float EnergyDecay => Random.Range(6, 9);     //Complete exhaustion after 16-11 hours
        private float Recreation;       //
        private float RecreationDecay => Random.Range(10, 20);   //Total boredom occurs after about 10-5 hours
        private float Social;           //
        private float SocialDecay => Random.Range(6, 16); //This stat suddenly got interesting thanks to nessie! While decay does happen, the stat shouldn't really hit a negative too quickly, unless the bot is in extreme isolation, or in a small environment. Regardless, Desolate feelings occur around the 16-6 hours
        private float Comfort;          //
        private float ComfortDecay;     //Does not decay naturally
        private float Adventure;        //
        private float AdventureDecay => Random.Range(0, 0.8f);  //This should take about 7 days to hit its maximum value, but maintains as the only decay that can remain at 0
        private float Hygiene;          //
        private float HygieneDecay => Random.Range(1.4f, 4.3f);  //Should take about 33-18 hours
        private float Wealth;           //
        private float WealthDecay;  //Calculated weekly
        private float Medical;          //
        private float MedicalDecay; //Calculated based on medical state
        private float Sanity;           //
        private float SanityDecay;  //Additive based on a number of factors. Reduction?
        private float Crime;            //
        private float CrimeDecay;   //Additive based on a number of factors.
        private float Chemicals;        //
        private float ChemicalsDecay;   //Decays slowly when no addiction, faster when one is present

        public int overheadValue;
        public int priorityThreshold;

        private void Start()
        {
            EventsManager.StartListening("NewHour", DecayNeed);
            SetStartDecay();
        }

        //This is randomized for now, but needs to take variables into account in the future
        //Time of day, sex, 
        void SetStartDecay()
        {
            Hunger = Random.Range(40, 100);
            Energy = Random.Range(40, 100);
            Recreation = Random.Range(40, 100);
            Social = Random.Range(40, 100);
            Adventure = Random.Range(40, 100);
            Hygiene = Random.Range(40, 100);
        }

        //This is triggered once per hour, reducing needs as applicable
        void DecayNeed()
        {
            Hunger = Hunger - HungerDecay;
            Energy = Energy - EnergyDecay;
            Recreation = Recreation - RecreationDecay;
            Social = Social - SocialDecay;
            Adventure = Adventure - AdventureDecay;
            Hygiene = Hygiene - HygieneDecay;
            Chemicals = Chemicals - ChemicalsDecay;

            if (Hunger > 15)
            {
                
            }
        }
    }
}

