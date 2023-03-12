using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonShot : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    private float speed = 10000f;
    private bool allowCollisions = false;
    private bool inertProjectile = false;
    // Start is called before the first frame update
    private void Start()
    {
        gameObject.layer = 17;
        StartCoroutine(ArmTimer());
    }

    void LateUpdate()
    {
        if (!inertProjectile)
        {
            _rb.AddForce(transform.forward * speed * Time.deltaTime);
        }
    }

    private IEnumerator ArmTimer()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.layer = 16;
        //_rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        //allowCollisions = true;
        //Physics.IgnoreLayerCollision(8, 11, false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("ShipInteractable"))// && allowCollisions)
        {
            Debug.Log("Hit a ship's component");
            gameObject.SetActive(false);
        }
        if (collision.transform.CompareTag("Ship"))// && allowCollisions)
        {
            Debug.Log("Hit ship's main hull");
            gameObject.SetActive(false);
        }
        else
        {
            inertProjectile = true;
            _rb.mass = 10000;
        }
    }

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Ship") && allowCollisions)
        {
            Debug.Log("Cannonball hit ship!");
            gameObject.SetActive(false);
        }
    }*/
}
