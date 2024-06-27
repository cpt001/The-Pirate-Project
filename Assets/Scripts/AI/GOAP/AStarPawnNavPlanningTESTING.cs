using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script has been built temporarily to interact with the ship
public class AStarPawnNavPlanningTESTING : MonoBehaviour
{
    private enum PathingTo
    {
        Food,
        Navigation,
        Wheel,
        Capstan,
        Hammock,
        Locker,
    }
    private PathingTo pathingTo;
    [SerializeField] private Transform Food, Navigation, Wheel, Capstan, Hammock, Locker;
    private Transform currentNavTarget;
    Pathfinding.IAstarAI ai => GetComponent<Pathfinding.IAstarAI>();
    Navigation_Pathfinder navPath => GetComponent<Navigation_Pathfinder>();

    // Start is called before the first frame update
    void Start()
    {
        pathingTo = (PathingTo)Random.Range(0, 5);
        PathToNewObject();
    }

    private bool updateLock = false;
    // Update is called once per frame
    void Update()
    {
        if (currentNavTarget != null && ai != null)
        {
            ai.destination = currentNavTarget.position;
        }

        if (ai.reachedEndOfPath == true && updateLock == false)
        {
            updateLock = true;
            StartCoroutine(TimeToNextSeek());
        }
    }

    IEnumerator newDestinationCooldown()
    {
        yield return new WaitForSeconds(5.0f);
        pathingTo = (PathingTo)Random.Range(0, 5);
        PathToNewObject();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == currentNavTarget)
        {
            //Snap to target interaction location
            StartCoroutine(TimeToNextSeek());

        }
    }

    IEnumerator TimeToNextSeek()
    {
        ai.isStopped = true;
        yield return new WaitForSeconds(5.0f);
        pathingTo = (PathingTo)Random.Range(0, 5);
        ai.isStopped = false;
        PathToNewObject();
        updateLock = false;
    }

    void PathToNewObject()
    {
        switch (pathingTo)
        {
            case PathingTo.Food:
                {
                    currentNavTarget = Food;
                    break;
                }
            case PathingTo.Navigation:
                {
                    currentNavTarget = Navigation;
                    break;
                }
            case PathingTo.Wheel:
                {
                    currentNavTarget = Wheel;
                    break;
                }
            case PathingTo.Capstan:
                {
                    currentNavTarget = Capstan;
                    break;
                }
            case PathingTo.Hammock:
                {
                    currentNavTarget = Hammock;
                    break;
                }
            case PathingTo.Locker:
                {
                    currentNavTarget = Locker;
                    break;
                }

        }
    }
}
