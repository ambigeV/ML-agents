using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class GlobalData : ScriptableObject
{
    public float difficulty = 0;
    public bool isSinplePlayer = true;
    public int killed = 0;
    public int saved = 0;
    public bool trainingMode = true;
    public bool ifReset = false;
    public int enemyHit = 0;
    public int friendHit = 0;
    public int emptyHit = 0;
    public float score = 0;
    public int metEnemy = 0;
    
    public List<EnemyTank> mEnemyTanks = new List<EnemyTank>();

    public void OnSliderValueChanged(float value)
    {
        difficulty = value;
        Debug.Log(value);
    }

    public void AddTank(EnemyTank mtank)
    {
        mEnemyTanks.Add(mtank);
    }
}
