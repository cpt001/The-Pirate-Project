using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// This script is designed to handle player to world interactions
/// </summary>
public class RowboatInteraction : MonoBehaviour
{
    [SerializeField] private ControllableShip homeShip => transform.root.GetComponent<ControllableShip>();
    private TextMeshProUGUI interactionText;
    private bool playerCanInteract;
    private bool canStow;
    private Transform interactor;
    private bool deployingLeft;
    [SerializeField] private Transform port1, port2, port3;
    [SerializeField] private Transform sb1, sb2, sb3;

    private BoatAlignNormal boatPhysics;

    private enum DeploymentState
    {
        Stored,
        Hanging,
        Dangling,
        Moored,
        Deployed,
        Ashore,
    }
    private DeploymentState deployState = DeploymentState.Stored;
    private Rigidbody _rb;

    private void Start()
    {
        RowboatUndeploy();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (deployState == DeploymentState.Stored || deployState == DeploymentState.Dangling)
            {
                playerCanInteract = true;
            }
            if (deployState == DeploymentState.Deployed)
            {
                //Handle control
            }
            interactionText = GameObject.FindWithTag("InteractionText").GetComponent<TextMeshProUGUI>();
            interactor = other.gameObject.transform;
        }
        if (other.GetComponent<ControllableShip>() == homeShip)
        {
            if (deployState == DeploymentState.Deployed)
            {
                canStow = true;
                interactor = null;
            }
        }
        if (other.CompareTag("Terrain"))
        {
            deployState = DeploymentState.Ashore;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCanInteract = false;
            interactionText.text = "";
        }
        if (other.GetComponent<ControllableShip>() == homeShip)
        {
            if (deployState == DeploymentState.Moored)
            {
                deployState = DeploymentState.Deployed;
            }
            canStow = false;
        }
    }

    private void Update()
    {
        if (playerCanInteract)
        {
            //Player interacts with deployed rowboat to assume command
            if (deployState == DeploymentState.Deployed && !canStow)
            {
                interactionText.text = "Press X to board rowboat";
                if (Input.GetKeyDown(KeyCode.X))
                {
                    boatPhysics._playerControlled = true;
                }
            }
            //Player interacts with deployed rowboat to stow on ship
            else if(deployState == DeploymentState.Deployed && canStow)
            {
                interactionText.text = "Press X to stow rowboat on " + homeShip.name;
                if (Input.GetKeyDown(KeyCode.X))
                {
                    RowboatUndeploy();
                }
            }
            //State 3: Player interacts with deployed rowboat stuck on shore (push) -- TBD
            else if (deployState == DeploymentState.Ashore)
            {
                interactionText.text = "Press X to shove rowboat -- NYI";
            }
            //State 4: Player interacts with undeployed rowboat
            else if (deployState == DeploymentState.Stored)
            {
                interactionText.text = "Press X to deploy rowboat into the sea";
                if (Input.GetKeyDown(KeyCode.X))
                {
                    RowboatDeploy();
                }
            }


            if (deployState == DeploymentState.Dangling)
            {
                interactionText.text = "Press X to board rowboat";
                if (Input.GetKeyDown(KeyCode.X))
                {
                    StartCoroutine(DeployToOcean());
                }
            }
        }
        else
        {
            if (interactionText)
            {
                interactionText.text = "";
            }
        }
    }

    void RowboatDeploy()
    {
        DeployRowboatAnimation();
    }

    void DeployRowboatAnimation()
    {
        //Get player direction, local to ship
        if (interactor != null)
        {
            //Debug.Log("DOT: " + Vector3.Dot(transform.forward, interactor.forward));
            if (Vector3.Dot(transform.forward, interactor.forward) > .5f && Vector3.Dot(transform.forward, interactor.forward) > -.5f)    //Less 90, greater -90
            {
                deployingLeft = true;
            }
            else
            {
                deployingLeft = false;
            }
        }
        //Send rowboat in player direction, through animations 1 and 2
        StartCoroutine(SetAnimationSequence());
        //When player boards, send to animation 3, and activate rigidbody
    }

    void RowboatUndeploy()
    {
        //Debug.Log("Undeploy Called");
        if (_rb)
        {
            Destroy(_rb);
        }
        gameObject.GetComponent<BoatAlignNormal>().enabled = false;
    }

    //Is this needed?
    /*private IEnumerator DeploymentProtectionTimer()
    {
        deploymentProtection = true;
        yield return new WaitForSeconds(5.0f);
        deploymentProtection = false;
    }*/

    IEnumerator SetAnimationSequence()
    {
        playerCanInteract = false;
        if (deployState == DeploymentState.Stored)
        {
            if (deployingLeft)
            {
                StartCoroutine(LerpPosition(port1.position, port1.rotation, 7f));
            }
            else
            {
                StartCoroutine(LerpPosition(sb1.position, sb1.rotation, 7f));
            }
            deployState = DeploymentState.Hanging;
            yield return new WaitForSeconds(7.3f);
            if (deployingLeft)
            {
                StartCoroutine(LerpPosition(port2.position, port2.rotation, 7f));
            }
            else
            {
                StartCoroutine(LerpPosition(sb2.position, sb2.rotation, 7f));
            }
            deployState = DeploymentState.Dangling;
            yield return new WaitForSeconds(2.0f);
            playerCanInteract = true;
        }
    }
    IEnumerator DeployToOcean()
    {
        if (deployState == DeploymentState.Dangling && deployingLeft)
        {
            StartCoroutine(LerpPosition(port3.position, port3.rotation, 5.0f));
            yield return new WaitForSeconds(5.0f);

            gameObject.AddComponent<Rigidbody>();
            _rb = gameObject.GetComponent<Rigidbody>();
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.mass = 200f;
            _rb.angularDrag = 1.5f;
            gameObject.GetComponent<BoatAlignNormal>().enabled = true;
            gameObject.layer = 8;
        }
        if (deployState == DeploymentState.Dangling && !deployingLeft)
        {
            StartCoroutine(LerpPosition(sb3.position, sb3.rotation, 5.0f));
            yield return new WaitForSeconds(5.0f);

            gameObject.AddComponent<Rigidbody>();
            _rb = gameObject.GetComponent<Rigidbody>();
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.mass = 200f;
            _rb.angularDrag = 1.5f;
            gameObject.GetComponent<BoatAlignNormal>().enabled = true;
            gameObject.layer = 8;
        }
        canStow = false;
        deployState = DeploymentState.Moored;
        yield return null;
    }

    //For use with animator
    IEnumerator LerpPosition(Vector3 targetPosition, Quaternion targetRotation, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;
        Quaternion startQuat = transform.rotation;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            transform.rotation = Quaternion.Lerp(startQuat, targetRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }
}
