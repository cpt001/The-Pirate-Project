using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;

namespace PlaceholderSoftware.WetStuff.Demos.Demo_Assets
{
    public class PauseTimeline
        : MonoBehaviour
    {
        private const float MovementSpeed = 3;
        private const float FreeLookSensitivity = 3;

        private bool _isCameraFree = false;
        private bool _isLooking = false;

        public bool ShowPauseTimeline = true;

        private void Start()
        {
            if (XRSettings.enabled)
            {
                GetComponent<PlayableDirector>().enabled = false;
                enabled = false;
            }
        }

        private void Update ()
        {
            if (_isCameraFree)
                FreeLook();
        }

        public void OnGUI()
        {
            if (XRSettings.enabled)
                return;

            if (ShowPauseTimeline)
            {
                var timeline = GetComponent<PlayableDirector>();

                //Create a box for the button
                var freeBox = new Rect(20, 50, 130, 30);

                if (!_isCameraFree)
                {
                    if (GUI.Button(freeBox, "Free Camera")) {
                        _isCameraFree = true;
                        timeline.Pause();
                    }
                }
                else
                {
                    if (GUI.Button(freeBox, timeline.isActiveAndEnabled ? "Resume Timeline" : "Lock Camera")) {
                        _isCameraFree = false;
                        timeline.Resume();
                    }
                }
            }
        }

        private void FreeLook()
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                transform.position = transform.position + (transform.forward * MovementSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                transform.position = transform.position + (-transform.right * MovementSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                transform.position = transform.position + (-transform.forward * MovementSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                transform.position = transform.position + (transform.right * MovementSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.E))
                transform.position = transform.position + (transform.up * MovementSpeed * Time.deltaTime);

            if (Input.GetKey(KeyCode.Q))
                transform.position = transform.position + (-transform.up * MovementSpeed * Time.deltaTime);

            if (_isLooking)
            {
                var newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * FreeLookSensitivity;
                var newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * FreeLookSensitivity;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
                _isLooking = true;
            else if (Input.GetKeyUp(KeyCode.Mouse1))
                _isLooking = false;
        }
    }
}
