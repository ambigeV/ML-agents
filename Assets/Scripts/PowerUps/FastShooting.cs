using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FastShooting : MonoBehaviour
{
    public float multiplier = 1.4f;
    public float duration = 5f;

    public GameObject pickupEffect;
    public AudioSource _powerSFX;
    public AudioClip _clipPickUpSound;
    
    void OnTriggerEnter(Collider other)
    {
        PlayerTank stats = other.GetComponent<PlayerTank>();
        if (stats == null) {
            return;
        }
        
        TankFiringSystem firesystem = other.GetComponent<TankFiringSystem>();
        if (firesystem == null) {
            return;
        }
        StartCoroutine(Pickup(firesystem));
    }

    IEnumerator Pickup(TankFiringSystem firesystem)
    {
        _powerSFX.clip = _clipPickUpSound;
        _powerSFX.Play();
        // Speedbooster
        firesystem._cooldown /= multiplier;
                
        // Spawn a cool effect
        GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);

        // Play a sound effect
        
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        //Wait X amount of seconds
        yield return new WaitForSeconds(duration);

        //Reverse the effect on our player
        firesystem._cooldown *= multiplier;

        _powerSFX.Stop();

        // Remove power up object
        Destroy(effect);
        Destroy(this);
    }
}

