namespace echo17.EndlessBook.Demo02
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Simple class to rotate a camera around the y axis
    /// </summary>
    public class RotateCamera : MonoBehaviour
    {
        public Transform rotateTransform;
        public float rotateSpeed;

        void Update()
        {
            rotateTransform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        }
    }
}