using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    private bool isOperable;
    private bool loaded;
    private int numCrewManning;
    private bool temp_allowedToManuallyFire;
    [SerializeField] private GameObject barrel;

    private enum CannonType
    {
        Swivel,     //Short range, small shot
        Howitzer,   //X long range, big shot
        Carronade,  //Short range, big shot
        Cannon,     //Mid range, Mid shot
        Culverin,   //Long range, mid shot
    }
    [SerializeField] private CannonType cannonType;
    private enum ShotLoaded
    {
        Round,
        Double,
        Canister,
        Chain,
        Langrage,   //Junk shot
        Heated,
        Explosive,
    }
    [SerializeField] private ShotLoaded shotLoaded;

    private void Update()
    {
        if (loaded)
        {
            barrel.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            barrel.GetComponent<Renderer>().material.color = Color.gray;
        }

        if (temp_allowedToManuallyFire)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Fire(1);
            }
        }
    }
    void Fire(float fireDelay)
    {
        if (loaded)
        {
            StartCoroutine(FireWeapon(fireDelay));
        }
    }

    private IEnumerator FireWeapon(float fireDelay)
    {
        yield return new WaitForSeconds(fireDelay * (numCrewManning * 0.25f));  //Fire delay is increased if >4 crew man cannon

        GameObject cannonBall = ShotPool.shotInstance.GetPooledObject();
        if (cannonBall != null)
        {
            cannonBall.transform.position = this.transform.position;
            cannonBall.transform.rotation = this.transform.rotation;
            cannonBall.SetActive(true);
        }
    }
}
