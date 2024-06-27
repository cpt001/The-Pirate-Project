namespace echo17.EndlessBook
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteInEditMode]

    public class Unpack : MonoBehaviour
    {
        // Start is called before the first frame update
        void Awake()
        {
#if UNITY_2019_1_OR_NEWER && !UNITY_2019_3_3 && UNITY_EDITOR
            Debug.Log("here");
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(transform))
            {
                UnityEditor.PrefabUtility.UnpackPrefabInstance(gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
            }
#endif
        }
    }
}
