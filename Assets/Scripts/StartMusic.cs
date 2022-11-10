using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMusic : MonoBehaviour
{
    private bool ifPlay = false;
    public AudioSource _musicSource;
    // Start is called before the first frame update
    void Start()
    {
        if(ifPlay)
            _musicSource.Play();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
