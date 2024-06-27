using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This is somehow the most inefficient script in the project, including the ocean rendering
/// 
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
    private bool lanternBool = true;
    public bool lanternOverride;
    [SerializeField] private bool isPlayersLantern = false;
    [SerializeField] private bool handheldLantern = false;

    private int hour;


    // Continuous average calculation via FIFO queue
    // Saves us iterating every time we update, we just change by the delta
    Queue<float> smoothQueue;
    float lastSum = 0;

    void Start()
    {
        SetInitialLightStatus();
        SetEventTriggers();
        

        /*ToggleLightsImmediate();
*/
    }

    void SetInitialLightStatus()
    {
        smoothQueue = new Queue<float>(smoothing);
        // External or internal light?
        if (light == null)
        {
            light = GetComponent<Light>();
            if (GetComponent<MeshRenderer>() && meshRend == null)
            {
                meshRend = GetComponent<MeshRenderer>();
            }
            if (GameObject.Find("DemoLighting").GetComponent<TimeScalar>().isNightTime)
            {
                if (!handheldLantern)
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
    }

    void SetEventTriggers()
    {
        if (gameObject.activeInHierarchy)
        {
            EventsManager.StartListening("ToggleLights", ToggleLight);
        }
    }



    void ToggleLight()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(ToggleLightsOnBy(15.0f));
        }
    }
    IEnumerator ToggleLightsOnBy(float waitTime)
    {
        float randomTimeToLights = Random.Range(0, waitTime);
        yield return new WaitForSeconds(randomTimeToLights);
        SetLights(lanternBool = !lanternBool);
    }

    //Controls both the mesh and light source.
    public void SetLights(bool lightToggle)
    {
        Debug.Log("Light set to: " + lightToggle);
        if (handheldLantern)
        {
            meshRend.enabled = lightToggle;
        }
        light.enabled = lightToggle;
    }

    private void Update()
    {
        if (light == null)
            return;

        if (isPlayersLantern)
        {
            StartCoroutine(ToggleLightsOnBy(0));
        }

        // pop off an item if too big
        if (light.isActiveAndEnabled)
        {
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
}