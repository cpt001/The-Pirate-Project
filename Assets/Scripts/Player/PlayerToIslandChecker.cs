using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerToIslandChecker : MonoBehaviour
{
    [SerializeField] private Crest.OceanRenderer oceanObject;

    [SerializeField] private Transform closestIsland = null;   //Replace with island director script later

    private float oceanStrengthMinValue = 16.5f;
    private float oceanStrengthMaxValue = 120.0f;
    private float playerDistanceFromIsland;
    private float ignoreRadius; //Anything outside this radius is ignored logic wise

    private List<Transform> islandList = new List<Transform>();

    private void Awake()
    {
        foreach (Transform t in transform.root)
        {
            if (t.GetComponent<IslandController>())
            {
                islandList.Add(t);
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        foreach (Transform island in islandList)
        {
            if (closestIsland)
            {
                Debug.Log(island.name);

                if (Vector3.Distance(island.position, closestIsland.position) < Vector3.Distance(transform.position, closestIsland.position))   //If the island is closer
                {
                    closestIsland = island;
                }
            }
            else
            {
                Debug.Log("No island found");
            }

        }
        if (closestIsland != null)
        {
            if (Vector3.Distance(transform.position, closestIsland.position) < ignoreRadius)
            {
                playerDistanceFromIsland = Vector3.Distance(transform.position, closestIsland.position);
                oceanObject._globalWindSpeed = Mathf.Lerp(oceanStrengthMinValue, oceanStrengthMaxValue, playerDistanceFromIsland);
            }
        }
    }
}
