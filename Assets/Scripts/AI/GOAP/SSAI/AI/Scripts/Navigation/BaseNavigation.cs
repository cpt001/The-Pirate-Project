using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseNavigation : MonoBehaviour
{
    public enum EState
    {
        Idle                = 0,
        FindingPath         = 1,
        FollowingPath       = 2,
        FollowingShipPath   = 3,

        Failed_NoPathExists = 100
    }

    [Header("Path Following")]
    [SerializeField] protected float DestinationReachedThreshold = 0.25f;
    [SerializeField] protected float MaxMoveSpeed = 5f;
    [SerializeField] protected float RotationSpeed = 120f;

    [Header("Animation")]
    [SerializeField] protected Animator AnimController;

    [Header("Debug Tools")]
    [SerializeField] protected bool DEBUG_UseMoveTarget;
    [SerializeField] protected Transform DEBUG_MoveTarget;
    [SerializeField] protected bool DEBUG_ShowHeading;

    private bool LookForPathOnShip = false;

    public Vector3 Destination { get; private set; }
    public EState State { get; private set; } = EState.Idle;

    public bool IsFindingOrFollowingPath => State == EState.FindingPath || State == EState.FollowingPath;
    
    //This isn't firing right now
    public bool IsAtDestination
    {
        get
        {
            if (State != EState.Idle)
            {
                Debug.Log("Not idling, not at destination");    //So it's never exiting the non-idle condition
                return false;
            }
            Vector3 vecToDestination = Destination - transform.position;
            vecToDestination.y = 0f;

            //Returns true if below threshold
            return vecToDestination.magnitude <= DestinationReachedThreshold;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG_UseMoveTarget)
            SetDestination(DEBUG_MoveTarget.position, false);

        if (State == EState.FindingPath)
            Tick_Pathfinding();

        Tick_Default();

        if (AnimController != null)
            Tick_Animation();

        //This bool is never set to true again
        if (LookForPathOnShip == true)
        {
            ShipPathing();
            Debug.Log("Looking for ship path");
        }
    }

    void FixedUpdate()
    {
        Debug.Log("Estate: " + State.ToString());
        if (State == EState.FollowingPath)
            Tick_PathFollowing();
    }

    //This needs to check whether the destination is on a ship or on land. 
    //If the destination is on a ship, but the unit is on land...?
    public bool SetDestination(Vector3 newDestination, bool destinationIsOnShip)
    {
        LookForPathOnShip = destinationIsOnShip;
        if (destinationIsOnShip)
        {
            //This condition is never fired?
            Debug.Log("Destination on ship");
            return true;    //This definitely needs to return true, or the destination isn't actually set
        }
        else
        {
            Debug.Log("Destination not on ship");
            // location is already our destination?
            Vector3 destinationDelta = newDestination - Destination;
            destinationDelta.y = 0f;
            if (IsFindingOrFollowingPath && (destinationDelta.magnitude <= DestinationReachedThreshold))
                return true;

            // are we already near the destination
            destinationDelta = newDestination - transform.position;
            destinationDelta.y = 0f;
            if (destinationDelta.magnitude <= DestinationReachedThreshold)
                return true;

            Destination = newDestination;

            return RequestPath();
        }
    }

    //Called by NSSAI   -- I think the issue is with this function
    Transform localObjectToTrack;
    public void SetShipPathing(Transform objectTarget)
    {
        State = EState.FollowingShipPath;
        localObjectToTrack = objectTarget;
        LookForPathOnShip = true;
        //Check for arrival at destination/end of path
    }
    void ShipPathing()
    {
        Debug.Log("Firing shippathing");
        Destination = localObjectToTrack.position;
        RequestShipPath();
    }


    public abstract void StopMovement();

    public abstract bool FindNearestPoint(Vector3 searchPos, float range, out Vector3 foundPos);

    protected abstract void Initialise();

    protected abstract bool RequestPath();
    protected abstract void RequestShipPath();

    protected virtual void OnBeganPathFinding()
    {
        State = EState.FindingPath;
    }

    protected virtual void OnPathFound()
    {
        State = EState.FollowingPath;
    }

    protected virtual void OnFailedToFindPath()
    {
        State = EState.Failed_NoPathExists;
    }
    //Need to figure out how to stop destination pathfinding. Null out destination?
    protected virtual void OnReachedDestination()
    {
        //Without this bool, the state is fired infinitely. 
        //
        State = EState.Idle;
        if (LookForPathOnShip == true)
        {
            LookForPathOnShip = false;
        }
    }

    protected abstract void Tick_Default();
    protected abstract void Tick_Pathfinding();
    protected abstract void Tick_PathFollowing();
    protected abstract void Tick_Animation();
}
