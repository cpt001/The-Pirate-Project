using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script monitors individual pawn's health stats. It determines life span, daily health based on natural immunity, sickness, and bodily injuries.
/// 
/// ToDo:
/// -Finish female health, specifically pregnancy day proximity check
/// -Create birth defects
/// -Finish injury cascade section
/// 
/// -Create physical detection for body parts.
/// -> Send messages when impacted by spell or weapon
/// </summary>

public class PawnHealth : MonoBehaviour
{
    private PawnVisualGeneration pawnVisual;
    //private PawnNeeds pawnNeeds;

    [SerializeField] private int age;   //Age of the character
    private int maxAge;
    private int daysRemaining;
    private int birthday;    //0 - 100, what day is their birthday?
    private int immuneSystem;  //How frequently they get sick -- 0 - 5 (Sickly, Susceptible, Normal, RarelySick, IronGuard)
    public enum Sickness { Healthy, Sick, Medicated, Bedridden }     //Is the character sick? Healthy = 100%, Sick = 50%, Medicated = 75% of usual stats 
    public Sickness isSick;
    private bool permanentSickness;
    private int sicknessLifetime;   //How long the sickness will last in hours

    private enum InjuryStatus
    {
        Healthy,            //The body part operates as expected
        Bruised,            //Minor injury, operates at 85% capacity. Not lethal.
        Cut,                //Minor injury, operates at 75% capacity. Not lethal, unless on core body part.
        Eviscerated,        //Major injury, operates at 50% capacity. Lethal if not treated quickly.
        Treated,            //A treated injury, prevents bleed out.
        Missing_Bleeding,   //Critical injury, operates at 0% capacity, and will remove associated limbs. Lethal if not treated quickly, lethal if inflicted on core.
        Missing_Treated,    //Missing, but treated.
        Missing_Healed,     //Non-injury.
        Prosthetic,         //Non-injury, restores a portion of mobility in limb.
        Broken,             //Major injury, operates at 25% capacity.
        Frostbite,
    }
    private InjuryStatus injuryStatus;
    private Dictionary<string, InjuryStatus> bodyDictionary = new Dictionary<string, InjuryStatus>();

    private int internalHourCount;
    private int internalDayCount;

    [Header("Female Health")]
    #region
    public float fertility;/* = Random.Range(0.0f, 70.0f);*/ //Female only: Chance to get pregnant
    private float maxFertility;
    private bool recentTryForChild;
    private int maxFertilityDay;
    private int daysInPregnancy;
    private int daysInPregnancyRemaining;
    public int daysInCycle;   //Female only: Current day on period.
    public int numDaysCycleReset; //Number of days in cycle (21 - 35)
    public int timesPregnant;  //Value should be very low chance >1% for 1 or 2.
    public bool currentlyPregnant;  //Starting value is VERY low >3%
    public int numDaysRemainPregnant;   //Female only: Determines the number of days they will remain pregnant
    [SerializeField] private GameObject pawnPrefab;
    public enum PregnancyState
    {
        Not_Pregnant,
        First_Trimester,    //Morning sickness, tiredness, cravings, mood swings, small bladder (w1-12)
        Second_Trimester,   //Aches, 'mask of pregnancy', stretch marks (w13-28)
        Third_Trimester,    //Lower fitness, leaky breasts, poor sleep, contractions
        Nursing,            //Custom state, simple cooldown period of about 2 weeks before normal activities resume
        Menopause,
    }
    public PregnancyState pregnancyState;   //How long does pregnancy last? 69-74 days of course. For the meme
    #endregion

    /*protected override*/ void Start()
    {
        //base.Start();
        pawnVisual = GetComponent<PawnVisualGeneration>();
        InitPawnStats();
        EventsManager.StartListening("NewHour", HourlyPawnStats);
        EventsManager.StartListening("NewDay", DailyPawnStats);
        EventsManager.StartListening("NewYear", YearSetup);
    }

    void InitPawnStats()
    {
        SetupDefaultBodyDictionary();
        age = Random.Range(18, 41);
        maxAge = Random.Range(60, 105);
        birthday = Random.Range(0, 100);
        isSick = (Sickness)Random.Range(0, 4);
        if (pawnVisual.pawnGender == PawnVisualGeneration.PawnSex.Female)
        {
            SetFemaleHealth();
        }
    }
    void SetupDefaultBodyDictionary()
    {
        //Left limbs
        bodyDictionary.Add("LeftFoot", InjuryStatus.Healthy);   //No Cascade
        bodyDictionary.Add("LeftLeg", InjuryStatus.Healthy);
        bodyDictionary.Add("LeftThigh", InjuryStatus.Healthy);
        bodyDictionary.Add("LeftHand", InjuryStatus.Healthy);   //No Cascade
        bodyDictionary.Add("LeftArm", InjuryStatus.Healthy);
        bodyDictionary.Add("LeftBicep", InjuryStatus.Healthy);
        bodyDictionary.Add("LeftShoulder", InjuryStatus.Healthy);

        bodyDictionary.Add("LeftEye", InjuryStatus.Healthy);    //No Cascade

        //Right limbs
        bodyDictionary.Add("RightFoot", InjuryStatus.Healthy);  //No Cascade
        bodyDictionary.Add("RightLeg", InjuryStatus.Healthy);
        bodyDictionary.Add("RightThigh", InjuryStatus.Healthy);
        bodyDictionary.Add("RightHand", InjuryStatus.Healthy);  //No Cascade
        bodyDictionary.Add("RightArm", InjuryStatus.Healthy);
        bodyDictionary.Add("RightBicep", InjuryStatus.Healthy);
        bodyDictionary.Add("RightShoulder", InjuryStatus.Healthy);

        bodyDictionary.Add("RightEye", InjuryStatus.Healthy);   //No Cascade

        //Core
        bodyDictionary.Add("Genitals", InjuryStatus.Healthy);   //No Cascade
        bodyDictionary.Add("Hips", InjuryStatus.Healthy);       
        bodyDictionary.Add("Stomach", InjuryStatus.Healthy);    
        bodyDictionary.Add("Chest", InjuryStatus.Healthy);      //ToDo    
        bodyDictionary.Add("Heart", InjuryStatus.Healthy);      
        bodyDictionary.Add("Neck", InjuryStatus.Healthy);       
        bodyDictionary.Add("Jaw", InjuryStatus.Healthy);        //No Cascade   
        bodyDictionary.Add("Skull", InjuryStatus.Healthy);      
        bodyDictionary.Add("Brain", InjuryStatus.Healthy);      
    }
    void HourlyPawnStats()
    {
        internalHourCount++;
        if (sicknessLifetime >= 0)
        {
            sicknessLifetime--;
        }
    }
    void DailyPawnStats()
    {
        internalHourCount = 0;
        if (internalDayCount == birthday)
        {
            age++;
        }
        if (age > maxAge)
        {
            if (daysRemaining != 0)
            {
                daysRemaining = Mathf.RoundToInt(Random.Range(3, 300));
            }
            daysRemaining--;
            if (daysRemaining <= -1)
            {
                //Debug.Log(pawnNeeds.name + " be ded");
                Destroy(gameObject);
            }
        }
        DetermineSickStatus();
        if (pawnVisual.pawnGender == PawnVisualGeneration.PawnSex.Female)
        {
            DetermineFemaleHealth();
        }
    }
    void YearSetup() { internalDayCount = 0; }
    //Functionally, this rolls a small chance for sickness. If the pawn becomes sick, this function becomes a countdown timer
    void DetermineSickStatus()
    {
        //Set an hour for the pawn to become sick

        float chanceToBeSick = Random.Range(0, 100);

        if (chanceToBeSick >= immuneSystem)
        {
            isSick = (Sickness)Random.Range(0, 4);
        }


        #region Natural sickness chances
        if (age > 0 || age <= 3)
        {
            //Increase chance
            immuneSystem = 6;
        }
        else if (age > 3 || age <= 13)
        {
            //Decrease a bit
            immuneSystem = 8;
        }
        else if (age > 13 || age <= 45)
        {
            //Decrease to minimum
            immuneSystem = 4;
        }
        else if (age > 46)
        {
            //Big increase
            immuneSystem = 10;
        }
        #endregion


        switch (isSick)
        {
            case Sickness.Healthy:
                {
                    sicknessLifetime = 0;
                    break;
                }
            case Sickness.Sick:
                {
                    if (!permanentSickness)
                    {
                        sicknessLifetime = Random.Range(4, 20);
                    }
                    else
                    {
                        sicknessLifetime = 2400000;
                    }
                    if (sicknessLifetime == 0)
                    {
                        isSick = Sickness.Healthy;
                    }
                    break;
                }
            case Sickness.Medicated:
                {
                    sicknessLifetime = Random.Range(4, 8);
                    if (sicknessLifetime == 0)
                    {
                        isSick = Sickness.Healthy;
                    }
                    break;
                }
            case Sickness.Bedridden:
                {
                    if (!permanentSickness)
                    {
                        sicknessLifetime = Random.Range(24, 240);
                    }
                    else
                    {
                        sicknessLifetime = 2400000;
                    }
                    if (sicknessLifetime == 0)
                    {
                        isSick = Sickness.Sick;
                    }
                    break;
                }
        }
    }
    void DetermineBirthDefects()    //This case generates permanent injuries to a pawn, be it from birth, or recent injuries
    {
        foreach (KeyValuePair<string, InjuryStatus> bodykvp in bodyDictionary)
        {
            if (bodykvp.Value != InjuryStatus.Healthy)
            {
                //Do nothing
            }
            else
            {
                int randomChanceOfDisfigurement = Random.Range(1, 1000);
                if (randomChanceOfDisfigurement >= 999)
                {
                    //bodykvp.Value = InjuryStatus.Missing_Healed;
                }
            }
        }
    }
    //Cascade injuries are only triggered if a particular body part is severed. This ensures there aren't cases where the pawn may still have a hand, but no arm with which to use it.
    //Cascades are processed by combat messages sent by weapon/spell impacts to body parts.
    void ProcessInjuryCascades()
    {
        foreach (KeyValuePair<string, InjuryStatus> bodyKVP in bodyDictionary)
        {
            #region Left
            if (bodyKVP.Key == "LeftLeg" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["LeftFoot"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "LeftThigh" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["LeftLeg"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftFoot"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "LeftArm" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["LeftHand"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "LeftBicep" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["LeftArm"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftHand"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "LeftShoulder" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["LeftBicep"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftArm"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftHand"] = InjuryStatus.Missing_Healed;
            }
            #endregion
            #region Right
            if (bodyKVP.Key == "RightLeg" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["RightFoot"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "RightThigh" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["RightLeg"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightFoot"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "RightArm" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["RightHand"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "RightBicep" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["RightArm"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightHand"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "RightShoulder" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["RightBicep"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightArm"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightHand"] = InjuryStatus.Missing_Healed;
            }
            #endregion
            #region Core
            if (bodyKVP.Key == "Hips" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["RightFoot"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightLeg"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightThigh"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftFoot"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftLeg"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftThigh"] = InjuryStatus.Missing_Healed;
                bodyDictionary["Genitals"] = InjuryStatus.Missing_Healed;
            }
            if (bodyKVP.Key == "Stomach" && (bodyKVP.Value == InjuryStatus.Missing_Bleeding || bodyKVP.Value == InjuryStatus.Missing_Healed))
            {
                bodyDictionary["RightFoot"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightLeg"] = InjuryStatus.Missing_Healed;
                bodyDictionary["RightThigh"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftFoot"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftLeg"] = InjuryStatus.Missing_Healed;
                bodyDictionary["LeftThigh"] = InjuryStatus.Missing_Healed;
                bodyDictionary["Genitals"] = InjuryStatus.Missing_Healed;
                bodyDictionary["Hips"] = InjuryStatus.Missing_Healed;
            }
            #endregion
        }
    }

    //Initializes female health
    void SetFemaleHealth()
    {
        maxFertility = Random.Range(20, 70);
        numDaysCycleReset = Random.Range(21, 30);
        daysInCycle = Random.Range(0, numDaysCycleReset);
        maxFertilityDay = Mathf.RoundToInt(numDaysCycleReset / 2);
        if (daysInCycle == maxFertilityDay)
        {
            //If pawn has partner, is above age, and meets the fertility check
            /*if (pawnNeeds.partner != null && age > 18 && bodyDictionary["Genitals"] == InjuryStatus.Healthy)
            {
                int randomStartPregChance = Random.Range(0, 100);
                if (randomStartPregChance >= 2)
                {
                    Debug.Log("Pawn is pregnant");
                    pregnancyState = (PregnancyState)Random.Range(0, 4);

                    DetermineFemaleHealth();    //Send the status upriver for standard determination
                }
            }*/
        }
    }
    //Determined daily
    /*float CurveWeightedRandom(AnimationCurve curve)
    {
        return curve.Evaluate(Random.value);
    }
    */
    void DetermineFemaleHealth()
    {
        if (age < 45)
        {
            pregnancyState = PregnancyState.Menopause;
        }

        switch (pregnancyState)
        {
            case PregnancyState.Not_Pregnant:
                {
                    //Get rid of this -- 3mo's later; but replace with what?
                    if (daysInCycle == maxFertilityDay || daysInCycle == (maxFertilityDay + 5) || daysInCycle == (maxFertilityDay - 5))
                    {
                        if (daysInCycle != maxFertilityDay)
                        {
                            if (recentTryForChild == true)
                            {
                                //Still needs to check for proximity to max fertility day
                                float fertPercent = 0;  //This needs to check for % toward max fert, then % over max fert
                                float randNatFert = Random.Range(0, Mathf.RoundToInt(fertPercent)); //30% of 40 = (30/100)(40) = 12% -- Check this percentage. 
                                if (randNatFert < maxFertility)
                                {
                                    for (int i = 0; i < timesPregnant; i++)
                                    {
                                        fertility = fertility / 2;
                                    }
                                    float finalPregCheck = Mathf.RoundToInt(Random.Range(0, 100));
                                    if (finalPregCheck <= fertility)
                                    {
                                        daysInPregnancy = Random.Range(69, 74);
                                        daysInPregnancyRemaining = daysInPregnancy;
                                        pregnancyState = PregnancyState.First_Trimester;
                                    }
                                }
                                else
                                {
                                    fertility = 0;
                                }
                            }
                        }
                        else
                        {
                            if (recentTryForChild == true)
                            {
                                //Ignores the proximity to best fertility day
                                float randNatFert = Random.Range(0, 100);
                                if (randNatFert < maxFertility)
                                {
                                    for (int i = 0; i < timesPregnant; i++)
                                    {
                                        fertility = fertility / 2;
                                    }
                                    float finalPregCheck = Mathf.RoundToInt(Random.Range(0, 100));
                                    if (finalPregCheck <= fertility)
                                    {
                                        daysInPregnancy = Random.Range(69, 74);
                                        daysInPregnancyRemaining = daysInPregnancy;
                                        pregnancyState = PregnancyState.First_Trimester;
                                    }
                                }
                                else
                                {
                                    fertility = 0;
                                }
                            }
                        }
                    }
                    break;
                }
            case PregnancyState.First_Trimester:
                {
                    if (daysInPregnancyRemaining <= daysInPregnancy / 3f)
                    {
                        daysInPregnancyRemaining--;
                    }
                    else
                    {
                        pregnancyState = PregnancyState.Second_Trimester;
                    }
                    break;
                }
            case PregnancyState.Second_Trimester:
                {
                    if (daysInPregnancyRemaining <= (daysInPregnancy / 3f) + (daysInPregnancy / 3f))
                    {
                        daysInPregnancyRemaining--;
                    }
                    else
                    {
                        pregnancyState = PregnancyState.Third_Trimester;
                    }
                    break;
                }
            case PregnancyState.Third_Trimester:
                {
                    daysInPregnancyRemaining--;
                    if (daysInPregnancyRemaining <= 0)
                    {
                        GameObject newPawn = Instantiate(pawnPrefab, transform.position, transform.rotation);
                        //Update relations
                        pregnancyState = PregnancyState.Nursing;
                    }
                    break;
                }
            case PregnancyState.Nursing:
                {
                    //This acts as a cooldown between pregnancies
                    break;
                }
        }
    }
}
