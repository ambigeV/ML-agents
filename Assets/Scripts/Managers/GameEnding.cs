using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    public CanvasGroup exitWinCanvasGoup;
    public CanvasGroup exitLoseCanvasGroup;
    public bool isEnding = false;
    public bool ifWining = false;

    public float mTimer = 0;

    void EndLevel()
    {
        mTimer += Time.deltaTime;

        Debug.Log("Now the inter mTimer goes to \t" + mTimer);

        exitWinCanvasGoup.alpha = mTimer / fadeDuration;

        if (mTimer > fadeDuration + displayImageDuration)
            ;
    }

    // Start is called before the first frame update
    void Restart()
    {
        mTimer = 0;
        isEnding = false;
        ifWining = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnding)
        {
            EndLevel();
            Debug.Log("Now the outter mTimer goes to \t" + mTimer);
        }
    }

    public void SetEnding()
    {
        isEnding = true;
    }

    public void SetWining()
    {
        ifWining = true;
    }
}
