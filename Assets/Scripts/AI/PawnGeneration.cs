using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PawnGeneration : MonoBehaviour
{
    /// <summary>
    /// --Removed intersex options from generation to match time period
    /// --Gamemanager removed, will implement an NPC list later with the new GM
    /// --Time scalar completely removed to make way for a scheduling system later on down the line
    /// --Removed pubic hair references, replaced with facial hair
    /// --Job references removed, system redundant, replaced with building controlled system instead
    /// --Health marked for update later
    /// --Annoyingly, I'm keeping female health. It's well thought out, and adds good depth to the simulation
    /// --Removed seduction and reduced opinion matrix
    /// --Reduced navigation section to planned systems
    /// --Calendar adjustment to 60 days in game. Time now moves ~6x as fast as real time. Avg lifespan: ~4800 days/80 years. 1-3 baby, 4-6/toddler, 7-12/kid, 13-16/teen, 17-55/adult, 56-80/elder
    /// </summary>
    #region ScriptReferences
    //public GameManager gameManager;
    public TimeScalar timeLord;
    public GameObject currentTaskMaster;
    public NPCNames npcNamer;
    public Rigidbody _rb;
    #endregion

    [Header("Special Cases")]
    public bool isPlayerPawn;   //This simply determines if this setup is automated, or if the variables are exposed during char creation
    private bool hasPartner;
    public Dictionary<PawnGeneration, int> opinionMatrix;   //This won't appear in the inspector anyway, might as well keep it here -- Opinion of other pawns that have been met
    private int moneyOnHand;

    /// <summary>
    /// This section determines the unit's external gender, sexual preference, and time of day preference
    /// </summary>
    [Header("Initialization")]
    #region
    public string characterName;                                                    //Name of the character
    public enum Gender { Male, Female };
    public Gender pawnGender;                                                       //Initializes, will be randomized for NPCs
    public bool hidingGender;                                                       //Built in to accomodate scenarios where an NPC might be trying to hide their gender identity (ship crew, draft dodging, etc)
    public enum GenderPreference { Male, Female, Bisexual, Asexual };               //Simplified - what gender do they prefer to be partnered with?
    public GenderPreference genderPrefs;                                            //Initialize default
    public enum TimePreference { NightOwl, EarlyRiser, Daybird, EveningDove };      //Preferred time of day to do activities + work
    public TimePreference timePreference;                                           //Randomize me per AI - 1-3 Night, 7-10 Day
    #endregion

    /// <summary>
    /// Eye, hair and skin color are determined in this section
    /// </summary>
    [Header("Looks")]
    #region
    public GameObject maleBody;
    public GameObject femaleBody;
    public GameObject unitBody;
    public EyeColor eyeColor;
    public enum EyeColor { Gray, Blue, Green, Brown, Red};
    private Color eyeColorToApply;
    public bool heterochromatic_NYI;
    public enum HairColor { White, Silver, Blonde, DirtyBlonde, Auburn, Brunette, Dark, Reddish, Red, Ginger };
    public HairColor hairColor;
    private Color hairColorToApply;
    public enum SkinColor { Albino, White, Tan, Brown, DarkBrown, Black };
    public SkinColor skinColor;
    private Color skinColorToApply;
    public enum HairLength
    {
        Bald,
        Buzz,   //Short
        Crew,   //Ear
        Neck,
        Shoulder,
        Armpit,
        Bra,
        Butt,
        Wig,
    }
    public HairLength hairLength;
    private int preferredHairType;
    private int preferredHairTypeOnPartner;

    public enum FacialHairType
    {
        Smooth,
        Oclock, //5oclock shadow
        Bush,
        Beard,
        Goatee,
        Full,
        FakeShadow,
        FakeMustache,
        FakeBeard,
    }
    public FacialHairType facialHairType_NYI;
    private int preferredFacialHairType;
    private int preferredFacialHairOnPartner;
    public enum UpperlipHair
    {
        Smooth,
        Peachfuzz,
        Pencil,
        English,
        FuManchu,
        Dali,
        Pancho,
        Mexican,
        Imperial,
        Handlebar,
        Horseshoe,
        Toothbrush,
        Walrus,
        Chevron,
    }
    public UpperlipHair upperLipHair;
    private int preferredUpperLipHairType;
    private int preferredUpperLipHairOnPartner;
    public enum BodyType
    {
        Skinny,
        Fit,
        Muscular,
        Normal,
        ALittleExtra,
        Chubby,
    }
    public BodyType bodyFat_NYI;
    private int preferredBodyFat;
    private int preferredBodyFatOnPartner;
    private GameObject eyeR;
    private GameObject eyeL;
    private GameObject hair;
    #endregion

    //[Header("What Job They Have")]

    [Header("Health")]
    #region
    public int age;   //Age of the character
    public int birthday;    //0 - 100, what day is their birthday?
    public float health = 100.0f;    //General stat, to be updated later
    public float moveSpeed; //How fast they move, ranged
    public int immuneSystem;  //How frequently they get sick -- 0 - 5 (Sickly, Susceptible, Normal, RarelySick, IronGuard)
    public enum Sickness { Healthy, Sick, Medicated }     //Is the character sick? Healthy = 100%, Sick = 50%, Medicated = 75% of usual stats 
    public Sickness isSick;
    #endregion

    [Header("Female Health")]
    #region
    public float fertility;/* = Random.Range(0.0f, 70.0f);*/ //Female only: Chance to get pregnant
    public int daysInCycle;   //Female only: Current day on period.
    public int numDaysCycleReset; /* = Random.Range(21.0f, 35.0f);*/    //Female only: Number of days in cycle (21 - 35)
    public int timesPregnant;  //Value should be very low chance >1% for 1 or 2.
    public bool currentlyPregnant;  //Starting value is VERY low >3%
    public int numDaysRemainPregnant;   //Female only: Determines the number of days they will remain pregnant
    public enum PregnancyState
    {
        Not_Pregnant,
        First_Trimester,    //Morning sickness, tiredness, cravings, mood swings, small bladder (w1-12)
        Second_Trimester,   //Aches, 'mask of pregnancy', stretch marks (w13-28)
        Third_Trimester,    //Lower fitness, leaky breasts, poor sleep, contractions
        Nursing,            //Custom state, simple cooldown period of about 2 weeks before normal activities resume
    }
    public PregnancyState pregnancyState;
    public enum FemaleBodyShape
    {
        Apple,  //Large top, small bottom
        Pear,   //Small top, large bottom
        Strawberry, //Much larger top, smaller bottom
        Banana, //Equal features
        Hourglass,  //Equal features, thin middle
    }
    public FemaleBodyShape femBody;
    #endregion


    [Header("Stat Affectors")]
    #region
    public float will;          //The unit's motivation, and willingness to go outside their preferences
    public float constitution;  //The unit's willingness to accomplish a goal
    #endregion

    [Header("Needs")]
    #region 
    public float comfort;   //How comfortable they are with the situation, and their surroundings
    public int hungerLevel; //How hungry they are 
    public int thirstLevel;
    public int tiredLevel; //How tired they are
    public float sobriety;  //How drunk they are
    public int hygiene;   //Seduction stat, how they smell
    public int socialNeed;  //How lonely they are -- affects multiple important stats
    #endregion

    [Header("Navigation")]
    #region
    public NavMeshAgent agent;
    //public Quarters homeQuarters;
    public Transform goingTo;
    #endregion

    [Header("Character")]
    #region
    public bool ultraMood;
    public enum Mood { Fine, Bored, Confident, Dazed, Embarrassed, Energized, Angry, Flirty, Focused, Happy, Inspired, Playful, Uncomfortable, Sad, Scared, Tense, Asleep };    //https://sims.fandom.com/wiki/Emotion
    public Mood currentMood;
    public enum UltraMood { Null, Bored_Catatonic, Embarrassed_Mortified, Angry_Enraged, Flirty_Amorous, Playful_Hysterical, Asleep_Cryo }; //These are exaggerations of current moods.
    public UltraMood superMood;
    public enum Archetypes { Sage, Artist, Hero, Caregiver, Jester, Innocent, Lover, Greedy, Everyman } //How do they speak to you? New - https://www.lipstickalley.com/threads/sims-4-wicked-whims-impression-feature.2291054/ \\Original - Warrior, Child, Orphan, Creator, Caregiver, Mentor, Joker, Magician, Ruler, Rebel, Lover, Seductress 
    public Archetypes characterArchetype;
    // Wicked whims uses archetypes sage, artist, hero, caregiver, jester, innocent, lover, greedy, everyman
    public enum Personality { Emo, Sunny, Smart, Dumb, Bored, Snide };    //How do they present themselves?
    //Napolean matrix -- dumb, energetic, smart, lazy
    //Could set it up based on intelligence vs dumb and energetic vs lazy?
    public Personality personalityType;
    #endregion

    void GenerateNewPawn()
    {
        #region Initialization [Gender, time, name]
        characterName = "NullName"; //Name of the character
        pawnGender = (Gender)Random.Range(0, 1);
        genderPrefs = (GenderPreference)Random.Range(0, 2);
        timePreference = (TimePreference)Random.Range(0, 1);
        characterName = npcNamer.selectedName;
                gameObject.name = "Pawn_" + npcNamer.selectedName + " [" + pawnGender + "]";
        if (characterName == "NullName")
        {
            Debug.Log("Character has no name! " + gameObject);
        }
        #endregion
        #region BodySetup
        switch (pawnGender)
        {
            case Gender.Male:
                {
                    unitBody = maleBody;
                    break;
                }
            case Gender.Female:
                {
                    unitBody = femaleBody;
                    break;
                }

        }
        transform.GetComponent<MeshRenderer>().enabled = false;
        unitBody = Instantiate(unitBody, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), Quaternion.identity, transform);
        #endregion
        //Need to fix all the bloody RGBA to fit 0-1 color schemas -- Fixed with Color32 instead
        #region EyeColor
        eyeColor = (EyeColor)Random.Range(0, System.Enum.GetValues(typeof(EyeColor)).Length);
        switch (eyeColor)
        {
            case EyeColor.Gray:
                {
                    eyeColorToApply = Color.gray;
                    break;
                }
            case EyeColor.Blue:
                {
                    eyeColorToApply = Color.blue;
                    break;
                }
            case EyeColor.Green:
                {
                    eyeColorToApply = Color.green;
                    break;
                }
            case EyeColor.Brown:
                {
                    eyeColorToApply = new Color(0, 0.5f, 1, 0.6f);
                    break;
                }
            case EyeColor.Red:
                {
                    eyeColorToApply = Color.red;
                    break;
                }
        }
        #endregion
        #region HairColor
        hairColor = (HairColor)Random.Range(0, System.Enum.GetValues(typeof(HairColor)).Length);
        switch (hairColor)
        {
            case HairColor.White:
                {
                    hairColorToApply = Color.white;
                    break;
                }
            case HairColor.Silver:
                {
                    hairColorToApply = new Color32(172, 170, 191, 1);
                    break;
                }
            case HairColor.Blonde:
                {
                    hairColorToApply = new Color32(205, 184, 157, 1);
                    break;
                }
            case HairColor.DirtyBlonde:
                {
                    hairColorToApply = new Color32(175, 129, 90, 1);
                    break;
                }
            case HairColor.Auburn:
                {
                    hairColorToApply = new Color32(190, 91, 43, 1);
                    break;
                }
            case HairColor.Brunette:
                {
                    hairColorToApply = new Color32(69, 41, 34, 1);
                    break;
                }
            case HairColor.Dark:
                {
                    hairColorToApply = new Color32(56, 44, 58, 1);
                    break;
                }
            case HairColor.Red:
                {
                    hairColorToApply = new Color32(147, 69, 37, 1);
                    break;
                }
            case HairColor.Ginger:
                {
                    hairColorToApply = new Color32(158, 102, 60, 1);
                    break;
                }
        }
        #endregion
        #region SkinColor
        //skinColor = (SkinColor)Random.Range(0, System.Enum.GetValues(typeof(SkinColor)).Length);
        //Move away from randomization, approach as a funnel system instead? -- 6 skin colors atm
        //This implementation is sloppy, but it provides a nice result. Fuck it.
        int rng1 = Random.Range(0, 100);
        if (rng1 <= 70)
        {
            int rng2 = Random.Range(0, 100);
            if (rng2 <= 50)
            {
                int rng3 = Random.Range(0, 100);
                if (rng3 <= 40)
                {
                    int rng4 = Random.Range(0, 100);
                    if (rng4 <= 30)
                    {
                        int rng5 = Random.Range(0, 100);
                        if (rng5 <= 30)
                        {
                            skinColor = SkinColor.Albino;
                        }
                        else
                        {
                            skinColor = SkinColor.Black;
                        }
                    }
                    else
                    {
                        skinColor = SkinColor.DarkBrown;
                    }
                }
                else
                {
                    skinColor = SkinColor.Brown;
                }
            }
            else
            {
                skinColor = SkinColor.Tan;
            }
        }
        else
        {
            skinColor = SkinColor.White;    //70% bias
        }
        switch (skinColor)
        {
            case SkinColor.Albino:  //0.001
                {
                    skinColorToApply = Color.white;
                    break;
                }
            case SkinColor.White:   //.30
                {
                    skinColorToApply = new Color32(153, 131, 103, 1);
                    break;
                }
            case SkinColor.Tan:     //.20
                {
                    skinColorToApply = new Color32(251, 194, 125, 1);
                    break;
                }
            case SkinColor.Brown:   //.20
                {
                    skinColorToApply = new Color32(143, 103, 63, 1);
                    break;
                }
            case SkinColor.DarkBrown:   //.15
                {
                    skinColorToApply = new Color32(85, 51, 22, 1);
                    break;
                }
            case SkinColor.Black:   //.15
                {
                    skinColorToApply = new Color32(44, 16, 17, 1);
                    break;
                }
        }

        foreach (Transform t in unitBody.transform)
        {
            if (t.GetComponent<Renderer>() != null)
            {
                if (t.GetComponent<Renderer>().material.name == "MainSkin" || t.GetComponent<Renderer>().material.name == "Genitals");
                {
                    //Debug.Log("Color applied");
                    t.GetComponent<Renderer>().material.color = skinColorToApply;
                }
            }
        }
        #endregion
        #region Hair and Eye Application
        Transform hairTarget = unitBody.transform.Find("Hair");
        hairTarget.GetComponent<Renderer>().material.color = hairColorToApply;
        hairLength = (HairLength)Random.Range(0, System.Enum.GetValues(typeof(HairLength)).Length);
        if (hairLength == HairLength.Bald)
        {
            if (pawnGender != Gender.Female)
            {
                hairTarget.gameObject.SetActive(false);
            }
            else
            {
                hairLength = HairLength.Buzz;
            }
        }
        
        foreach(Transform t in unitBody.transform)
        {
            //Debug.Log(t.name);
            if (!heterochromatic_NYI)
            {
                if (t.name == "Eyeball")
                {
                    t.GetComponent<Renderer>().material.color = eyeColorToApply;
                }
            }
            if (heterochromatic_NYI)
            {
                //Foreach loop for each eyeball
            }
        }
        #endregion
        #region Opinion Matrix
        opinionMatrix = new Dictionary<PawnGeneration, int>();
        foreach (PawnGeneration go in FindObjectsOfType<PawnGeneration>())
        {
            if (go != this)
            {
                opinionMatrix.Add(go, Random.Range(-10, 10));
            }
        }
        /*foreach (KeyValuePair<PawnGeneration, float> kvp in opinionMatrix) //Debug statement
        {
            Debug.Log("Name: " + kvp.Key.ToString() + " || Opinion: " + kvp.Value.ToString());
        }*/
        #endregion

        #region Health
        age = Random.Range(18, 41);
        birthday = Random.Range(0, 60);
        health = 100.0f;
        moveSpeed = Random.Range(2.2f, 3.5f);
        isSick = (Sickness)Random.Range(0, 2);
        hungerLevel = Random.Range(65, 100);
        tiredLevel = Random.Range(65, 100);
        //fitnessLevel = Random.Range(50, 100);   //If fitness is below threshold 70, 50, 20, they tired 1.5, 2, 4x faster
        #endregion

        #region Female Health
        if (pawnGender == Gender.Female)
        {
            fertility = Random.Range(0, 70);/* = Random.Range(0.0f, 70.0f);*/ //Female only: Chance to get pregnant
            daysInCycle = Random.Range(0, 20);   //Female only: Current day on period.
            numDaysCycleReset = Random.Range(21, 35); /* = Random.Range(21.0f, 35.0f);*/    //Female only: Number of days in cycle (21 - 35)
            timesPregnant = Random.Range(0, 5);  //Value should be very low chance >1% for 1 or 2.
            currentlyPregnant = false;  //Starting value is VERY low >3%
            numDaysRemainPregnant = 0;   //Female only: Determines the number of days they will remain pregnant
            pregnancyState = PregnancyState.Not_Pregnant;
        }
        else
        {
            fertility = 0;/* = Random.Range(0.0f, 70.0f);*/ //Female only: Chance to get pregnant
            daysInCycle = 0;   //Female only: Current day on period.
            numDaysCycleReset = 0; /* = Random.Range(21.0f, 35.0f);*/    //Female only: Number of days in cycle (21 - 35)
            timesPregnant = 0;  //Value should be very low chance >1% for 1 or 2.
            currentlyPregnant = false;  //Starting value is VERY low >3%
            numDaysRemainPregnant = 0;   //Female only: Determines the number of days they will remain pregnant
            pregnancyState = PregnancyState.Not_Pregnant;
        }
        #endregion

        #region Other Stats
        moneyOnHand = Random.Range(250, 2000);   //How much money they have onhand. 
        moneyOnHand = moneyOnHand * 2;
        moneyOnHand = Mathf.RoundToInt(moneyOnHand);
        moneyOnHand = moneyOnHand / 2;
        #endregion

        #region Character Archetype
        characterArchetype = (Archetypes)Random.Range(0, 13);
        personalityType = (Personality)Random.Range(0, 6);
        #endregion
    }

    void DisplayPlayerSettings()
    {

    }

    void GeneratePlayerPawn()
    {
        
    }

    /*void OpinionOfInteractable
    {
        #region Opinion of Interactable
        flexibleSexuality;
        alignmentOverall;  //Where they lie on the ship political spectrum.  (BDSM Anarchy - Sexual Deviant - Pervert - Corruptable - Neutral - Purist - Zealot - Incorruptable)
        alignmentToPlayer; //What they think of the player                   (Monster - Asshole - Bad - Idiot - Strange Face - Neutral -  Acquaintance - Coworker - Friendly - Interest - Crush - Lover - Fuck Me Now - Have my babies)
        maxAlignToPlayer;  //How far their opinion can actually be swayed..  (This should be a range based on day to day.)
        comfort;   //How comfortable they are with the situation. Player coming on too strong may lower.
        hasPartner; //If the AI has a partner already, they can be coerced into a new relationship, but this task may be more difficult than others
        relationQuality; //Quality of current relationship. The lower, the more easily it can be broken.
        numAutoFucks;    //Number of sessions since AutoFuck
        sexFails;        //Number of fails
        forgivenessTolerance;    //If the relation quality, autofucks, sexfails, or really any stat drops too low, this is the amount of tolerance the AI will have before breaking up with MC
        #endregion
    }*/
    void DayToDayIncrementals()
    {

    }
    void DayToDayRandomization()
    {

    }

    private void Awake()
    {
        if (!timeLord)
        {
            timeLord = GameObject.Find("DemoLighting").GetComponent<TimeScalar>();
        }
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        RaycastHit rayhit;
        if (Physics.Raycast(transform.position, Vector3.down, out rayhit, 3.0f))
        {
            if (rayhit.transform.GetComponent<IslandController>())
            {
                rayhit.transform.GetComponent<IslandController>().pawnsOnIsland.Add(this);
            }
            else if (rayhit.transform.GetComponent<ControllableShip>())
            {

            }
            //else if (rayhit.transform.GetComponent<AIShip>().pawnsOnShip.Add(this))
        }

        //gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        //gameManager.gameUnits.Add(gameObject);
        //timeLord = gameManager.gameObject.GetComponent<TimeScalar>();     
        if (!isPlayerPawn)
        {
            GenerateNewPawn();
        }
        else
        {
            GeneratePlayerPawn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        DisplayPlayerSettings();

        #region Time Increments; Pulled from Timelord script
        if (timeLord.newDayTriggered)
        {
            if (timeLord.dayNumber == birthday)
            {
                age++;
                Debug.Log("Happy " + age + " Birthday, " + name + "!"); //works! :D
            }
            DayToDayIncrementals();
            DayToDayRandomization();
        }
        #endregion
    }
}
