using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class PawnGeneration : MonoBehaviour
{
    /// <summary>
    /// [DEFUNCT] This is the original pawn generation formula. 
    /// 
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
    /// --Navigation moved to PawnNavigation script for clarity
    /// </summary>
    #region ScriptReferences
    //public GameManager gameManager;
    public TimeScalar timeLord;
    public GameObject currentTaskMaster;
    public PawnNavigation pawnNavigator;
    //[SerializeField] private Transform goingTo;
    #endregion

    [Header("Special Cases")]
    //public bool isPlayerPawn;   //This simply determines if this setup is automated, or if the variables are exposed during char creation
    public bool hasPartner;
    public PawnGeneration partner;
    public Dictionary<PawnGeneration, int> opinionMatrix;   //This won't appear in the inspector anyway, might as well keep it here -- Opinion of other pawns that have been met
    private int moneyOnHand;
    private IslandController islandController = null;
    public string workTitle = null;

    /// <summary>
    /// This section determines the unit's external gender, sexual preference, and time of day preference
    /// </summary>
    [Header("Initialization")]
    #region
    private string selectedName;

    public string characterName;                                                    //Name of the character
    public enum Gender { Male, Female };
    public Gender pawnGender;                                                       //Initializes, will be randomized for NPCs
    public bool hidingGender;                                                       //Built in to accomodate scenarios where an NPC might be trying to hide their gender identity (ship crew, draft dodging, etc)
    public enum GenderPreference { Male, Female, Bisexual, Asexual };               //Simplified - what gender do they prefer to be partnered with?
    public GenderPreference genderPrefs;                                            //Initialize default
    private enum WorkHours { First, Second, Grave };    //6p-2, 12p-8, 8p-4
    private WorkHours workingHours;
    #endregion

    [Header("DailyDetails")]
    #region
    //Everyday
    private bool nightOwl;
    public int sleepStartTime; //9-10p || 12-2p
    private int sleepLength;    //7-10 hours
    //Sundays

    #endregion

    /// <summary>
    /// Eye, hair and skin color are determined in this section
    /// </summary>
    [Header("Looks")]
    #region
    public GameObject maleBody;
    public GameObject femaleBody;
    public GameObject unitBody = null;
    public EyeColor eyeColor;
    public enum EyeColor { Gray, Blue, Green, Brown, Red };
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
        Oclock, //5 o'clock shadow
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

    [Header("Health")]
    #region
    public int age;   //Age of the character
    private int birthday;    //0 - 100, what day is their birthday?
    public float health = 100.0f;    //General stat, to be updated later
    public int immuneSystem;  //How frequently they get sick -- 0 - 5 (Sickly, Susceptible, Normal, RarelySick, IronGuard)
    public enum Sickness { Healthy, Sick, Medicated, Bedridden }     //Is the character sick? Healthy = 100%, Sick = 50%, Medicated = 75% of usual stats 
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

    //[Header("Navigation")]    --Moved to its own script for clarity purposes while programming

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

    #region AffectorVariables
    private UnityAction newDestinationAssigned;
    private int currentHourCount = -1;  //Hacky fix to get around an issue created from running days in start
    private int internalDayCount;
    #endregion

    void GenerateNewPawn()
    {
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

    void SetBody()
    {
        #region BodySetup
        pawnGender = (Gender)Random.Range(0, 2);

        switch (pawnGender)
        {
            case Gender.Male:
                {
                    unitBody = maleBody;
                    GetRandomMaleName();
                    break;
                }
            case Gender.Female:
                {
                    unitBody = femaleBody;
                    GetRandomFemaleName();
                    break;
                }

        }
        transform.GetComponent<MeshRenderer>().enabled = false;
        unitBody = Instantiate(unitBody, new Vector3(transform.position.x, transform.position.y - 1, transform.position.z), Quaternion.identity, transform);
        #endregion
        #region SetHeight
        float heightScalar = Random.Range(0.9f, 1.1f);
        unitBody.transform.localScale = new Vector3(heightScalar, heightScalar, heightScalar);
        #endregion
    }
    void DetermineUniqueCharacteristics()
    {
        #region Initialization [Gender, time, name, agent move speed]
        characterName = "NullName"; //Name of the character
        genderPrefs = (GenderPreference)Random.Range(0, 2);
        characterName = selectedName;
        gameObject.name = "Pawn_" + selectedName + " [" + pawnGender + "]";
        if (characterName == "NullName")
        {
            Debug.Log("Character has no name! " + gameObject);
        }

        //Heterochromia determined here
        #endregion
    }

    void DetermineSkinColor()
    {
        #region SkinColor
        //skinColor = (SkinColor)Random.Range(0, System.Enum.GetValues(typeof(SkinColor)).Length);
        //Move away from randomization, approach as a funnel system instead? -- 6 skin colors atm
        //This implementation is sloppy, but it provides a nice result. Fuck it.
        int rng1 = Random.Range(0, 100);
        if (rng1 <= 40)
        {
            int rng2 = Random.Range(0, 100);
            if (rng2 <= 30)
            {
                int rng3 = Random.Range(0, 100);
                if (rng3 <= 20)
                {
                    int rng4 = Random.Range(0, 100);
                    if (rng4 <= 15)
                    {
                        int rng5 = Random.Range(0, 100);
                        if (rng5 <= 5)
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
        //Need to fix this again
        foreach (Transform t in unitBody.transform)
        {
            if (t.name == "HumanMaleRigged_LOD")
            {
                foreach (Transform u in t)
                {
                    u.GetComponentInChildren<Renderer>().material.color = skinColorToApply;
                }
            }
            if (t.name == "HumanFemaleRigged_LOD")
            {
                foreach (Transform u in t)
                {
                    u.GetComponentInChildren<Renderer>().material.color = skinColorToApply;
                }
            }
            else
            {
                //Do nothin
            }
        }
        #endregion
    }   //Needs bias based on island
    void DetermineHair()    //Needs updating with ranked choices
    {
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
    }
    void DetermineEyeColor()
    {
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

        foreach (Transform t in unitBody.transform)
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

    }
    void DetermineHairStyle()
    {
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
    }   //Bias?
    void SendPawnToIslandController()
    {
        if (islandController)
        {
            islandController.unassignedWorkers.Add(gameObject);
            EventsManager.TriggerEvent("PawnAddedToIsland");
        }
        else
        {
            Debug.Log("Island controller is null");
        }
        //Check sleeping hours
        //Check for jobs that align with hours
        //Add name to job roster (Structure) -- jobs listed under island controller
        //If job roster is full, change sleeping hours?
        //If all job rosters on island are full, set job to bum
        //If bum job > #jobs + #structure, remove pawn
    }


    private void Awake()
    {
        islandController = GetComponentInParent<IslandController>();
        pawnNavigator = GetComponent<PawnNavigation>();
        Init();
        EventsManager.StartListening("NewDay", DayToDayIncrementals);
    }

    private void Init()
    {
        SetBody();
        DetermineUniqueCharacteristics();
        //WorkingHours();   //Working hours is being changed. Job location is determined when the pawn enters a structure's premises. When the pawn is at their position, they'll receive wait time, pause commands, and contribute to resource gen speed
        DetermineSkinColor();
        DetermineHair();
        DetermineEyeColor();
        DetermineHairStyle();

        //Job determination here, i think. 
    }

    private void Start()
    {
        #region Island Raycasting
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
        #endregion

        GenerateNewPawn();
        SendPawnToIslandController(); //This needs to be called after the structure's awake functions
    }

    /// <summary>
    /// Need to build working hours and social hours
    /// current - 8 sleep, 4 social, 8-10 work - leaves 2-4 hours free
    /// social - 2 before, 2 after?
    /// work - 8-10 hours, stops if production structure is full
    /// </summary>

    void DayToDayIncrementals()
    {
        internalDayCount++;
        currentHourCount = 0;
        if (internalDayCount == birthday)
        {
            age++;
            //Debug.Log("Happy " + age + " Birthday, " + name + "!"); //works! :D
        }
    }



    ///Next steps:
    //Look for cargo, set destination from cargo.
    //Fix scheduling

    private string[] namesMale = new string[]
{
        "Noah",
        "Liam",
        "Jacob",
        "William",
        "Mason",
        "Ethan",
        "Michael",
        "Alexander",
        "James",
        "Elijah",
        "Benjamin",
        "Daniel",
        "Aiden",
        "Logan",
        "Jayden",
        "Matthew",
        "Lucas",
        "David",
        "Jackson",
        "Joseph",
        "Anthony",
        "Samuel",
        "Joshua",
        "Gabriel",
        "Andrew",
        "John",
        "Christopher",
        "Oliver",
        "Dylan",
        "Carter",
        "Isaac",
        "Luke",
        "Henry",
        "Owen",
        "Ryan",
        "Nathan",
        "Wyatt",
        "Caleb",
        "Sebastian",
        "Jack",
        "Christian",
        "Jonathan",
        "Julian",
        "Landon",
        "Levi",
        "Isaiah",
        "Hunter",
        "Aaron",
        "Charles",
        "Thomas",
        "Eli",
        "Jaxon",
        "Connor",
        "Nicholas",
        "Jeremiah",
        "Grayson",
        "Cameron",
        "Brayden",
        "Adrian",
        "Evan",
        "Jordan",
        "Josiah",
        "Angel",
        "Robert",
        "Gavin",
        "Tyler",
        "Austin",
        "Colton",
        "Jose",
        "Dominic",
        "Brandon",
        "Ian",
        "Lincoln",
        "Hudson",
        "Kevin",
        "Zachary",
        "Adam",
        "Mateo",
        "Jason",
        "Chase",
        "Nolan",
        "Ayden",
        "Cooper",
        "Parker",
        "Xavier",
        "Asher",
        "Carson",
        "Jace",
        "Easton",
        "Justin",
        "Leon",
        "Bentley",
        "Jaxson",
        "Nathaniel",
        "Blake",
        "Elias",
        "Theodore",
        "Kayden",
        "Luis",
        "Tristan",
        "Ezra",
        "Bryson",
        "Juan",
        "Brody",
        "Vincent",
        "Micah",
        "Miles",
        "Santiago",
        "Cole",
        "Ryder",
        "Carlos",
        "Damian",
        "Leonardo",
        "Roman",
        "Max",
        "Sawyer",
        "Jesus",
        "Diego",
        "Greyson",
        "Alex",
        "Maxwell",
        "Axel",
        "Eric",
        "Wesley",
        "Declan",
        "Giovanni",
        "Ezekiel",
        "Braxton",
        "Ashton",
        "Ivan",
        "Harden",
        "Camden",
        "Silas",
        "Bryce",
        "Weston",
        "Harrison",
        "Jameson",
        "George",
        "Antonio",
        "Timothy",
        "Kaiden",
        "Jonah",
        "Everett",
        "Miguel",
        "Steven",
        "Richard",
        "Emmett",
        "Victor",
        "Kaleb",
        "Kai",
        "Maverick",
        "Joel",
        "Bryan",
        "Maddox",
        "Kingston",
        "Aidan",
        "Patrick",
        "Edward",
        "Emmanuel",
        "Jude",
        "Alejandro",
        "Preston",
        "Luca",
        "Bennett",
        "Jesse",
        "Colin",
        "Jaden",
        "Malachi",
        "Kaden",
        "Jayce",
        "Alan",
        "Kyle",
        "Marcus",
        "Brian",
        "Ryker",
        "Grant",
        "Jeremy",
        "Abel",
        "Riley",
        "Calvin",
        "Brantley",
        "Caden",
        "Oscar",
        "Abraham",
        "Brady",
        "Sean",
        "Jake",
        "Tucker",
        "Nicolas",
        "Mark",
        "Amir",
        "Avery",
        "King",
        "Gael",
        "Kenneth",
        "Bradley",
        "Cayden",
        "Xander",
        "Graham",
        "Rowan",
};
    public void GetRandomMaleName()
    {
        selectedName = namesMale[Random.Range(0, namesMale.Length)];
    }
    private string[] namesFemale = new string[]
    {
        "Emma",
        "Olivia",
        "Sophia",
        "Isabella",
        "Ava",
        "Mia",
        "Abigail",
        "Emily",
        "Charlotte",
        "Madison",
        "Elizabeth",
        "Amelia",
        "Evelyn",
        "Ella",
        "Chloe",
        "Harper",
        "Avery",
        "Sofia",
        "Grace",
        "Addison",
        "Victoria",
        "Lily",
        "Natalie",
        "Aubrey",
        "Lillian",
        "Zoey",
        "Hannah",
        "Layla",
        "Brooklyn",
        "Scarlett",
        "Zoe",
        "Camila",
        "Samantha",
        "Riley",
        "Leah",
        "Aria",
        "Savannah",
        "Audrey",
        "Anna",
        "Allison",
        "Gabriella",
        "Claire",
        "Hailey",
        "Penelope",
        "Aaliyah",
        "Sarah",
        "Nevaeh",
        "Kaylee",
        "Stella",
        "Mila",
        "Nora",
        "Ellie",
        "Bella",
        "Alexa",
        "Lucy",
        "Arianna",
        "Violet",
        "Ariana",
        "Genesis",
        "Alexis",
        "Eleanor",
        "Maya",
        "Caroline",
        "Payton",
        "Skylar",
        "Madelyn",
        "Serenity",
        "Kennedy",
        "Taylor",
        "Alyssa",
        "Autumn",
        "Paisley",
        "Ashley",
        "Brianna",
        "Sadie",
        "Naomi",
        "Kylie",
        "Julia",
        "Sophie",
        "Mackenzie",
        "Eva",
        "Gianna",
        "Luna",
        "Katherine",
        "Hazel",
        "Khloe",
        "Ruby",
        "Melanie",
        "Piper",
        "Lydia",
        "Aubree",
        "Madeline",
        "Aurora",
        "Faith",
        "Alexandra",
        "Alice",
        "Kayla",
        "Jasmine",
        "Maria",
        "Annabelle",
        "Lauren",
        "Reagan",
        "Elena",
        "Rylee",
        "Isabelle",
        "Bailey",
        "Eliana",
        "Sydney",
        "Makayla",
        "Cora",
        "Morgan",
        "Natalia",
        "Kimberly",
        "Vivian",
        "Quinn",
        "Valentina",
        "Andrea",
        "Willow",
        "Clara",
        "London",
        "Jade",
        "Liliana",
        "Jocelyn",
        "Kinsley",
        "Trinity",
        "Brielle",
        "Mary",
        "Molly",
        "Hadley",
        "Delilah",
        "Emilia",
        "Josephine",
        "Brooke",
        "Ivy",
        "Lilly",
        "Adeline",
        "Payton",
        "Lyla",
        "Isla",
        "Jordyn",
        "Paige",
        "Isabel",
        "Mariah",
        "Mya",
        "Nicole",
        "Valeria",
        "Destiny",
        "Rachel",
        "Ximena",
        "Emery",
        "Everly",
        "Sara",
        "Angelina",
        "Adalynn",
        "Kendall",
        "Reese",
        "Aliyah",
        "Margaret",
        "Juliana",
        "Melody",
        "Amy",
        "Eden",
        "Mckenzie",
        "Laila",
        "Vanessa",
        "Ariel",
        "Gracie",
        "Valerie",
        "Adalyn",
        "Brooklynn",
        "Gabrielle",
        "Kaitlyn",
        "Athena",
        "Elise",
        "Jessica",
        "Adriana",
        "Leilani",
        "Ryleigh",
        "Daisy",
        "Nova",
        "Norah",
        "Eliza",
        "Rose",
        "Rebecca",
        "Michelle",
        "Alaina",
        "Catherine",
        "Londyn",
        "Summer",
        "Lila",
        "Jayla",
        "Katelyn",
        "Daniela",
        "Harmony",
        "Alana",
        "Amaya",
        "Emerson",
        "Julianna",
        "Cecilia",
        "Izabella",
};
    public void GetRandomFemaleName()
    {
        selectedName = namesFemale[Random.Range(0, namesFemale.Length)];
    }
}
