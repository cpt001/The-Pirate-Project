using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonShot : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    private float speed = 10000f;
    [SerializeField] private bool inertProjectile = false;
    [Range(0, 6)][Tooltip("Denotes Calibur: 0/6, 1/8, 2/12, 3/18, 4/24, 5/32, 6/36")]
    private int shotCalibur;    //This is set by the gun firing it - scale 0-7 - 6/8/12/18/24/32/36
    private int castLeakSpeed;

    private void Start()
    {
        EventsManager.StartListening("FreshShot" + this, Fired);
    }

    private void Fired()
    {
        gameObject.layer = 17;
        StartCoroutine(ArmTimer());
        StartCoroutine(LongDeactivateTimer());
    }

    void LateUpdate()
    {
        if (!inertProjectile)
        {
            _rb.AddForce(transform.forward * speed * Time.deltaTime);
            RaycastHit rayHit;
            if (Physics.Raycast(transform.position, Vector3.left, out rayHit, 1.5f))
            {
                //Debug.Log("Raycast hit " + rayHit.transform);
                if (rayHit.transform.CompareTag("ShipInteractable"))
                {
                    Debug.Log("Hit ship component");
                    gameObject.SetActive(false);
                }
                if (rayHit.transform.CompareTag("Ship"))
                {
                    Debug.DrawRay(rayHit.point, Vector3.forward, Color.red);
                    ApplyHullDamage(rayHit.transform.root.GetComponent<ControllableShip>(), rayHit.point);
                }
            }
            Debug.DrawRay(transform.position, Vector3.left);
        }
    }

    private IEnumerator ArmTimer()
    {
        inertProjectile = false;
        yield return new WaitForSeconds(0.3f);
        gameObject.layer = 16;
    }

    //Not always working because projectile is too fast. Either slow down, or build raycast to compensate
    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("ShipInteractable"))// && allowCollisions)
        {
            //This needs to either damage or destroy the component
            Debug.Log("Hit a ship's component");
            gameObject.SetActive(false);
        }
        if (collision.transform.CompareTag("Ship"))// && allowCollisions)
        {
            //Get the hit position
            ContactPoint contact = collision.contacts[0];
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
            Vector3 pos = contact.point;
            Debug.DrawRay(contact.point, contact.normal, Color.red);
            ApplyHullDamage(collision.transform.root.GetComponent<ControllableShip>(), pos);
            //Leak applies an increase over time to force point over time
            //If this applies a +y coordinate, it gives a really convincing sinking effect c:
        }
        else
        {
            Debug.Log("Cannonball inert due to: " + collision.gameObject);
            inertProjectile = true;
            _rb.mass = 10000;
        }
    }

    void ApplyHullDamage(ControllableShip shipTargeted, Vector3 hitPosition)
    {
        int damageRand = Mathf.RoundToInt(Random.Range(-1, 1));
        castLeakSpeed = (int)shipTargeted.hullWood - (shotCalibur - damageRand);
        if (castLeakSpeed < 0)
        {
            castLeakSpeed = 0;
        }
        else if (castLeakSpeed < 4)
        {
            castLeakSpeed = 4;
        }

        shipTargeted.AddNewLeak(hitPosition, (ControllableShip.LeakSeverity)castLeakSpeed);
        inertProjectile = true;
        _rb.mass = 10000;
        StartCoroutine(DeactivateTimer());
    }

    IEnumerator DeactivateTimer()
    {
        yield return new WaitForSeconds(3.0f);
        gameObject.SetActive(false);
    }

    IEnumerator LongDeactivateTimer()
    {
        yield return new WaitForSeconds(15.0f);
        gameObject.SetActive(false);
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
