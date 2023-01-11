using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item itemInSlot = null;
    public int numberOfItem = 0;
    [SerializeField] private Image itemImage = null;
    [SerializeField] private TextMeshProUGUI textElement = null;
    public InventoryManager inventoryTarget;
    public int slotStatus;
    private enum SellBehavior
    {
        Protected,
        Normal,
        Junk,
        end,
    }
    private SellBehavior itemSellBehavior;
    public enum ItemTypeRestrictor
    {
        Unrestricted,
        Weapon,
        Consumable,
        Ammunition,
        Clothing,
        OneWaySlot, //Allows for loot only inventories
    }
    public ItemTypeRestrictor itemRestrictionsOnSlot;

    private void Awake()
    {
        if (!inventoryTarget)
        {
            Debug.LogError(name + " needs the inventory defined!");
        }
        foreach (Transform t in transform)
        {
            if (t.GetComponent<Image>())
            {
                itemImage = t.GetComponent<Image>();
            }
            if (t.GetComponent<TextMeshProUGUI>())
            {
                textElement = t.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void LateUpdate()
    {
        if (itemInSlot != null)
        {
            if (numberOfItem < itemInSlot.maxStack)
            {
                numberOfItem = itemInSlot.maxStack;
            }
            itemImage.sprite = itemInSlot.itemSprite;
            textElement.text = itemInSlot.name;
        }
        else
        {
            textElement.text = null;
            itemImage.sprite = null;
        }

        if (slotStatus != (int)itemRestrictionsOnSlot && itemInSlot != null)
        {
            itemSellBehavior = (SellBehavior)slotStatus;
            if (slotStatus == (int)SellBehavior.end)
            {
                slotStatus = 0;
            }
        }
        else if (itemInSlot == null)
        {
            itemSellBehavior = SellBehavior.Normal;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Pointer entered " + name + ", setting target");
        inventoryTarget.targetSlot = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Pointer exiting " + name + ", nulling target");
        inventoryTarget.targetSlot = null;
    }
}
