using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameMenuUIController : MonoBehaviour
{
    [SerializeField] private GameObject SideBarPanel;
    [SerializeField] private GameObject MapPanel = null;
    [SerializeField] private GameObject SkillsPanel = null;
    [SerializeField] private GameObject InventoryPanel;
    [SerializeField] private GameObject LedgerPanel = null ;
    [SerializeField] private GameObject SettingsPanel = null;
    [SerializeField] private GameObject WardrobePanel;
    private bool displayObject = false;
    private enum CurrentlyDisplayedMenu
    {
        no_Menu,
        Map,
        Skills,
        Inventory,
        Ledger,
        Settings,
        Wardrobe,
    }
    [SerializeField] private CurrentlyDisplayedMenu menuToDisplay;

    /*private CurrentlyDisplayedMenu displayedMenu      //Ill figure this out someday.
    {
        get { return menuToDisplay; }
        set
        {

        }
    }*/

    private void Start()
    {
        SideBarPanel.SetActive(false);
    }


    private void Update()
    {
        SwitchActiveDisplay();

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMainUIDisplay();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (menuToDisplay != CurrentlyDisplayedMenu.Map)
            {
                menuToDisplay = CurrentlyDisplayedMenu.Map;
            }
            else if (menuToDisplay == CurrentlyDisplayedMenu.Map)
            {
                menuToDisplay = CurrentlyDisplayedMenu.no_Menu;
            }
            ToggleMainUIDisplay();
            //Map
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (menuToDisplay != CurrentlyDisplayedMenu.Skills)
            {
                menuToDisplay = CurrentlyDisplayedMenu.Skills;
            }
            else if (menuToDisplay == CurrentlyDisplayedMenu.Skills)
            {
                menuToDisplay = CurrentlyDisplayedMenu.no_Menu;
            }
            ToggleMainUIDisplay();
            //Skills
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (menuToDisplay != CurrentlyDisplayedMenu.Inventory)
            {
                menuToDisplay = CurrentlyDisplayedMenu.Inventory;
            }
            else if (menuToDisplay == CurrentlyDisplayedMenu.Inventory)
            {
                menuToDisplay = CurrentlyDisplayedMenu.no_Menu;
            }
            ToggleMainUIDisplay();
            //Inventory
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (menuToDisplay != CurrentlyDisplayedMenu.Ledger)
            {
                menuToDisplay = CurrentlyDisplayedMenu.Ledger;
            }
            else if (menuToDisplay == CurrentlyDisplayedMenu.Ledger)
            {
                menuToDisplay = CurrentlyDisplayedMenu.no_Menu;
            }
            ToggleMainUIDisplay();
            //Ledger
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (menuToDisplay != CurrentlyDisplayedMenu.Settings)
            {
                menuToDisplay = CurrentlyDisplayedMenu.Settings;
            }
            else if (menuToDisplay == CurrentlyDisplayedMenu.Settings)
            {
                menuToDisplay = CurrentlyDisplayedMenu.no_Menu;
            }
            ToggleMainUIDisplay();
            //Settings
        }
        if (Input.GetKeyDown(KeyCode.Minus))    //This is a temporary keybind. Wardrobe should only be accessed from physical object in world
        {
            if (menuToDisplay != CurrentlyDisplayedMenu.Wardrobe)
            {
                menuToDisplay = CurrentlyDisplayedMenu.Wardrobe;
            }
            else if (menuToDisplay == CurrentlyDisplayedMenu.Wardrobe)
            {
                menuToDisplay = CurrentlyDisplayedMenu.no_Menu;
            }
            ToggleMainUIDisplay();
        }
    }

    public void ToggleMainUIDisplay()
    {
        displayObject = displayObject ? false : true;
        if (menuToDisplay != CurrentlyDisplayedMenu.no_Menu)
        {
            displayObject = true;
        }
        if (displayObject == false)
        {
            menuToDisplay = CurrentlyDisplayedMenu.no_Menu;
        }
        SideBarPanel.SetActive(displayObject);
    }

    private void SwitchActiveDisplay()
    {
        switch (menuToDisplay)
        {
            case CurrentlyDisplayedMenu.no_Menu:
                {
                    MapPanel.SetActive(false);
                    SkillsPanel.SetActive(false);
                    InventoryPanel.SetActive(false);
                    LedgerPanel.SetActive(false);
                    SettingsPanel.SetActive(false);
                    WardrobePanel.SetActive(false);
                    break;
                }
            case CurrentlyDisplayedMenu.Map:
                {
                    MapPanel.SetActive(true);
                    SkillsPanel.SetActive(false);
                    InventoryPanel.SetActive(false);
                    LedgerPanel.SetActive(false);
                    SettingsPanel.SetActive(false);
                    WardrobePanel.SetActive(false);
                    break;
                }
            case CurrentlyDisplayedMenu.Skills:
                {
                    MapPanel.SetActive(false);
                    SkillsPanel.SetActive(true);
                    InventoryPanel.SetActive(false);
                    LedgerPanel.SetActive(false);
                    SettingsPanel.SetActive(false);
                    WardrobePanel.SetActive(false);
                    break;
                }
            case CurrentlyDisplayedMenu.Inventory:
                {
                    MapPanel.SetActive(false);
                    SkillsPanel.SetActive(false);
                    InventoryPanel.SetActive(true);
                    LedgerPanel.SetActive(false);
                    SettingsPanel.SetActive(false);
                    WardrobePanel.SetActive(false);
                    break;
                }
            case CurrentlyDisplayedMenu.Ledger:
                {
                    MapPanel.SetActive(false);
                    SkillsPanel.SetActive(false);
                    InventoryPanel.SetActive(false);
                    LedgerPanel.SetActive(true);
                    SettingsPanel.SetActive(false);
                    WardrobePanel.SetActive(false);

                    break;
                }
            case CurrentlyDisplayedMenu.Settings:
                {
                    MapPanel.SetActive(false);
                    SkillsPanel.SetActive(false);
                    InventoryPanel.SetActive(false);
                    LedgerPanel.SetActive(false);
                    SettingsPanel.SetActive(true);
                    WardrobePanel.SetActive(false);
                    break;
                }
            case CurrentlyDisplayedMenu.Wardrobe:
                {
                    MapPanel.SetActive(false);
                    SkillsPanel.SetActive(false);
                    InventoryPanel.SetActive(false);
                    LedgerPanel.SetActive(false);
                    SettingsPanel.SetActive(false);
                    WardrobePanel.SetActive(true);
                    break;
                }
        }
    }
}
