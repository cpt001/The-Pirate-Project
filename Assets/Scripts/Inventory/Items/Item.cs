using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    [Header("Item Information")]
    //public string itemName;
    public Sprite itemSprite;
    public enum ItemRarity
    {
        undefined,
        Crude,              //55% drop chance
        StandardIssue,      //35% drop chance
        PrecisionBuilt,     //15% drop chance
        MasterCraft,        //5% drop chance
        Legendary,          //1% drop chance
        WhisperedOf,        //.1% drop chance
    }
    //[SerializeField] private
    public ItemRarity itemRarity;
    public enum ItemClass
    {
        undefined,
        MissionItem,
        Currency,
        Consumable,
        Ammunition,
        Clothing,
        Weapon,
        /*
        Melee,
        Firearm,
        Voodoo,
        */
    }
    public ItemClass itemClass;
    [Header("Player Interaction Rules")]
    [SerializeField] private float healthAdjustment;
    [SerializeField] private bool playerCanConsumeManually;
    [SerializeField] private bool playerCanEquip;
    [SerializeField] private int value;
    public int maxStack;

    [Header("Weapon Ammunition Data")]
    [SerializeField] private bool requiresAmmunition;
    [SerializeField] private int numberAvailableShots;
    [SerializeField] private int numberMaxShots;

    [Header("Building Interaction Rules")]
    [SerializeField] private float craftingTime;
    public List<CargoSO> craftingMaterials = new List<CargoSO>();


    private void Awake()
    {
        if (itemClass == ItemClass.undefined || itemRarity == ItemRarity.undefined)
        {
            Debug.LogWarning(name + " has an undefined property");
        }
    }
}
