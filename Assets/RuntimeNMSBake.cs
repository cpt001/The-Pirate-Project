using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
public class RuntimeNMSBake : MonoBehaviour
{
    private NavMeshSurface navSurface => GetComponent<NavMeshSurface>();
    void Update()
    {
        navSurface.BuildNavMesh();
    }
}
