using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Pathfinding;

/// <summary>
/// This is the ticket. 
/// Everything else works, but this needs to be rewritten to utilize A*
/// 
/// This script still needs some more work for conversion. It's close, but it's still not detecting end point, and notifying the AI
/// to perform needs work. 
/// ->Pathfinding may also still be in world space calculation
/// 
/// ToDo:
/// Need to figure out jobs and commands logic
/// Need to figure out the best way to integrate that with GOAP needs
/// Need to wipe and or clean out previous code to help with project bloat
/// </summary>
/// 
//[RequireComponent(typeof(NavMeshAgent))]
public class Navigation_Pathfinder : BaseNavigation
{
    RichAI LinkedPathfinder;

    protected override void Initialise()
    {
        LinkedPathfinder = GetComponent<RichAI>();
    }

    protected override bool RequestPath()
    {
        LinkedPathfinder.maxSpeed = MaxMoveSpeed;
        LinkedPathfinder.rotationSpeed = RotationSpeed;
        LinkedPathfinder.endReachedDistance = DestinationReachedThreshold;
        LinkedPathfinder.destination = Destination;
        OnBeganPathFinding();

        return true;
    }

    //This is working now. See Base Navigation, under lookforpathonship variable. 
    //The Estate is never swapped from idle, and thats causing the pathfinding stop issue
    protected override void RequestShipPath()
    {
        LinkedPathfinder.destination = Destination;
        //Debug.Log("Destination found, navigating to: " + Destination);  //It knows theres a new destination, but isn't trying to unlock it or update it
        if (LinkedPathfinder.reachedEndOfPath)
        {
            Debug.Log("Destination reached, stopping");
            StopMovement();
            OnReachedDestination();
        }
        else
        {
            Debug.Log("Destination not reached, navigating");
            LinkedPathfinder.isStopped = false;
        }
    }
    protected override void Tick_Default()
    {

    }

    protected override void Tick_Pathfinding()
    {
        if (!LinkedPathfinder.pathPending)
        {           
            if (LinkedPathfinder.hasPath) //NavMeshPathStatus.PathComplete)
                OnPathFound();
            else
                OnFailedToFindPath();
        }
    }

    protected override void Tick_PathFollowing()
    {
        bool atDestination = false;
        // do we have a path and we near the destination?
        if (!LinkedPathfinder.reachedEndOfPath && LinkedPathfinder.remainingDistance <= LinkedPathfinder.endReachedDistance)
        {
            Debug.Log("Condition near end of path reached");
            atDestination = true;
        }
        else if (LinkedPathfinder.reachedEndOfPath == false)
        {
            Vector3 vecToDestination = Destination - transform.position;
            float heightDelta = Mathf.Abs(vecToDestination.y);
            vecToDestination.y = 0f;

            atDestination = heightDelta < LinkedPathfinder.height && 
                            vecToDestination.magnitude <= DestinationReachedThreshold;
        }
        else if (LinkedPathfinder.reachedEndOfPath == true)
        {
            Debug.Log("At destination");

            atDestination = true;
        }

        if (atDestination) 
        {
            OnReachedDestination();
        }
        else
        {
            if (DEBUG_ShowHeading)
                Debug.DrawLine(transform.position + Vector3.up, LinkedPathfinder.steeringTarget, Color.green);
        }
    }

    protected override void Tick_Animation()
    {
        float forwardsSpeed = Vector3.Dot(LinkedPathfinder.velocity, transform.forward) / LinkedPathfinder.slowdownTime;
        float sidewaysSpeed = Vector3.Dot(LinkedPathfinder.velocity, transform.right) / LinkedPathfinder.slowdownTime;

        AnimController.SetFloat("ForwardsSpeed", forwardsSpeed);
        AnimController.SetFloat("SidewaysSpeed", sidewaysSpeed);
    }

    public override void StopMovement()
    {
        LinkedPathfinder.isStopped = true;
    }

    public override bool FindNearestPoint(Vector3 searchPos, float range, out Vector3 foundPos)
    {
        NavMeshHit hitResult;
        if (NavMesh.SamplePosition(searchPos, out hitResult, range, NavMesh.AllAreas))
        {
            foundPos = hitResult.position;
            return true;
        }

        foundPos = searchPos;

        return false;
    }


}
