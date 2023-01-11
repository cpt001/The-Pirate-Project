using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private float playTimeSeconds;
    private float playTimeMinutes;
    private float playTimeHours;
    private float playTimeDays;
    [SerializeField]
    private int level;
    [SerializeField]
    private float experience;

    private void FixedUpdate()
    {
        PlayTimeIterator();
    }

    public void PlayTimeIterator()
    {
        playTimeSeconds += Time.deltaTime;
        if (playTimeSeconds >= 60.0f)
        {
            playTimeSeconds = 0;
            playTimeMinutes++;
        }
        if (playTimeMinutes >= 60.0f)
        {
            playTimeMinutes = 0;
            playTimeHours++;
        }
        if (playTimeHours >= 60.0f)
        {
            playTimeHours = 0;
            playTimeDays++;
        }
    }
}
