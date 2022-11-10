using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] _playerSpawnLocation;
    public GameObject _tankPrefab;
    public GlobalData globalData;

    public delegate void PlayerDead(PlayerTank dead);    // This will be called when only one tank left in the scene
    public PlayerDead dPlayerDead = null;

    protected PlayerTank mTank = null;
    protected PlayerTank nTank = null;
    protected Transform mSpawnPoint = null;
    protected Transform nSpawnPoint = null;
    protected int playNumbers;

    private void Awake()
    {
        mSpawnPoint = _playerSpawnLocation[0].transform;
        if (!globalData.isSinplePlayer)
            nSpawnPoint = _playerSpawnLocation[1].transform;
        SpawnTank();
    }

    public void OnPlayerDeath(PlayerTank target)
    {
        dPlayerDead.Invoke(mTank); // First tank is always the winner
    }

    public void Restart()
    {
        playNumbers = 1;
        mTank.Restart(mSpawnPoint.position, mSpawnPoint.rotation);
        if (!globalData.isSinplePlayer) {
            playNumbers++;
            nTank.Restart(nSpawnPoint.position, nSpawnPoint.rotation);
        }
    }

    // Spawn and setup their color
    public void SpawnTank()
    {
        GameObject tank = Instantiate(_tankPrefab, mSpawnPoint.position, mSpawnPoint.rotation);
        mTank = tank.GetComponent<PlayerTank>();
        mTank.dTankDestroyed = OnPlayerDeath;
        mTank._playerNum = 0;
        MeshRenderer[] renderers = mTank.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer rend in renderers)
            rend.material.color = Color.red;
        if (!globalData.isSinplePlayer) {
            GameObject tank2 = Instantiate(_tankPrefab, mSpawnPoint.position, mSpawnPoint.rotation);
            nTank = tank2.GetComponent<PlayerTank>();
            nTank.dTankDestroyed = OnPlayerDeath;
            nTank._playerNum = 1;
            MeshRenderer[] renderers2 = nTank.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer rend in renderers2)
                rend.material.color = Color.blue;
        }
    }

    public Transform[] GetTankTransform()
    {
        
        if (globalData.isSinplePlayer) {
            if (mTank.transform.gameObject.activeSelf) {
                return new Transform[] { mTank.transform };
            } else {
                return new Transform[] { };
            }
        } else {
            if (mTank.transform.gameObject.activeSelf &&
                nTank.transform.gameObject.activeSelf)
            {
                return new Transform[] { mTank.transform, nTank.transform };
                
            }
            else if (mTank.transform.gameObject.activeSelf)
            {
                Transform[] targets = new Transform[] { mTank.transform };
                return targets;
            }
            else 
            {
                return new Transform[] { nTank.transform };
                
            }
        }

    }
}
