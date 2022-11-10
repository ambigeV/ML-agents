using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class EnemyTank : MonoBehaviour
{
    public float _minMaxHealth = 50f, _maxMaxHealth = 200f;
    public float _maxHealth = 100;
    public float mHealth;

    //difficulty
    public GlobalData globalData;

    //delegate
    public delegate void TankDestroyed(EnemyTank target);
    public TankDestroyed dTankDestroyed;
    public bool ifFriend = true;

    public enum State
    {
        Idle = 0,
        Moving,
        Chase,
        TakingDamage,
        Death,
        Inactive
    };
    protected State mState;
    protected float _timeLimit = 3;
    protected Rigidbody mRigidbody;
    protected TankFiringSystem mTankShot;
    public float _minCoolDown = 3f, _maxCoolDown = 1f;

    UnityEngine.AI.NavMeshAgent agent;
    public float _minSpeed = 2f, _maxSpeed = 5f;

    // Waypoints and Player transform
    private Transform waterTankGoal;
    private Transform[] goals;
    private Transform[] players;
    public float _threshold = 25f, _lowThreshold = 15f;
    public float _minThreshold = 8f, _maxThreshold = 20f;
    public float _minLowThreshold = 5f, _maxLowThreshold = 10f;
    private Random rnd = new Random();
    private int currentWP;
    private bool randomOn = false;

    // Health
    public Slider HPStrip;
    public Image fill;
    private float rate = 0.25f;

    // Chase
    private float mTime = 0;
    private int playIndex = -1;

    public void InitHP()
    {
        HPStrip.value = mHealth;
        HPStrip.maxValue = _maxHealth;
        fill.color = Color.green;
    }

    void UpdateHP()
    {
        HPStrip.value = mHealth;
        if (HPStrip.value <= rate * HPStrip.maxValue)
        {
            fill.color = Color.red;
        } else {
            fill.color = Color.green;
        }
    }

    // Utility Function
    //Vector3 Cross(Vector3 v, Vector3 w)
    //{
    //    float xMult = v.y * w.z - v.z * w.y;
    //    float yMult = v.x * w.z - v.z * w.x;
    //    float zMult = v.x * w.y - v.y * w.x;

    //    return (new Vector3(xMult, yMult, zMult));
    //}

    //void CalculateAngle(Transform target)
    //{
    //    Vector3 tankForward = transform.forward;
    //    Vector3 fuelDirection = target.position - transform.position;

    //    float dot = tankForward.x * fuelDirection.x + tankForward.y * fuelDirection.y + tankForward.z * fuelDirection.z;
    //    float angle = Mathf.Acos(dot / (tankForward.magnitude * fuelDirection.magnitude));

    //    int clockwise = 1;
    //    if (Cross(tankForward, fuelDirection).y < 0)
    //        clockwise = -1;

    //    //if ((angle * Mathf.Rad2Deg) > 10)
    //    this.transform.LookAt(target.position);
    //}

    // Awake is called right at the beginning if the object is active
    private void Awake()
    {
        mRigidbody = GetComponent<Rigidbody>();
        mTankShot = GetComponent<TankFiringSystem>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        float df = globalData.difficulty;
        _maxHealth = _minMaxHealth + (_maxMaxHealth - _minMaxHealth) * df;
        agent.speed = _minSpeed + (_maxSpeed - _minSpeed) * df;
        mTankShot._cooldown = _minCoolDown + (_maxCoolDown - _minCoolDown) * df;
        _threshold = _minThreshold + (_maxThreshold - _minThreshold) * df;
        _lowThreshold = _minLowThreshold + (_maxLowThreshold - _minLowThreshold) * df;

        mHealth = _maxHealth;
        InitHP();

    }

    public void UpdateDestination(Transform target)
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            agent.Resume();
        }
    }

    public void UpdateRandomDestination(Transform target, float uncertainty)
    {
        randomOn = true;
        float degree = 2 * Mathf.PI * UnityEngine.Random.Range(0.01f, 1.0f);
        float distanceX = uncertainty * Mathf.Cos(degree);
        float distanceZ = uncertainty * Mathf.Sin(degree);
        Vector3 newPosition = target.position + new Vector3(distanceX, 0, distanceZ);

        if (target != null)
        {
            agent.SetDestination(newPosition);
            agent.Resume();
        }
    }

    public void UpdatePoints(Transform[] points, Transform keyPoint)
    {
        waterTankGoal = keyPoint;
        goals = points;
    }

    public void UpdateTarget(Transform[] target)
    {
        //int num = targets.Length;
        //for (int i = 0; i < num; ++i)
        //    players[i] = targets[i];
        players = target;
    }

    void Shoot() {
        agent.Stop();

        if (!ifFriend)
        {
            if (rnd.Next() % 1 == 0)
            {
                mTankShot.Fire();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
        if (rnd.Next() % 10 > -1)
        {
            currentWP = -1;
            UpdateDestination(waterTankGoal);
        }
        else
        {
            currentWP = rnd.Next() % goals.Length;
            UpdateDestination(goals[currentWP]);
        }
    }

    // Move around the station --> Chase the goal
    // --> Shoot the goal
    void Chase()
    {
        if (playIndex >= players.Length)
            return;

        float goalDistance = Vector3.Distance(players[playIndex].position, this.transform.position);

        if(goalDistance <= _lowThreshold)
        {
            mTime = _timeLimit;
            this.transform.LookAt(players[playIndex].position);
            Shoot();
        }
        else if (goalDistance <= _threshold)
        {
            UpdateDestination(players[playIndex]);
        }
    }

    void Move()
    {
        if (ifFriend)
        {
            UpdateDestination(waterTankGoal);
            if (Vector3.Distance(this.transform.position, waterTankGoal.position) < _lowThreshold)
            {
                this.transform.LookAt(waterTankGoal.position);
                state = State.Death;
                globalData.saved += 1;
            }
            return;
        }

        int numGoal = players.Length;
        if (numGoal < 1)
            return;

        float goalDistance = Vector3.Distance(players[0].position, this.transform.position);
        playIndex = 0;

        if(numGoal > 1)
        { 
            float goalDistance2 = Vector3.Distance(players[1].position, this.transform.position);
            if (goalDistance2 < goalDistance)
            {
                goalDistance = goalDistance2;
                playIndex = 1;
            }
        }

        if (goalDistance < _threshold * 0)
        {
            state = State.Chase;
            return;
        }
        else
        {
            if (currentWP == -1)
            {
                if (Vector3.Distance(this.transform.position, waterTankGoal.position) < _lowThreshold)
                {
                    this.transform.LookAt(waterTankGoal.position);
                    Shoot();
                    return;
                }

                else
                    ;
            }
            else
            {
                if (Vector3.Distance(this.transform.position, goals[currentWP].position) < _threshold)
                {
                    if (rnd.Next() % 10 > -1)
                    {
                        currentWP = -1;
                        UpdateDestination(waterTankGoal);
                    }
                    else
                    {
                        int curNext = -1;
                        do
                        {
                            curNext = rnd.Next() % goals.Length;

                        } while (curNext == currentWP);

                        currentWP = curNext;
                        UpdateDestination(goals[currentWP]);
                    }
                }
                else
                    UpdateDestination(goals[currentWP]);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHP();
        switch (state)
        {
            case State.Idle:
            case State.Moving:
                Move();
                break;

            case State.Chase:
                Chase();
                mTime -= Time.deltaTime;
                if (mTime <= 0)
                    state = State.Moving;
                break;

            case State.TakingDamage:
                break;
            case State.Death:
                break;
            case State.Inactive:
                break;
            default:
                break;
        }
    }
    protected void FireInput()
    {
        if (mTankShot.Fire())
        {
            //PlaySFX(_clipShotFired);
        }
    }


    // Physic update. Update regardless of FPS
    void FixedUpdate()
    {

    }


    public void TakeDamage(float damage)
    {
        if (mState != State.Inactive || mState != State.Death)
        {
            mHealth = Mathf.Clamp(mHealth-damage, 0, _maxHealth);

            if (mHealth > 0)
                state = State.TakingDamage;
            else
            {
                state = State.Death;
                if (!ifFriend)
                    globalData.killed += 1;
            }
        }
    }

    protected void Death()
    {
        //PlaySFX(_clipTankExplode);
        StartCoroutine(ChangeState(State.Inactive, 0.2f));
    }

    public void Restart(Vector3 pos, Quaternion rot)
    {
        // Reset position, rotation and health
        transform.position = pos;
        transform.rotation = rot;
        mHealth = _maxHealth;

        // Diable kinematic and activate the gameobject and input
        mRigidbody.isKinematic = false;
        gameObject.SetActive(true);

        // Re-initalize the health bar
        InitHP();

        // Change state
        state = State.Idle;
    }

    private IEnumerator ChangeState(State state, float delay)
    {
        // Delay
        yield return new WaitForSeconds(delay);

        // Change state
        this.state = state;
    }

    public State state
    {
        get { return mState; }
        set
        {
            if (mState != value)
            {
                switch (value)
                {
                    case State.Idle:
                        break;

                    case State.Moving:
                        break;

                    case State.Chase:
                        mTime = _timeLimit;
                        break;

                    case State.TakingDamage:
                        StartCoroutine(ChangeState(State.Idle, 0.2f));
                        break;

                    case State.Death:
                        Death();
                        break;

                    case State.Inactive:
                        gameObject.SetActive(false);
                        dTankDestroyed.Invoke(this);
                        mRigidbody.isKinematic = true;
                        break;
                    default:
                        break;
                }

                mState = value;
            }
        }
    }
}
