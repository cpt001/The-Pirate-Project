using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    private bool isOperable = true;
    private bool loaded = true;
    private int numCrewManning = 4;
    [SerializeField] private GameObject barrel;
    [SerializeField] private GameObject fireParticle;
    private float particleResetTime = 1.7f;

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

    private void Awake()
    {
        //Debug.Log("Cannon root: " + gameObject.transform.root.name);
        if (transform.parent.name == "BroadsidePort")
        {
            EventsManager.StartListening("Broadside_Port_" + gameObject.transform.root.name, Fire);
        }
        if (transform.parent.name == "BroadsideStarboard")
        {
            EventsManager.StartListening("Broadside_Starboard_" + gameObject.transform.root.name, Fire);
        }
    }

    void Fire()
    {
        if (loaded && isOperable)
        {
            float fireDelay = Random.Range(1f, 3f);
            StartCoroutine(FireWeapon(fireDelay));
        }
    }

    private IEnumerator FireWeapon(float fireDelay)
    {
        loaded = false;
        yield return new WaitForSeconds(fireDelay * (numCrewManning * 0.25f));  //Fire delay is increased if >4 crew man cannon
        StartCoroutine(EffectController());
        GameObject cannonBall = ShotPool.shotInstance.GetPooledObject();
        if (cannonBall != null)
        {
            cannonBall.transform.position = barrel.transform.position;
            cannonBall.transform.localEulerAngles = barrel.transform.localEulerAngles;  //There's still some odd issues being caused by this. Might be related to how things are parented?
            cannonBall.SetActive(true);
        }
        StartCoroutine(ReloadTime(60.0f));  //Minute and a half between shots theoretically
    }
    private IEnumerator EffectController()
    {
        fireParticle.SetActive(true);
        yield return new WaitForSeconds(particleResetTime);
        fireParticle.SetActive(false);
    }
    private IEnumerator ReloadTime(float delay)
    {
        yield return new WaitForSeconds(delay * (numCrewManning * 0.25f));
        loaded = true;
    }
}
