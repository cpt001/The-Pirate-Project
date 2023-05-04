using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//[RequireComponent(typeof(PawnNavigation))]
public class PawnVisualGeneration : MonoBehaviour
{
    /// <summary>
    /// This is a new, paired down version of the Pawn Generation script, focused entirely on the visual generation
    /// 
    /// ToDo:
    /// -Finish implementing missing cases -- Female body shape, preferred partner stats(?), heterochromia
    /// -Implement new cases -- Clothing preference
    /// -Move personality segment into needs?
    /// -Player character creator exception -- Generate a random character to start, then allow customization from there
    /// </summary>
    #region Script References and Containers
    public NPCNames npcNamer;
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
    private EyeColor eyeColor;
    private enum EyeColor { Gray, Blue, Green, Brown, Red };
    private Color eyeColorToApply;
    public bool heterochromatic_NYI;  //NYI, allows for heterochromatic eye options
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
                    npcNamer.GetRandomMaleName();
                    break;
                }
            case PawnSex.Female:
                {
                    unitBody = femaleBody;
                    npcNamer.GetRandomFemaleName();
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
        genderPrefs = (PawnPreferredPartner)Random.Range(0, 2);
        characterName = npcNamer.selectedName;
        gameObject.name = "Pawn_" + npcNamer.selectedName + " [" + pawnGender + "]";
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


    private void Awake()
    {
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

    private void Start()
    {
        GenerateNewPawn();
    }
}
