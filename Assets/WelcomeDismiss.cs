using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeDismiss : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            gameObject.SetActive(false);
        }
    }
}
