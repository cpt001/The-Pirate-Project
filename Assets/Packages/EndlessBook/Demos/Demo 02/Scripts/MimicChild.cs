namespace echo17.EndlessBook.Demo02
{
    using UnityEngine;

    /// <summary>
    /// Makes an object act like a child of another object.
    /// This is useful because we can have objects follow the book's
    /// transforms and not disappear when the animated book is
    /// set inactive, switching to a static standin
    /// </summary>
    public class MimicChild : MonoBehaviour
    {
        protected Vector3 parentInitialPosition;
        protected Quaternion parentInitialRotation;
        protected Vector3 childInitialPosition;
        protected Quaternion childInitialRotation;
        protected Matrix4x4 parentMatrix;

        public Transform parentTransform;

        void Start()
        {
            parentInitialPosition = parentTransform.position;
            parentInitialRotation = parentTransform.rotation;

            childInitialPosition = transform.position;
            childInitialRotation = transform.rotation;

            childInitialPosition = DivideVectors(Quaternion.Inverse(parentTransform.rotation) * (childInitialPosition - parentInitialPosition), parentTransform.lossyScale);
        }

        void LateUpdate()
        {
            parentMatrix = Matrix4x4.TRS(parentTransform.position, parentTransform.rotation, parentTransform.lossyScale);
            transform.position = parentMatrix.MultiplyPoint3x4(childInitialPosition);
            transform.rotation = (parentTransform.rotation * Quaternion.Inverse(parentInitialRotation)) * childInitialRotation;
        }

        protected virtual Vector3 DivideVectors(Vector3 num, Vector3 den)
        {
            return new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);
        }
    }
}