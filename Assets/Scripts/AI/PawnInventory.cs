using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnInventory : MonoBehaviour
{
    public int copper, silver, gold;
    public int maxInventorySlotCount;
    public List<Item> inventoryList = new();
}
