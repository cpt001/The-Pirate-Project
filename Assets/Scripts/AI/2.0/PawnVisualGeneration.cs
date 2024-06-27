using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//[RequireComponent(typeof(PawnNavigation))]
public class PawnVisualGeneration : MonoBehaviour
{
    /// <summary>
    /// This is a new, paired down version of the Pawn Generation script, focused entirely on the visual generation of a new pawn.
    /// 
    /// Nuked NPC namer script. It started irritating me, and was functionally unnecessary
    /// 
    /// ToDo:
    /// -Finish implementing missing cases -- Female body shape, preferred partner stats(?)
    /// -Implement new cases -- Clothing preference
    /// -Move personality segment into needs?
    /// -Player character creator exception -- Generate a random character to start, then allow customization from there
    /// </summary>
    #region Script References and Containers
    public GameObject maleBody;
    public GameObject femaleBody;
    public GameObject unitBody = null;
    #endregion

    [Header("Special Cases")]
    //public bool isPlayerPawn;   //This simply determines if this setup is automated, or if the variables are exposed during char creation
    public string workTitle = null;

    /// <summary>
    /// This section determines the unit's external gender, sexual preference, and time of day preference
    /// </summary>
    [Header("Initialization")]
    #region
    private string selectedName;
    public string characterName;                                                    //Name of the character
    public enum PawnSex { Male, Female };
    public PawnSex pawnGender;                                                       //Initializes, will be randomized for NPCs
    public bool hidingGender;                                                       //Built in to accomodate scenarios where an NPC might be trying to hide their gender identity (ship crew, draft dodging, etc)
    public enum PawnPreferredPartner { Male, Female, Bisexual, Asexual };               //Simplified - what gender do they prefer to be partnered with?
    public PawnPreferredPartner genderPrefs;                                            //Initialize default
    #endregion

    /// <summary>
    /// Eye, hair and skin color are determined in this section
    /// </summary>
    [Header("Looks")]
    #region
    private EyeColor eyeColorRight;
    private EyeColor eyeColorLeft;
    private enum EyeColor { Gray, Blue, Green, Brown, Red };
    private Color eyeColorRToApply;
    private Color eyeColorLToApply;
    private bool heterochromatic;
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

    //Move determinators to needs?
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
        #region Character Archetype
        characterArchetype = (Archetypes)Random.Range(0, 13);
        personalityType = (Personality)Random.Range(0, 6);
        #endregion
    }

    void SetBody()
    {
        #region BodySetup
        pawnGender = (PawnSex)Random.Range(0, 2);

        switch (pawnGender)
        {
            case PawnSex.Male:
                {
                    unitBody = maleBody;
                    GetRandomMaleName();
                    break;
                }
            case PawnSex.Female:
                {
                    unitBody = femaleBody;
                    GetRandomFemaleName();
                    break;
                }

        }
        transform.GetComponent<MeshRenderer>().enabled = false;
        unitBody = Instantiate(unitBody, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity, transform);
        #endregion
        #region SetHeight
        float heightScalar = Random.Range(0.75f, 1f);
        unitBody.transform.localScale = new Vector3(heightScalar, heightScalar, heightScalar);
        #endregion

    }
    void DetermineUniqueCharacteristics()
    {
        #region Initialization [Gender, time, name, agent move speed]
        characterName = "NullName"; //Name of the character
        genderPrefs = (PawnPreferredPartner)Random.Range(0, 2);
        gameObject.name = "Pawn_" + selectedName + " [" + pawnGender + "]";
        /*if (characterName == "NullName")
        {
            Debug.Log("Character has no name! " + gameObject);
        }*/

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
        eyeColorRight = (EyeColor)Random.Range(0, System.Enum.GetValues(typeof(EyeColor)).Length);
        if (heterochromatic)
        {
            eyeColorLeft = (EyeColor)Random.Range(0, System.Enum.GetValues(typeof(EyeColor)).Length);
        }
        else
        {
            eyeColorLeft = eyeColorRight;
        }
        switch (eyeColorRight)
        {
            case EyeColor.Gray:
                {
                    eyeColorRToApply = Color.gray;
                    break;
                }
            case EyeColor.Blue:
                {
                    eyeColorRToApply = Color.blue;
                    break;
                }
            case EyeColor.Green:
                {
                    eyeColorRToApply = Color.green;
                    break;
                }
            case EyeColor.Brown:
                {
                    eyeColorRToApply = new Color(0, 0.5f, 1, 0.6f);
                    break;
                }
            case EyeColor.Red:
                {
                    eyeColorRToApply = Color.red;
                    break;
                }
        }        
        switch (eyeColorLeft)
        {
            case EyeColor.Gray:
                {
                    eyeColorLToApply = Color.gray;
                    break;
                }
            case EyeColor.Blue:
                {
                    eyeColorLToApply = Color.blue;
                    break;
                }
            case EyeColor.Green:
                {
                    eyeColorLToApply = Color.green;
                    break;
                }
            case EyeColor.Brown:
                {
                    eyeColorLToApply = new Color(0, 0.5f, 1, 0.6f);
                    break;
                }
            case EyeColor.Red:
                {
                    eyeColorLToApply = Color.red;
                    break;
                }
        }

        foreach (Transform t in unitBody.transform)
        {
            if (t.name == "Eyeball")
            {
                foreach (Transform u in t.transform)
                {
                    if (u.Find("Pupil (R)"))
                    {
                        Debug.Log(u.name + " on AI tester");

                        u.GetComponent<Renderer>().material.color = eyeColorRToApply;
                    }
                    if (u.Find("Pupil (L)"))
                    {
                        u.GetComponent<Renderer>().material.color = eyeColorLToApply;
                    }
                }
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
            if (pawnGender != PawnSex.Female)
            {
                hairTarget.gameObject.SetActive(false);
            }
            else
            {
                hairLength = HairLength.Buzz;
            }
        }
    }   //Bias?


    /*protected override*/ void Awake()
    {
        //base.Awake();
        Init();
    }

    private void Init()
    {
        SetBody();
        DetermineUniqueCharacteristics();
        DetermineSkinColor();
        DetermineHair();
        DetermineEyeColor();
        DetermineHairStyle();

        //Job determination here, i think. 
    }

    /*protected override*/ void Start()
    {
        //base.Start();
        GenerateNewPawn();
    }


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
