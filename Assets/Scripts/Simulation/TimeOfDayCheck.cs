using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeOfDayCheck : MonoBehaviour
{
    private bool isDaytime = false;
    private List<LightFlicker> occupants = new List<LightFlicker>();
    //Creates listener for toggleLights event
    void Start()
    {
        EventsManager.StartListening("ToggleLights", ToggleAutoLights);
    }

    //Tracks exterior lighting conditions here
    void ToggleAutoLights()
    {
        isDaytime = !isDaytime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<LightFlicker>())
        {
            //Debug.Log("Light detected");
            LightFlicker tempItem = other.GetComponent<LightFlicker>();
            if (occupants.Contains(tempItem))   //If the object is in the list, it's exiting
            {
                if (isDaytime)
                {
                    //Toggle light off
                    tempItem.lanternOverride = false;
                    tempItem.SetLights(false);
                }
                else
                {
                    //Keep light on
                    tempItem.lanternOverride = false;
                }
                occupants.Remove(tempItem);
            }
            //If the object isn't found, it's entering
            else
            {
                occupants.Add(tempItem);
                tempItem.lanternOverride = true;
                tempItem.SetLights(true);
            }
        }
    }
}
