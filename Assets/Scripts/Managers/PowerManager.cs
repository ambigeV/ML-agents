using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public GameObject _spawnPointContainer;
    public float duration = 8f;

    public List<GameObject> _powerUpPrefabs;

    protected List<(Transform trans, GameObject powerUp)> mSpawnPoints = new List<(Transform trans, GameObject powerUp)>();
    private bool ifPowerUp = false;

    void Awake()
    {
        Transform spawnTrans = _spawnPointContainer.transform;
        for (int i = 0; i < spawnTrans.childCount; i++)
            mSpawnPoints.Add((spawnTrans.GetChild(i), null));
    }

    void Start()
    {
        if(ifPowerUp)
            StartCoroutine(spawnPowerUp());
    }

    // Update is called once per frame
    void Update()
    {

    }


    IEnumerator spawnPowerUp()
    {
        while (true)
        {
            int idx = Random.Range(0, mSpawnPoints.Count);
            (Transform trans, GameObject powerUp) spawnPoint = mSpawnPoints[idx];
            GameObject powerUpPrefab = _powerUpPrefabs[Random.Range(0, _powerUpPrefabs.Count)];
            if (spawnPoint.powerUp != null) {
                Destroy(spawnPoint.powerUp);
            }
            
            GameObject powerup = Instantiate(powerUpPrefab, spawnPoint.trans.position, spawnPoint.trans.rotation);

            mSpawnPoints[idx] = (spawnPoint.trans, powerup);
            
            
            yield return new WaitForSeconds(duration);
        }
    }
}
