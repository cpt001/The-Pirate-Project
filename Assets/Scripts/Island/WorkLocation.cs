using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WorkLocation : MonoBehaviour
{
    private string title;
    private Structure workSite;
    public bool isBeingWorked;
    [SerializeField] private int workTime;
    [SerializeField] private CargoSO.CargoType itemProduced;
    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;
        if (GetComponentInParent<Structure>())
        {
            workSite = GetComponentInParent<Structure>();
        }
        //title = GetComponentInParent<Transform>().name + "_"
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PawnGeneration>())
        {
            isBeingWorked = true;
            other.GetComponent<PawnGeneration>().timeToWait = workTime;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PawnGeneration>())
        {
            isBeingWorked = false;
        }
    }
}
