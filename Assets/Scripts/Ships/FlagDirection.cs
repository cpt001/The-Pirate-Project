using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagDirection : MonoBehaviour
{
    [SerializeField] private Transform windObject => GameObject.FindWithTag("WindObject").transform;


    void FixedUpdate()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, windObject.transform.rotation, Time.deltaTime);
    }
}
