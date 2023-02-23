using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField]
    private KeyCode disarm = KeyCode.Tilde;
    [SerializeField]
    private KeyCode aimDownSight = KeyCode.Z;
    //Tapping these buttons will draw the weapon. Scrolling while drawn will swap between types of weapons in this category. Tapping again will put the weapon away. Tap and hold to draw the weapon in addition for duel wielding.
    [SerializeField]
    private KeyCode drawMelee = KeyCode.Alpha1; //Sword, dagger, hatchet
    [SerializeField]
    private KeyCode drawFirearm = KeyCode.Alpha2;   //Pistol, Musket, Blunderbuss, Grenade
    [SerializeField]
    private KeyCode drawVoodoo = KeyCode.Alpha3;    //Doll, Staff
    [SerializeField]
    private KeyCode reloadFirearm = KeyCode.R;
    [SerializeField]
    private KeyCode throwMelee = KeyCode.H;
    [SerializeField]
    private KeyCode toggleLantern = KeyCode.L;
    [SerializeField]
    private GameObject lantern;
    private bool lanternStatus;
    private bool hasMeleeEquipped;
    private bool hasFirearmEquipped;
    private bool hasVoodooEquipped;

    [SerializeField]
    private bool toggleADS;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update mostly handles direct input
    void Update()
    {
        if (Input.GetKeyDown(toggleLantern))
        {
            lanternStatus = !lanternStatus;
            lantern.SetActive(lanternStatus);
        }
        if (Input.GetKeyDown(disarm))
        {
            if (hasMeleeEquipped | hasFirearmEquipped | hasVoodooEquipped)
            {
                hasMeleeEquipped = false;
                hasFirearmEquipped = false;
                hasVoodooEquipped = false;
            }
        }
        if (!hasMeleeEquipped && !hasFirearmEquipped && !hasVoodooEquipped)
        {
            Fisticuffs();
        }
        if (Input.GetKeyDown(drawMelee))
        {
            hasMeleeEquipped = !hasMeleeEquipped;
        }
        if (hasMeleeEquipped)
        {
            MeleeOperation();
        }
        if (Input.GetKeyDown(drawFirearm))
        {
            hasFirearmEquipped = !hasFirearmEquipped;
        }
        if (hasFirearmEquipped)
        {
            FirearmOperation();
        }
        if (Input.GetKeyDown(drawVoodoo))
        {
            hasVoodooEquipped = !hasVoodooEquipped;
        }
        if (hasVoodooEquipped)
        {
            VoodooOperation();
        }
    }

    void Fisticuffs()
    {
        //Simple punch animation cycle
        if (Input.GetMouseButtonDown(0))
        {
            //Create a gameobject that can act as a small spherecast, damaging anything in its radius
        }

    }

    void MeleeOperation()
    {
        //Draw weapon animation
        //Swing
        //ThrowWeapon
    }
    void FirearmOperation()
    {
        //Draw weapon animation
        //Quickfire
        if (Input.GetMouseButtonDown(0))
        {
            Debug.DrawRay(transform.position, Vector3.forward, Color.red, 2.0f, true);
            RaycastHit shotHit;
            if (Physics.Raycast(transform.position, Vector3.forward, out shotHit, 300.0f))
            {
                Debug.Log("Shot hit: " + shotHit.transform.name);
            }
        }
        //ADS
        if (Input.GetKey(aimDownSight) && !toggleADS)
        {

        }
        if (Input.GetKeyDown(aimDownSight) && toggleADS)
        {

        }
        //Reload
    }
    void VoodooOperation()
    {
        //Draw weapon animation
        //Select target
    }
}
