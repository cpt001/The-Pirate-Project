using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Drop Table", menuName = "LootItem")]
public class UnitDropTable : ScriptableObject
{
    public List<Item> possibleItemDrops; //Add items to possible drops, and script will create drop chance probability.
    public Dictionary<Item, Item.ItemRarity> itemDropChance = new Dictionary<Item, Item.ItemRarity>();
}
