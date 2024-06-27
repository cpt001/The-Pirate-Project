using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShipInteraction : MonoBehaviour
{
    private ControllableShip assignedShip;
    private TextMeshProUGUI interactionText;
    private bool playerCanInteract;

    [SerializeField] private enum ShipInteractionList
    {
        Wheel,
        Capstan,
        Door_NYI,
        Rigging_NYI,
        Blind_NYI,
        RopeAnchor_NYI,
    }
    [SerializeField] private ShipInteractionList interaction;

    private void Awake()
    {
        assignedShip = transform.root.GetComponent<ControllableShip>();
        interactionText = GameObject.FindGameObjectWithTag("InteractionText").GetComponent<TextMeshProUGUI>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCanInteract = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !assignedShip.playerControllingShip)
        {
            playerCanInteract = false;
            interactionText.text = "";
        }
        else
        {
            Debug.Log("Player is outside of trigger, but can still exit interaction");
        }
    }

    void ShipCommandToggle()
    {
        assignedShip.playerControllingShip = assignedShip.playerControllingShip ? false : true;
    }
    void ShipAnchorToggle()
    {
        assignedShip.anchorDropped = assignedShip.anchorDropped ? false : true;
    }

    private void Update()
    {
        if (playerCanInteract)
        {
            #region Command Execute
            if (Input.GetKeyDown(KeyCode.X))
            {
                switch (interaction)
                {
                    case ShipInteractionList.Wheel:
                        {
                            ShipCommandToggle();
                            break;
                        }
                    case ShipInteractionList.Capstan:
                        {
                            ShipAnchorToggle();
                            break;
                        }
                }
            }
            #endregion
            #region Text Swapper
            switch (interaction)
            {
                case ShipInteractionList.Wheel:
                    {
                        if (!assignedShip.playerControllingShip)
                        {
                            interactionText.text = "Press X to command " + transform.root.name;
                        }
                        else if (assignedShip.playerControllingShip)
                        {
                            interactionText.text = "Press x to relinquish command";
                        }
                        break;
                    }
                case ShipInteractionList.Capstan:
                    {
                        if (!assignedShip.anchorDropped)
                        {
                            interactionText.text = "Press X to drop anchor";
                        }
                        else
                        {
                            interactionText.text = "Press X to weigh anchor";
                        }

                        break;
                    }
            }
            #endregion
        }
    }
}
