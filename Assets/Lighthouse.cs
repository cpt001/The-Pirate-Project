using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lighthouse : MonoBehaviour
{
    protected Transform cachedTransform;

    public Vector3 rotationEulers;
    public float speed;

    private Light lightComponent;
    private GameObject beam;
    private bool currentStatus = true;

    void Start()
    {
        cachedTransform = this.transform;
        EventsManager.StartListening("ToggleLights", LighthouseToggle);
        lightComponent = gameObject.GetComponent<Light>();
        beam = gameObject.GetComponentInChildren<Transform>().gameObject;
    }

    void Update()
    {
        cachedTransform.Rotate(rotationEulers * (speed * Time.deltaTime));
    }

    void LighthouseToggle()
    {
        //Debug.Log("Lighthouse toggling");
        currentStatus = !currentStatus;
        lightComponent.enabled = currentStatus;
        beam.SetActive(currentStatus);
    }
}
