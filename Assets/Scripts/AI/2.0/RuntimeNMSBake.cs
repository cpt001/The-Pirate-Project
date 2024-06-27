using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;
/// <summary>
/// This currently works, but the navmesh seems off
/// Navmesh is definitely off. The movement is desynced from the movement of the ship. 
/// </summary>
public class RuntimeNMSBake : MonoBehaviour
{
    private NavMeshSurface navSurface => GetComponent<NavMeshSurface>();
    private Transform player;
    private float updateRate;
    private Vector3 navMeshSize =  new Vector3(20, 20, 20);

    [SerializeField]
    private NavMeshData navData;
    private List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();

    private void Awake()
    {
        //navSurface.BuildNavMesh();
        //navData = GetComponent<NavMeshSurface>().navMeshData;
        //navData = new NavMeshData();
        //NavMesh.AddNavMeshData(NavMeshData);
        //BuildNavMesh(false);
    }

    void Update()
    {
        //UnityEditor.AI.NavMeshBuilder.BuildNavMeshAsync();
        if (!navData)
        {
            navSurface.BuildNavMesh();
            navData = GetComponent<NavMeshSurface>().navMeshData;
        }
        else
        {
            navSurface.UpdateNavMesh(navData);
        }
        //navSurface.BuildNavMesh();
    }

    /*private void BuildNavMesh(bool async)
    {
        Bounds navMeshBounds = new Bounds(player.transform.position, navMeshSize);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

        List<NavMeshModifier> modifiers;
        if (navSurface.collectObjects == CollectObjects.Children)
        {
            modifiers = new List<NavMeshModifier>(navSurface.GetComponentsInChildren<NavMeshModifier>());
        }
        else
        {
            modifiers = NavMeshModifier.activeModifiers;
        }
    }*/

    /*public static AsyncOperation UpdateNavMeshDataAsync(NavMeshSurface navData)
    {
        return navData;
    }*/
}
