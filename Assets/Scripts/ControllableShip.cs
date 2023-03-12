using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Bottom H can be manipulated to create a sinking effect.
/// -If the ship is actively sinking, the forward speed can be slowed * sink level
/// -if it's below the main deck, the ship is sunk. Movement speed can be set to 0, or very slow.
/// </summary>
public class ControllableShip : MonoBehaviour
{
    [SerializeField] private Crest.BoatProbes boatTarget;
    [SerializeField] private StarterAssets.ThirdPersonController playerController;

    [SerializeField] private bool anchorDropped;
    private Rigidbody _rb;
    [SerializeField] private Transform bowSprit;

    [Header("Sail Schematic")]
    [SerializeField] private List<GameObject> DeadSlowSail;
    [SerializeField] private List<GameObject> BattleSail;
    [SerializeField] private List<GameObject> SlowSail;
    [SerializeField] private List<GameObject> HalfSail;
    [SerializeField] private List<GameObject> FullSail;

    [Header("Broadside Setup")]
    [SerializeField] private List<Cannon> portBroadside;
    [SerializeField] private List<Cannon> starboardBroadside;
    [SerializeField] private List<Cannon> bowCannons;
    [SerializeField] private List<Cannon> sternCannons;
    [SerializeField] private enum FireMode
    {
        ForeToStern,
        SternToFore,
        Random,
        reset,
    }
    private FireMode firingOrder;

    private int setSail;

    private enum SailState
    {
        NoSail,
        //RowboatTug,
        DeadSlow,
        Battle,
        Slow,
        Half,
        Full,
    }
    [SerializeField] private SailState sailState;

    private bool playerControllingShip;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        foreach (Transform t in transform.Find("BroadsidePort"))
        {
            if (t.gameObject.GetComponent<Cannon>())
            {
                portBroadside.Add(t.GetComponent<Cannon>());
            }
        }
        foreach (Transform t in transform.Find("BroadsideStarboard"))
        {
            if (t.gameObject.GetComponent<Cannon>())
            {
                starboardBroadside.Add(t.GetComponent<Cannon>());
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            playerControllingShip = playerControllingShip ? false : true;
        }

        if (playerControllingShip)
        {
            //Debug.Log("Player is controlling ship");
            playerController.MoveSpeed = 0;
            playerController.JumpHeight = 0;
            boatTarget._playerControlled = true;

            if (anchorDropped)
            {
                boatTarget._engineBias = 0;
                _rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }

            if (Input.GetKeyDown(KeyCode.W) && sailState != SailState.Full)
            {
                Debug.Log("Sails up! " + sailState);
                setSail++;
                sailState = (SailState)setSail;
            }
            if (Input.GetKeyDown(KeyCode.S) && sailState != SailState.NoSail)
            {
                Debug.Log("Sails down!");
                setSail--;
                sailState = (SailState)setSail;

            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                EventsManager.TriggerEvent("Broadside_Port_" + gameObject.transform.name);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                EventsManager.TriggerEvent("Broadside_Starboard_" + gameObject.transform.name);
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                firingOrder++;
                UpdateFiringMode();
                if (firingOrder == FireMode.reset)
                {
                    firingOrder = FireMode.ForeToStern;
                }
            }
        }
        else
        {
            playerController.MoveSpeed = 2.0f;
            playerController.JumpHeight = 2.0f;
            boatTarget._playerControlled = false;
        }

        ShipSailCaseUpdate();
    }

    void UpdateFiringMode()
    {
        //https://forum.unity.com/threads/sorting-a-list-of-vector3-by-x-values.126237/
        //IEnumerable<Vector3> sorted = portBroadside.OrderBy<>
        //smods.OrderBy(bastard => Vector3.Distance(transform.position, sm.transform.position));
        switch (firingOrder)
        {
            case FireMode.ForeToStern:
                {
                    portBroadside = portBroadside.OrderBy((d) => (d.transform.position - bowSprit.position).sqrMagnitude).ToList();
                    starboardBroadside = starboardBroadside.OrderBy((d) => (d.transform.position - bowSprit.position).sqrMagnitude).ToList();
                    break;
                }
            case FireMode.SternToFore:
                {
                    portBroadside = portBroadside.OrderBy((d) => (d.transform.position + bowSprit.position).sqrMagnitude).ToList();
                    starboardBroadside = starboardBroadside.OrderBy((d) => (d.transform.position + bowSprit.position).sqrMagnitude).ToList();
                    break;
                }
            case FireMode.Random:
                {
                    for (int i = 0; i < portBroadside.Count - 1; i++)
                    {
                        int rand = Random.Range(i, portBroadside.Count);
                        Cannon tempCannon = portBroadside[rand];
                        portBroadside[rand] = portBroadside[i];
                        portBroadside[i] = tempCannon;
                    }                    
                    for (int i = 0; i < starboardBroadside.Count - 1; i++)
                    {
                        int rand = Random.Range(i, starboardBroadside.Count);
                        Cannon tempCannon = starboardBroadside[rand];
                        starboardBroadside[rand] = starboardBroadside[i];
                        starboardBroadside[i] = tempCannon;
                    }
                    break;
                }
        }
    }

    void ShipSailCaseUpdate()
    {
        switch(sailState)
        {
            #region No Sails
            case SailState.NoSail:
            {
                    boatTarget._engineBias = 0;

                foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(false);
                    }
                    break;
            }
            #endregion
            #region Rowboat Tug (NYI)
            /*case SailState.RowboatTug:
            {
                    boatTarget._engineBias = 1;

                    foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(false);
                    }
                    break;
            }*/
            #endregion
            #region Dead Slow Sail
            case SailState.DeadSlow:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = .5f;
                    }

                    foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(true);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(false);
                    }
                    break;
                }
            #endregion            
            #region Battle Sails
            case SailState.Battle:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = .75f;
                    }

                    foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(false);
                    }

                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(true);
                    }
                    break;
                }
            #endregion            
            #region Slow Sail
            case SailState.Slow:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = 1f;
                    }

                    foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(false);
                    }

                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(true);
                    }
                    break;
                }
            #endregion            
            #region Half Sail
            case SailState.Half:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = 1.5f;
                    }

                    foreach (GameObject sail in DeadSlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in BattleSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in SlowSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(false);
                    }

                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(true);
                    }
                    break;
                }
            #endregion            
            #region Full Sail
            case SailState.Full:
                {
                    if (!anchorDropped)
                    {
                        boatTarget._engineBias = 2f;
                    }
                    foreach (GameObject sail in HalfSail)
                    {
                        sail.SetActive(false);
                    }
                    foreach (GameObject sail in FullSail)
                    {
                        sail.SetActive(true);
                    }
                    break;
                }
            #endregion
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with ship detected: " + collision.transform.name);
    }
}
