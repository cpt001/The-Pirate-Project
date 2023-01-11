using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public string pawnName;
    [SerializeField] private Transform currentDestination;
    [SerializeField] private string taskPerforming; //Collect, transport, work, sleep
    [SerializeField] private string jobName;

    private WaitForSeconds collectionWaitTime = new WaitForSeconds(3.0f);

    private void Awake()
    {
        
    }

    private IEnumerator CollectingItem()
    {
        yield return collectionWaitTime;
    }
}
