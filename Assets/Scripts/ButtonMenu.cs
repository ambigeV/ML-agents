using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class ButtonMenu : MonoBehaviour
{

    public GlobalData globalData;

    private int[] scenes = {0, 0}; 

    public void PlayGame()
    {
        Random rnd = new Random();
        int sceneInd = rnd.Next() % 2;
        globalData.isSinplePlayer = true;
        SceneManager.LoadScene(scenes[sceneInd]);
    }
    
    public void PlayGameMultiplayer()
    {
        Random rnd = new Random();
        int sceneInd = rnd.Next() % 2;

        globalData.isSinplePlayer = false;
        SceneManager.LoadScene(scenes[sceneInd]);
    }

    public void QuitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
        if (globalData.isSinplePlayer)
            PlayGame();
        else
            PlayGameMultiplayer();
    }
}
