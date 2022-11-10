using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TextUpdate : MonoBehaviour
{
    protected float seconds = 0.0f;
    protected float timeSpend = 0.0f;
    Text text_time;
    public GlobalData globalData;

    // Start is called before the first frame update
    void Start()
    {
        text_time = GetComponent<Text>();
        globalData.killed = 0;
        globalData.saved = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeSpend += Time.deltaTime;
        seconds = timeSpend;
        text_time.text = string.Format("{0:F4}s, {1:D2} killed, {2:D2} saved, {3:F2} score", 
            seconds, globalData.killed, globalData.saved, globalData.score);
    }
}
