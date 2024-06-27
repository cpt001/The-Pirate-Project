using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public enum CameraState
    {
        FirstPerson,
        ThirdPerson,
        Death,
        Cutscene,
    }

    public Camera mainCamera;

    void SetPlayerCamera()
    {

    }
    void SetShipCamera()
    {

    }
    void SetIslandOverviewCamera()
    {
        throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
