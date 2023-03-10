using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Lantern needs to turn on and off dynamically based on trigger volume. 
/// Possible solutions:
/// -Single volume at entrance that toggles the lantern. would need to get time of day on exit for appropriate response.
/// -Single volume covering entire cave that holds lantern. still needs TOD reference on exit
/// </summary>
public class LightFlicker : MonoBehaviour
{
    [Tooltip("External light to flicker; you can leave this null if you attach script to a light")]
    [SerializeField] private new Light light;
    [SerializeField] private MeshRenderer meshRend;
    [Tooltip("Minimum random light intensity")]
    [SerializeField] private float minIntensity = 0f;
    [Tooltip("Maximum random light intensity")]
    [SerializeField] private float maxIntensity = 1f;
    [Tooltip("How much to smooth out the randomness; lower values = sparks, higher = lantern")]
    [Range(1, 75)]
    [SerializeField] private int smoothing = 5;
    private bool lanternBool;
    public bool lanternOverride;
    [SerializeField] private bool playerLantern = false;
    [SerializeField] private bool environmentLighting = false;


    // Continuous average calculation via FIFO queue
    // Saves us iterating every time we update, we just change by the delta
    Queue<float> smoothQueue;
    float lastSum = 0;


    /// <summary>
    /// Reset the randomness and start again. You usually don't need to call
    /// this, deactivating/reactivating is usually fine but if you want a strict
    /// restart you can do.
    /// </summary>
    public void Reset()
    {
        smoothQueue.Clear();
        lastSum = 0;
    }

    void Start()
    {
        EventsManager.StartListening("ToggleLights", ToggleLights);
        ToggleLightsImmediate();
        smoothQueue = new Queue<float>(smoothing);
        // External or internal light?
        if (light == null)
        {
            light = GetComponent<Light>();
            if (GetComponent<MeshRenderer>() && meshRend == null)
            {
                meshRend = GetComponent<MeshRenderer>();
            }
            if (!playerLantern)
            {
                light.enabled = false;
                if (meshRend)
                {
                    meshRend.enabled = false;
                }
            }
            else
            {
                light.enabled = true;
                if (meshRend)
                {
                    meshRend.enabled = true;
                }
            }
        }
    }

    void ToggleLights() 
    { 
        if (!lanternOverride)
        {
            StartCoroutine(ToggleLightsAt(60));
            EventsManager.StartListening("ToggleLights", ToggleLights);
        }
        if (environmentLighting)
        {
            StartCoroutine(ToggleLightsAt(5));
        }
        else if (lanternOverride)
        {
            EventsManager.StopListening("ToggleLights", ToggleLights);
            lanternBool = true;
            SetLights(lanternBool);
        }
    }
    void ToggleLightsImmediate()
    {
        StartCoroutine(ToggleLightsAt(0));
    }
    IEnumerator ToggleLightsAt(float waitTime)
    {
        float randomTimeToLights = Random.Range(0, waitTime);
        yield return new WaitForSeconds(randomTimeToLights);
        lanternBool = !lanternBool;
        SetLights(lanternBool);
    }

    public void SetLights(bool lightToggle)
    {
        if (meshRend)
        {
            meshRend.enabled = lightToggle;
        }
        light.enabled = lightToggle;
    }

    void Update()
    {
        if (light == null)
            return;

        // pop off an item if too big
        while (smoothQueue.Count >= smoothing)
        {
            lastSum -= smoothQueue.Dequeue();
        }

        // Generate random new item, calculate new average
        float newVal = Random.Range(minIntensity, maxIntensity);
        smoothQueue.Enqueue(newVal);
        lastSum += newVal;

        // Calculate new smoothed average
        light.intensity = lastSum / (float)smoothQueue.Count;
    }
}