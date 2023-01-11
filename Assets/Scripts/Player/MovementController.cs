using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    [Header("Movement Key Inputs")]
    [SerializeField]
    private KeyCode forward = KeyCode.W;
    [SerializeField]
    private KeyCode back = KeyCode.S;
    [SerializeField]
    private KeyCode strafeLeft = KeyCode.A;
    [SerializeField]
    private KeyCode strafeRight = KeyCode.D;
    [SerializeField]
    private KeyCode turnLeft = KeyCode.Q;
    [SerializeField]
    private KeyCode turnRight = KeyCode.E;
    [SerializeField]
    private KeyCode jump = KeyCode.Space;
    [SerializeField]
    private KeyCode crouch = KeyCode.C;
    [SerializeField]
    private KeyCode sprint = KeyCode.LeftShift;
    [SerializeField]
    private KeyCode autoRun = KeyCode.Numlock;

    [Header("Stats")]
    [SerializeField]
    private float movementSpeed;
    [SerializeField]
    private float sprintSpeed;
    private float turnSmoothVelocity;
    [SerializeField]
    private float turnSpeed;
    [SerializeField]
    private float jumpHeight;

    [Header("References")]
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    private Camera cam;


    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        cam = Camera.main;
        characterController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            characterController.Move(moveDir.normalized * movementSpeed * Time.deltaTime);
        }

        #region Primary movement
        /*if (Input.GetKey(forward))
        {
            _rb.AddForce(Vector3.forward * movementSpeed);
        }
        if (Input.GetKey(back))
        {
            _rb.AddForce(Vector3.back * movementSpeed);
        }
        if (Input.GetKey(strafeLeft))
        {
            _rb.AddForce(Vector3.left * movementSpeed);
        }
        if (Input.GetKey(strafeRight))
        {
            _rb.AddForce(Vector3.right * movementSpeed);
        }
        #endregion
        #region KeyTurning
        if (Input.GetKey(turnLeft))
        {
            transform.Rotate(0, 1 * turnSpeed, 0);
        }
        if (Input.GetKey(turnRight))
        {
            transform.Rotate(0, -1 * turnSpeed, 0);

        }
        #endregion
        #region Jump and Crouch
        if (Input.GetKey(jump))
        {
            _rb.AddForce(Vector3.up * jumpHeight);
        }
        if (Input.GetKey(crouch))
        {
            throw new System.Exception("Crouch not implemented!");
        }*/
        #endregion
    }
}
