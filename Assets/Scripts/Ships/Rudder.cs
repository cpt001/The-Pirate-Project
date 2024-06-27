using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rudder : MonoBehaviour
{
    /// <summary>
    /// This works, but needs to be refined with rotate over time functionality.
    /// </summary>
    private ControllableShip assignedShip => transform.root.GetComponent<ControllableShip>();
    private float rotationSpeed;
    private float currentRotation;
    private float rotationLimit;
    private Quaternion homeRotation = Quaternion.AngleAxis(0, Vector3.right);

    // Update is called once per frame
    void Update()
    {
        if (assignedShip.playerControllingShip)
        {
            if (Input.GetKey(KeyCode.A))
            {
                transform.localRotation = Quaternion.Euler(0, Input.GetAxis("Horizontal") * -55, 0);
            }            
            if (Input.GetKey(KeyCode.D))
            {
                transform.localRotation = Quaternion.Euler(0, Input.GetAxis("Horizontal") * -55, 0);
            }
            else
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, homeRotation, Time.deltaTime);
                if (transform.localRotation.y >= 5 || transform.localRotation.y <= -5)
                {
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                }
            }
        }
        if (!assignedShip.playerControllingShip)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, homeRotation, Time.deltaTime);
        }
    }
}
