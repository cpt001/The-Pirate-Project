using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonShot : MonoBehaviour
{
    //[SerializeField] private Rigidbody _rb;
    private float speed = 200f;
    private bool allowCollisions = false;
    // Start is called before the first frame update
    void LateUpdate()
    {
        //_rb.AddRelativeForce(transform.forward * speed);
        transform.Translate(new Vector3(-1, -0.054f, 0) * speed * Time.deltaTime);
        //StartCoroutine(ArmTimer());
    }

    private IEnumerator ArmTimer()
    {
        Physics.IgnoreLayerCollision(8, 11, true);
        yield return new WaitForSeconds(.5f);
        allowCollisions = true;
        Physics.IgnoreLayerCollision(8, 11, false);
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
