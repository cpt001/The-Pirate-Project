using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
/// <summary>
/// Need to look at this: https://learn.unity.com/tutorial/runtime-navmesh-generation#5c7f8528edbc2a002053b496
/// -> Navmeshsurface doesn't seem to be an available class?
/// -> Fixed, reference location was changed, and link is invalid. 
/// 
/// Need to optimize this somehow
/// </summary>
public class RuntimeNavBake : MonoBehaviour
{
    public List<Transform> dynamicBakingObjects = new List<Transform>();

    void Update()
    {
        if (dynamicBakingObjects.Count != 0)
        {
            foreach (Transform nms in dynamicBakingObjects)
            {
                nms.GetComponent<NavMeshSurface>().BuildNavMesh();
            }
        }
        /*for (int i = 0; i < shipSurfacesToBake.Length; i++)
        {
            shipSurfacesToBake[i].BuildNavMesh();
        }*/
    }
}
