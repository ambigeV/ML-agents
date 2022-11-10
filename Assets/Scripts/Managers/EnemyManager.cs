using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyManager : MonoBehaviour
{

    public GameObject _spawnPointContainer;
    public GameObject[] _tankPrefab;
    public GameObject _bossPrefab;
    public GameObject _powerUpLocations;
    public GameObject _bossLocation;

    public delegate void AllEnemyDead();
    public AllEnemyDead dAllEnemyDead = null;

    public Transform waterTankLocation;

    private bool bossSpawned = false;
    private bool bossDead = false;

    protected Transform[] targets;
    
    protected Transform bossLocation;

    protected List<Transform> mSpawnPoints = new List<Transform>();
    
    protected List<EnemyTank> mEnemyTanks = new List<EnemyTank>(); 

    protected List<Transform> powerUpLocations = new List<Transform>();

    public GlobalData globalData;

    void Awake()
    {
        Transform spawnTrans = _spawnPointContainer.transform;
        for (int i = 0; i < spawnTrans.childCount; i++)
            mSpawnPoints.Add(spawnTrans.GetChild(i));

        Transform spawnTransPower = _powerUpLocations.transform;
        for (int i = 0; i < spawnTransPower.childCount; i++)
            powerUpLocations.Add(spawnTransPower.GetChild(i));

        bossLocation = _bossLocation.transform;
    }

    
    public void OnEnemyTankDeath(EnemyTank target)
    {
        mEnemyTanks.Remove(target);
        Destroy(target.gameObject);
        
        if (bossDead && mEnemyTanks.Count == 0) {
            dAllEnemyDead.Invoke();
        }
    }

    
    public void OnBossTankDeath(EnemyTank target)
    {
        mEnemyTanks.Remove(target);
        Destroy(target.gameObject);
        bossDead = true;

        if (mEnemyTanks.Count == 0) {
            dAllEnemyDead.Invoke();
        }
    }

    public void Restart(Transform[] targets)
    {
        this.targets = targets;
        foreach (EnemyTank eTank in mEnemyTanks) {
            Destroy(eTank.gameObject);
        }
        mEnemyTanks.Clear();
        
        bossSpawned = false;
        bossDead = false;
    }
    

    public void UpdateTarget(Transform[] targets)
    {
        this.targets = targets;
        foreach (EnemyTank eTank in mEnemyTanks) {
            eTank.UpdateTarget(targets);
        }
    }

    public void UpdatePoints()
    {
        foreach (EnemyTank eTank in mEnemyTanks)
        {
            eTank.UpdatePoints(powerUpLocations.ToArray(),
                waterTankLocation);
        }
    }

    public void SpawnBoss() {
        if (!bossSpawned) {
            GameObject tank = Instantiate(_bossPrefab, bossLocation.position, bossLocation.rotation);
            EnemyTank eTank = tank.GetComponent<EnemyTank>();
            eTank.dTankDestroyed = OnBossTankDeath;
            eTank.UpdateTarget(targets);
            eTank.UpdatePoints(powerUpLocations.ToArray(),
                waterTankLocation);
            
            mEnemyTanks.Add(eTank);
            bossSpawned = true;
        }
    }

    void Start()
    {
        StartCoroutine(spawnEnemies());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Transform GenerateSpawnPosition()
    {
        bool safePositionFound = false;
        int attemptsRemaining = 100; // Prevent an infinite loop
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        while (!safePositionFound && attemptsRemaining > 0)
        {
            attemptsRemaining--;

            // Pick a random flower
            Transform randomTurret = waterTankLocation;

            // Position 10 to 20 cm in front of the flower
            float radience = UnityEngine.Random.Range(20f, 40f);
            float degree = 2 * Mathf.PI * UnityEngine.Random.Range(0.01f, 1.0f);
            float distanceFromTurret_1 = radience * Mathf.Cos(degree);
            float distanceFromTurret_2 = radience * Mathf.Sin(degree);
            potentialPosition = new Vector3(distanceFromTurret_1, 0, distanceFromTurret_2);
            potentialPosition = potentialPosition + waterTankLocation.position;

            // Point beak at flower (bird's head is center of transform)
            Vector3 toTurret = waterTankLocation.position - potentialPosition;
            potentialRotation = Quaternion.LookRotation(toTurret, Vector3.up);

            // Check to see if the agent will collide with anything
            Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.1f);

            // Safe position has been found if no colliders are overlapped
            safePositionFound = colliders.Length <= 1;
            //Debug.Log(colliders.Length);
            //safePositionFound = true;
        }

        if (safePositionFound)
        {
            Transform tempTransform = bossLocation;
            tempTransform.position = potentialPosition;
            //Debug.Log(potentialPosition);
            tempTransform.rotation = potentialRotation;
            return tempTransform;
        }
        else
            return null;
    }
    
    IEnumerator spawnEnemies()
    {
        while (!bossSpawned) {
            //Generate a sufficently random position
            //Transform spawnPoint = mSpawnPoints[Random.Range(0, mSpawnPoints.Count)];
            Transform spawnPoint = GenerateSpawnPosition();
            if (spawnPoint == null)
                continue;
            //GameObject tank = Instantiate(_tankPrefab[UnityEngine.Random.Range(0, 3)/2], spawnPoint.position, spawnPoint.rotation);
            else if(Random.Range(0, 5) < 4)
            {
                GameObject tank = Instantiate(_tankPrefab[1], spawnPoint.position, spawnPoint.rotation);
                EnemyTank eTank = tank.GetComponent<EnemyTank>();
                eTank.dTankDestroyed = OnEnemyTankDeath;
                eTank.UpdateTarget(targets);
                eTank.UpdatePoints(powerUpLocations.ToArray(), waterTankLocation);
                mEnemyTanks.Add(eTank);
                globalData.mEnemyTanks.Add(eTank);
            }
            Transform spawnPoint_2 = GenerateSpawnPosition();

            if (spawnPoint_2 == null)
                ;
            //GameObject tank = Instantiate(_tankPrefab[UnityEngine.Random.Range(0, 3)/2], spawnPoint.position, spawnPoint.rotation);
            else if(Random.Range(0, 5) < 4)
            {
                GameObject tank_2 = Instantiate(_tankPrefab[0], spawnPoint_2.position, spawnPoint_2.rotation);
                EnemyTank eTank_2 = tank_2.GetComponent<EnemyTank>();
                eTank_2.dTankDestroyed = OnEnemyTankDeath;
                eTank_2.UpdateTarget(targets);
                eTank_2.UpdatePoints(powerUpLocations.ToArray(), waterTankLocation);
                mEnemyTanks.Add(eTank_2);
                globalData.mEnemyTanks.Add(eTank_2);
            }

            yield return new WaitForSeconds(5);
        }
    }

    public List<Transform> GetEnemyTransforms() {
        List<Transform> trans = new List<Transform>();
        foreach (EnemyTank eTank in mEnemyTanks) {
            trans.Add(eTank.transform);
        }

        return trans;
    }
}
