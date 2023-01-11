namespace echo17.EndlessBook.Demo02
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Simple class to rotate an object around an axis
    /// </summary>
    public class RotateObject : MonoBehaviour
    {
        protected Transform cachedTransform;

        public Vector3 rotationEulers;
        public float speed;

        void Awake()
        {
            cachedTransform = this.transform;
        }

        void Update()
        {
            cachedTransform.Rotate(rotationEulers * (speed * Time.deltaTime));
        }
    }
}