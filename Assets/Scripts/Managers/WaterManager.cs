using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    public GameObject _waterTankSpawnLocation;
    public GameObject _waterTankPrefab;

    public delegate void WaterTankDead(WatertankHealth dead); // It is called when watertank is dead
    public WaterTankDead dWaterTankDead = null;

    protected WatertankHealth mWaterTank = null;
    protected Transform mWaterTankPoint = null;
    private bool ifWaterTank = false;

    private void Awake()
    {
        mWaterTankPoint = _waterTankSpawnLocation.transform;

        if(ifWaterTank)
            SpawnWaterTank();
    }

    public void OnWaterTankDeath(WatertankHealth target)
    {
        dWaterTankDead.Invoke(mWaterTank);
    }

    public void Restart()
    {
        if(ifWaterTank)
            mWaterTank.Restart(mWaterTankPoint.position, mWaterTankPoint.rotation);
    }

    public void SpawnWaterTank()
    {
        //Spawn WaterTank
        GameObject tank = Instantiate(_waterTankPrefab, mWaterTankPoint.position,
            mWaterTankPoint.rotation);
        mWaterTank = tank.GetComponent<WatertankHealth>();
        mWaterTank.dGotDestroyed = OnWaterTankDeath;
    }

    public void AddHealth(float amount)
    {
        mWaterTank.TakeDamage(-amount);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
