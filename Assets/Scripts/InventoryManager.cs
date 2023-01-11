using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private bool isPlayerInventory;
    [SerializeField] private bool isLoot;
    [SerializeField] private UnitDropTable dropTable;
    [SerializeField] private int inventorySlotCount;
    [SerializeField] private Transform InventoryGameobject;
    private Dictionary<Slot, Item> itemAndPosition = new Dictionary<Slot, Item>();
    [SerializeField] private Item itemInHand;
    private int numberOfItemInHand;
    [SerializeField] private Item itemInSlot = null;
    [HideInInspector] public Slot targetSlot;    //Get from raycast to UI
    [SerializeField] private GameObject handSlotObject;
    private Transform targetInventory = null;
    [SerializeField] private List<Item> lootContainer = new List<Item>();

    // Start is called before the first frame update
    void Start()
    {
        if (!InventoryGameobject && !isLoot)
        {
            Debug.LogError("There is no inventory object assigned to: " + gameObject.name);
        }
        if (!InventoryGameobject && isLoot)
        {
            if (!dropTable)
            {
                Debug.Log(name + " flagged as loot, but is missing a drop table!");
            }
            else
            {
                GenerateLoot(); //This is here temporarily. When something is killed, it should trigger this, passing in the drop table on its character
                IdentifyInventoryTarget();
            }
        }
        else
        {
            StartCoroutine(DefinePlayerInventorySlots());
        }
    }

    //Can add a case for generating an inventory here; a bool will tell if the inventory needs to be populated.
    //If the bool is true, limit the number of slots to exactly the number of items in that inventory

    private void LateUpdate()
    {
        if (targetSlot != null)
        {
            itemInSlot = targetSlot.itemInSlot;
            TransferItemToSlot();
            if (Input.GetMouseButtonDown(0) && !Input.GetKeyDown(KeyCode.LeftShift))
            {
                targetSlot.slotStatus++;    //This changes the sell behavior of a specific slot
            }
            if (Input.GetMouseButtonDown(0) && Input.GetKeyDown(KeyCode.LeftShift))
            {
                QuickTransferItemToOpenSlot();
            }
        }
        if (itemInHand != null)
        {
            Debug.Log("Item detected in hand");
            handSlotObject.GetComponent<SpriteRenderer>().sprite = itemInHand.itemSprite;
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 1f;
            handSlotObject.transform.position = Camera.main.ScreenToWorldPoint(mousePos);
        }
        else
        {
            handSlotObject.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    private IEnumerator DefinePlayerInventorySlots()    //The entire point of adding this to a dictionary is to account for sell behavior
    {
        foreach (Transform t in InventoryGameobject)
        {
            if (t.tag == "InventorySlot")
            {
                //Debug.Log("Found inventory slot!");
                itemAndPosition.Add(t.GetComponent<Slot>(), null);
                t.GetComponent<Slot>().inventoryTarget = this;
            }
            else
            {
                //Debug.Log("No inventory slots found on: " + t.name + ", have you assigned an InventorySlot tag?");
            }
        }
        inventorySlotCount = itemAndPosition.Count;
        yield return null;
    }

    void TransferItemToSlot()  //Click to grab item
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Skipped case - item and target both null
            if (itemInHand == null && targetSlot.itemInSlot != null)    //Item in slot, no item in hand
            {
                itemInHand = targetSlot.itemInSlot;
                numberOfItemInHand = targetSlot.numberOfItem;
                targetSlot.itemInSlot = null;
                Debug.Log("Transfer triggered, item in slot: " + targetSlot.itemInSlot + " || Item in hand: " + itemInHand);

            }
            else if (itemInHand != null && targetSlot.itemInSlot == null)    //Item in hand, no item in slot
            {
                if (targetSlot.itemRestrictionsOnSlot == Slot.ItemTypeRestrictor.Unrestricted)
                {
                    targetSlot.itemInSlot = itemInHand;
                    targetSlot.numberOfItem = numberOfItemInHand;
                    itemInHand = null;
                    Debug.Log("Transfer triggered, item in hand: " + itemInSlot + " || adding to slot!");
                }
                else if (itemInHand.itemClass.ToString() == targetSlot.itemRestrictionsOnSlot.ToString())   //If the item class and restrictor match
                {
                    targetSlot.itemInSlot = itemInHand;
                    targetSlot.numberOfItem = numberOfItemInHand;
                    itemInHand = null;
                    Debug.Log("Transfer triggered, item in hand: " + itemInSlot + " || restriction met, adding to slot!");
                }
                else
                {
                    Debug.Log("Case mismatch, ignoring");
                }

            }
            else if (itemInHand != null && targetSlot.itemInSlot != null)    //If there's an item in hand, and an item in slot
            {
                if (itemInHand.name == targetSlot.itemInSlot.name)  //If the items are identical
                {
                    Debug.Log("Identical item found");
                    if (targetSlot.numberOfItem >= itemInHand.maxStack) //Less than max stack
                    {
                        if ((targetSlot.numberOfItem + numberOfItemInHand) <= itemInHand.maxStack)  //If the total is less than the max stack
                        {
                            targetSlot.numberOfItem += numberOfItemInHand;
                            numberOfItemInHand = 0;
                            itemInHand = null;
                            Debug.Log("Adding to stack!");
                        }
                        else    //If the total exceeds the max stack
                        {
                            int totalAllowed = targetSlot.itemInSlot.maxStack - targetSlot.numberOfItem;
                            targetSlot.numberOfItem += totalAllowed;
                            numberOfItemInHand -= totalAllowed;
                        }

                    }
                    else
                    {
                        Debug.Log("Cannot place in stack, item is probably at max count.");
                        //Throw an error to GUI
                    }
                }
                else if (itemInHand.name != targetSlot.itemInSlot.name) //Not the same item
                {
                    Item tempItem;
                    tempItem = targetSlot.itemInSlot;
                    targetSlot.itemInSlot = itemInHand;
                    itemInHand = tempItem;
                    tempItem = null;
                }
                //Ignore, throw UI error - slot already occupied
            }
        }
    }

    void QuickTransferItemToOpenSlot()  //IE: Shift click 
    {
        Debug.Log("Quick Transfer Triggered");
        if (isPlayerInventory)  //Swap out of inventory
        {
            //Define target inventory
            //Check inventory slot count
            //If slot available
            //Clone item to slot
            //Delete item from old slot
        }
        else
        {

        }
    }

    void GenerateLoot()
    {
        foreach (KeyValuePair<Slot, Item> targetSlot in itemAndPosition)
        {
            Debug.Log("Slots: " + targetSlot.Key + " || Items: " + targetSlot.Value);
        }
        foreach (Item lootOption in dropTable.possibleItemDrops)
        {
            int dropChance;
            switch (lootOption.itemRarity)
            {
                case Item.ItemRarity.Crude:
                    {
                        dropChance = Random.Range(0, 100);
                        if (dropChance < 55)
                        {
                            Debug.Log("Drop possible: " + lootOption + " || Loot Container: " + lootContainer);

                            inventorySlotCount++;
                            lootContainer.Add(lootOption);  //Null ref from this, but the loot option's rarity is found? -- Null ref was caused cause list wasnt being initialized

                            break;
                        }
                        break;
                    }
                case Item.ItemRarity.StandardIssue:
                    {
                        dropChance = Random.Range(0, 100);
                        if (dropChance < 35)
                        {
                            Debug.Log("Drop possible: " + lootOption + " || Loot Container: " + lootContainer);
                            inventorySlotCount++;
                            lootContainer.Add(lootOption);

                            break;
                        }
                        break;
                    }
                case Item.ItemRarity.PrecisionBuilt:
                    {
                        dropChance = Random.Range(0, 100);
                        if (dropChance < 15)
                        {
                            Debug.Log("Drop possible: " + lootOption + " || Loot Container: " + lootContainer);
                            inventorySlotCount++;
                            lootContainer.Add(lootOption);

                            break;
                        }
                        break;
                    }
                case Item.ItemRarity.MasterCraft:
                    {
                        dropChance = Random.Range(0, 100);
                        if (dropChance < 5)
                        {
                            Debug.Log("Drop possible: " + lootOption + " || Loot Container: " + lootContainer);
                            inventorySlotCount++;
                            lootContainer.Add(lootOption);

                            break;
                        }
                        break;
                    }
                case Item.ItemRarity.Legendary:
                    {
                        dropChance = Random.Range(0, 100);
                        if (dropChance < 1)
                        {
                            Debug.Log("Drop possible: " + lootOption + " || Loot Container: " + lootContainer);
                            inventorySlotCount++;
                            lootContainer.Add(lootOption);

                            break;
                        }
                        break;
                    }
                case Item.ItemRarity.WhisperedOf:
                    {
                        dropChance = Random.Range(0, 1000);
                        if (dropChance < 1)
                        {
                            Debug.Log("Drop possible: " + lootOption + " || Loot Container: " + lootContainer);
                            inventorySlotCount++;
                            lootContainer.Add(lootOption);

                            break;
                        }
                        break;
                    }
            }
        }
    }
    void IdentifyInventoryTarget()
    {

    }
}
