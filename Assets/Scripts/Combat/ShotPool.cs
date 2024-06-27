using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotPool : MonoBehaviour
{
    public static ShotPool shotInstance;
    [SerializeField] private List<GameObject> pooledObjects;
    [SerializeField] private GameObject objectToPool;
    [SerializeField] private int amountToPool;
    // Start is called before the first frame update
    void Awake()
    {
        shotInstance = this;
    }

    // Update is called once per frame
    void Start()
    {
        pooledObjects = new List<GameObject>();
        GameObject tmp;

        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool, transform);
            tmp.SetActive(false);
            pooledObjects.Add(tmp);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                return pooledObjects[i];
            }
        }
        return null;
    }
}
