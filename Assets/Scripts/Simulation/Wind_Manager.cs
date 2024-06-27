using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Manager : MonoBehaviour
{
    private int windRot;
    private Vector3 windDirection;
    private int windTime;   //How long until the wind shifts in hours
    public int windSpeed;
    private int timer;      //Internal hour counter, adds with new hour call
    private float secondTimer;
    private bool firstStartup = true;



    // Start is called before the first frame update
    void Start()
    {
        EventsManager.StartListening("NewHour", WindHourCheck);
        RandomizeWindValues();
    }

    //Generates new values for the wind to adhere to
    void RandomizeWindValues()
    {
        secondTimer = Random.Range(40, 360);
        windRot = Random.Range(0, 359);
        windTime = Random.Range(2, 120);    //120 is still 5 days
        windSpeed = Random.Range(0, 30);

        //Debug.Log("Randomized wind values! Seconds: " + secondTimer + " | WindRot: " + windRot + " | WindTime: " + windTime + " | WindSpeed: " + windSpeed);
        windDirection = new Vector3(0, windRot);
        if (firstStartup)
        {
            StartCoroutine(LerpRotation(Quaternion.Euler(windDirection), 0));
            firstStartup = false;
        }
        else
        {
            StartCoroutine(LerpRotation(Quaternion.Euler(windDirection), secondTimer));
        }
    }
    
    //Checks whether the wind can be adjusted again
    void WindHourCheck()
    {
        //Debug.Log("Wind triggered");
        timer += 1;
        if (timer >= windTime)  //Problem with timer
        {
            windTime = 0;
            windDirection = new Vector3(0, windRot, 0);
            RandomizeWindValues();
        }
    }

    //Rotates the wind to the correct position
    IEnumerator LerpRotation (Quaternion endvalue, float duration)
    {
        //Debug.Log("Lerping Rotation");
        float time = 0;
        Quaternion startvalue = transform.rotation;
        while (time < duration)
        {
            transform.rotation = Quaternion.Lerp(startvalue, endvalue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endvalue;
    }
}
