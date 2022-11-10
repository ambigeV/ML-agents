using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public PlayerManager _playerManager;
    public EnemyManager _enemyManager;
    public WaterManager _waterManager;
    public GameObject WinBackGround;
    public GameObject LoseBackGround;

    public AudioSource _musicSource;
    public AudioClip _clipWin;
    public AudioClip _clipLose;
    public AudioClip _clipGame;
    private int round = 0;
    private bool ifSurvive = true;
    private bool ifFinal = false;
    private float playTime = 0f;
    private float maxTime = 60f*3;
    private bool ifPlay = false;
    private bool ifBoss = false;

    public GlobalData globalData;

    public enum State
    {
        GameLoads = 0,
        GamePrep,
        GameLoop,
        GameEnds
    };
    private State mState = State.GameLoads;

    private void Start()
    {
        _playerManager.dPlayerDead = PlayerDead;
        _enemyManager.dAllEnemyDead = AllEnemyDead;
        _waterManager.dWaterTankDead = WaterDead;

        state = State.GamePrep;
        if (ifPlay)
        {
            _musicSource.clip = _clipGame;
            _musicSource.Play();
        }
    }

    public void PlayerDead(PlayerTank tank)
    {
        if (state == State.GameLoop)
        {
            // Set Canvas to Active
            if (ifPlay)
            {
                _musicSource.clip = _clipLose;
                _musicSource.Play();
            }
            //LoseBackGround.SetActive(true);

            // Eliminate All the Tanks
            _enemyManager.Restart(_playerManager.GetTankTransform());
            globalData.ifReset = false;
            // Pause the Game
            //ReturnToMenu(0);
        }
    }

    public void WaterDead(WatertankHealth target)
    {
        if (state == State.GameLoop)
        {
            // Set Canvas to Active
            if (ifPlay)
            {
                _musicSource.clip = _clipLose;
                _musicSource.Play();
            }
            //LoseBackGround.SetActive(true);

            // Eliminate All the Tanks
            _enemyManager.Restart(_playerManager.GetTankTransform());

            // Pause the Game
            //ReturnToMenu(0);
        }
    }

    public void AllEnemyDead()
    {
        if (state == State.GameLoop)
        {
            if (ifPlay)
            {
                _musicSource.clip = _clipWin;
                _musicSource.Play();
            }
            WinBackGround.SetActive(true);

            // Eliminate All the Tanks
            _enemyManager.Restart(_playerManager.GetTankTransform());

            // Pause the Game
            ReturnToMenu(0);
        }
    }

    private void InitGamePrep()
    {
        // Initialize all tanks
        _playerManager.Restart();
        _waterManager.Restart();
        //_enemyManager.Restart(_playerManager.GetTankTransform());
        playTime = 0;

        // Change state to game loop
        state = State.GameLoop;
    }

    private void ReturnToMenu(int num)
    {
        Time.timeScale = num;
        //SceneManager.LoadScene(1);
    }

    private IEnumerator InitGameEnd()
    {
        // Delay before starting a new round
        yield return new WaitForSeconds(3f);

        // Reinitialize tanks
        state = State.GamePrep;
    }

    private IEnumerator InitGameEnd2()
    {
        ReturnToMenu(0);
        // Delay before starting a new round
        yield return new WaitForSeconds(3f);
        
        ReturnToMenu(1);
        state = State.GameEnds;
    }

    void Update()
    {
        playTime += Time.deltaTime;
        if (ifBoss && playTime >= 60f*2)
        {
            _enemyManager.SpawnBoss();
        }
        
        if(false&&globalData.ifReset)
        {
            _enemyManager.Restart(_playerManager.GetTankTransform());

            globalData.ifReset = false;
        }

        switch (state)
        {
            case State.GamePrep:
            case State.GameEnds:
                break;

            case State.GameLoop:
                _enemyManager.UpdateTarget(_playerManager.GetTankTransform());
                _enemyManager.UpdatePoints();
                break;
            default:
                break;
        }
    }

    public State state
    {
        get { return mState; }
        set
        {
            if(mState != value)
            {
                mState = value;

                switch (value)
                {
                    case State.GamePrep:
                        InitGamePrep();
                        break;

                    case State.GameLoop:
                        break;

                    case State.GameEnds:
                        StartCoroutine(InitGameEnd());
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
