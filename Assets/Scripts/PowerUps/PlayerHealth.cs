using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int health = 400;

    public GameObject pickupEffect;
    public AudioSource _powerSFX;
    public AudioClip _clipPickUpSound;
    
    void OnTriggerEnter(Collider other)
    {
        // Apply effect to the player
        PlayerTank stats = other.GetComponent<PlayerTank>();
        if (stats == null) {
            return;
        }
        // Play a sound effect
        _powerSFX.clip = _clipPickUpSound;
        _powerSFX.Play();
        // Speedbooster
        stats.TakeDamage(-health);
                
        // Spawn a cool effect
        GameObject effect = Instantiate(pickupEffect, transform.position, transform.rotation);

        // Play a sound effect
        
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Remove power up object
        Destroy(effect);
        Destroy(this);
    }
}

