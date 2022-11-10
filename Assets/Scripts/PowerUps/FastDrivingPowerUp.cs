using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FastDrivingPowerUp : MonoBehaviour
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
        StartCoroutine(Pickup(stats));
    }

    IEnumerator Pickup(PlayerTank stats)
    {
        _powerSFX.clip = _clipPickUpSound;
        _powerSFX.Play();
        // Speedbooster
        stats._moveSpeed *= multiplier;
        stats._rotationSpeed *= multiplier;
                
        // Spawn a cool effect
        GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);

        // Play a sound effect
        
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        //Wait X amount of seconds
        yield return new WaitForSeconds(duration);

        //Reverse the effect on our player
        stats._moveSpeed /= multiplier;
        stats._rotationSpeed /= multiplier;

        _powerSFX.Stop();

        // Remove power up object
        Destroy(effect);
        Destroy(this);
    }
}

