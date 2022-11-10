using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTank : Agent
{
    public float _moveSpeed;
    public float _minMoveSpeed = 30f, _maxMoveSpeed = 10f;
    public float _rotationSpeed;
    //audio clip
    public AudioSource _movementSFX;
    public AudioSource _tankSFX;
    public AudioClip _clipIdle;
    public AudioClip _clipMoving;
    public AudioClip _clipShotFired;
    public AudioClip _clipTankExplode;

    public float _pitchRange = 0.2f;

    public bool _inputIsEnabled = true;
    public bool _ifHuman = false;

    public int _playerNum;
    public float _maxHealth = 1000;
    public float _minMaxHealth = 2000f, _maxMaxHealth = 500f;
    public float mHealth;

    protected List<EnemyTank> mEnemyTanks = new List<EnemyTank>();
    private EnemyTank nearestEnemy;
    private EnemyTank nearestFriend;

    //delegate
    public delegate void TankDestroyed(PlayerTank target);
    public TankDestroyed dTankDestroyed;

    public enum State
    {
        Idle = 0,
        Moving,
        TakingDamage,
        Death,
        Inactive
    };
    protected State mState;

    protected Rigidbody mRigidbody;
    protected TankFiringSystem mTankShot;
    public float _minCoolDown = 0.1f, _maxCoolDown = 0.4f;
    //audio source
    protected float mOriginalPitch;

    protected string mVerticalAxisInputName = "Vertical";
    protected string mHorizontalAxisInputName = "Horizontal";
    protected string mFireInputName = "Fire";
    protected float mVerticalInputValue = 0f;
    protected float mHorizontalInputValue = 0f;

    // Difficulty
    public GlobalData globalData;

    // Health
    public Slider HPStrip;
    public Image fill;
    private float rate = 0.40f;

    // Training Mode?
    public bool training = true;

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

    // Awake is called right at the beginning if the object is active
    private void Awake()
    {
     
    }

    // Start is called before the first frame update
    void Start()
    {
        //mOriginalPitch = _movementSFX.pitch;
        
    }

    public override void Initialize()
    {
        mRigidbody = GetComponent<Rigidbody>();
        mTankShot = GetComponent<TankFiringSystem>();

        float df = globalData.difficulty;
        _maxHealth = _minMaxHealth + (_maxMaxHealth - _minMaxHealth) * df;
        _moveSpeed = _minMoveSpeed + (_maxMoveSpeed - _minMoveSpeed) * df;
        mTankShot._cooldown = _minCoolDown + (_maxCoolDown - _minCoolDown) * df;

        mHealth = _maxHealth;
        InitHP();

        //Debug.Log("Here We Have Initialization.");
    }

    // Restart
    public override void OnEpisodeBegin()
    {
        dTankDestroyed.Invoke(this);
        globalData.score = 0;
        gameObject.SetActive(true);
        mRigidbody.isKinematic = false;
        _inputIsEnabled = true;
        mState = State.Idle;
        this.state = State.Idle;
        globalData.killed = 0;
        globalData.saved = 0;
        globalData.metEnemy = 0;
        //_ifHuman = true;
        
        float df = globalData.difficulty;
        _maxHealth = _minMaxHealth + (_maxMaxHealth - _minMaxHealth) * df;
        _moveSpeed = _minMoveSpeed + (_maxMoveSpeed - _minMoveSpeed) * df;
        mTankShot._cooldown = _minCoolDown + (_maxCoolDown - _minCoolDown) * df;

        mHealth = _maxHealth;
        InitHP();

        //Debug.Log("Tell Me Where to Start?");
        //globalData.ifReset = true;
    }

    //The collected observations should be 10 dimensions
    //First 3 should be the nearest enemy
    //Next 1 should be the dot product related to this enemy
    //Next 1 should be the cross product related to this enemy
    //Second 3 should be the nearest friend
    //Next 1 should be the dot product related to this friend
    //Next 1 should be the cross product
    
    public override void CollectObservations(VectorSensor sensor)
    {
        //Nearest Enemy
        if (nearestEnemy != null)
        {
            //Debug.DrawLine(mRigidbody.position, nearestEnemy.transform.position, Color.green);
            Vector3 toEnemy = nearestEnemy.transform.position - mRigidbody.position;
            toEnemy = toEnemy.normalized;
            sensor.AddObservation(toEnemy.x);
            sensor.AddObservation(toEnemy.z);
            sensor.AddObservation(Vector3.Dot(toEnemy, this.transform.forward.normalized));
            Vector3 ifClock = Vector3.Cross(toEnemy, this.transform.forward.normalized);
            sensor.AddObservation(ifClock.y);

        }
        else
            sensor.AddObservation(new float[4]);


        //Nearest Enemy
        if (nearestFriend != null)
        {
            //Debug.DrawLine(mRigidbody.position, nearestFriend.transform.position, Color.red);
            Vector3 toFriend = nearestFriend.transform.position - mRigidbody.position;
            toFriend = toFriend.normalized;
            sensor.AddObservation(toFriend.x);
            sensor.AddObservation(toFriend.z);
            sensor.AddObservation(Vector3.Dot(toFriend, this.transform.forward.normalized));
            Vector3 ifClock_2 = Vector3.Cross(toFriend, this.transform.forward.normalized);
            sensor.AddObservation(ifClock_2.y);

        }
        else
            sensor.AddObservation(new float[4]);
    }
    
    //Index 0: rotation speed (-1 for one direction and +1 for another and 0 for no rotation)
    //Index 1: shooting (0 for no shooting and 1 for shooting)
    public override void OnActionReceived(ActionBuffers actions)
    {
        float rotation = Mathf.Clamp(actions.ContinuousActions[0], -1.0f, 1.0f);

        float shooting = 1.0f * actions.DiscreteActions[0];

        //Conduct rotation
        float rotationDegree = _rotationSpeed * Time.deltaTime * rotation;
        Quaternion rotQuat = Quaternion.Euler(0f, rotationDegree, 0f);
        mRigidbody.MoveRotation(mRigidbody.rotation * rotQuat);

        //Conduct shooting
        if (shooting > 0)
        {
            //Shooting and check the shooting result
            mTankShot.Fire();
        }
        else
            ;
    }

    /// When Behavior Type is set to "Heuristic Only" on the agent's Behavior Parameters,
    /// this function will be called. Its return values will be fed into
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var dActionsOut = actionsOut.DiscreteActions;
        var cActionsOut = actionsOut.ContinuousActions;
        cActionsOut[0] = Input.GetAxis(mHorizontalAxisInputName + (_playerNum + 1));
        dActionsOut[0] = Input.GetButton(mFireInputName + (_playerNum + 1)) ? 1 : 0 ;
        //Debug.Log(cActionsOut[0]);
        //actionsOut.DiscreteActions[0] = Input.GetAxis(mHorizontalAxisInputName + (_playerNum + 1));
        //actionsOut.DiscreteActions[1] = Input.GetButton(mFireInputName + (_playerNum + 1));
    }

    // Update is called once per frame
    void Update()
    {
        if (nearestFriend == null)
            UpdateNearestFriend();
        if (nearestEnemy == null)
            UpdateNearestEnemy();

        if (nearestFriend != null)
        {
            Debug.DrawLine(mRigidbody.position, nearestFriend.transform.position, Color.green);
            //Debug.Log(nearestFriend.transform.position);
        }
        if (nearestEnemy != null)
        {
            Debug.DrawLine(mRigidbody.position, nearestEnemy.transform.position, Color.red);
            Vector3 toEnemy = nearestEnemy.transform.position - mRigidbody.position;
            toEnemy = toEnemy.normalized;
            float dotValue = Vector3.Dot(toEnemy, this.transform.forward.normalized);
            Debug.Log(dotValue);

            if (globalData.metEnemy == 0 && dotValue > 0.95)
            {
                globalData.metEnemy = 1;
                globalData.score += 0.0f;
                AddReward(0.0f);
            }
            //Debug.Log(nearestEnemy.transform.position);
        }
        //Debug.Log(this.state);
        switch (this.state)
        {
            case State.Idle:
            case State.Moving:
                if (_inputIsEnabled && _ifHuman)
                {
                    FireInput();
                    MovementInput();
                }
                break;

            case State.TakingDamage:
                if (_inputIsEnabled && _ifHuman)
                {
                    //FireInput();
                }
                break;
            case State.Death:
                break;
            case State.Inactive:
                break;
            default:
                break;
        }

        UpdateHP();

        //Update the hit condition to assign reward
        if(globalData.enemyHit > 0)
        {
            globalData.enemyHit -= 1;
            //Add Score
            globalData.score += 0.1f;
            AddReward(0.1f);
            
        }
        if(globalData.friendHit > 0)
        {
            globalData.friendHit -= 1;
            //Add Score
            globalData.score += -0.3f;
            AddReward(-0.3f);
        }
        if(globalData.emptyHit > 0)
        {
            globalData.emptyHit -= 1;
            //Add Score
            globalData.score += -0.01f;
            AddReward(-0.01f);
        }

        //Update to check the KDA ratio to end game?
        if(globalData.killed >= 20)
        {
            globalData.score += 0.4f;
            AddReward(0.5f);
            EndEpisode();
        }

        if(globalData.saved >= 10)
        {
            globalData.score += 0.8f;
            AddReward(0.8f);
            EndEpisode();
        }
    }

    protected void MovementInput()
    {
        // Update input
        mVerticalInputValue = Input.GetAxis(mVerticalAxisInputName + (_playerNum + 1));
        mHorizontalInputValue = Input.GetAxis(mHorizontalAxisInputName + (_playerNum + 1));
        //Debug.Log(mHorizontalInputValue);

        // Check movement and change states according to it
        if (Mathf.Abs(mVerticalInputValue) > 0.1f || Mathf.Abs(mHorizontalInputValue) > 0.1f)
            state = State.Moving;
        else state = State.Idle;
    }
    protected void FireInput()
    {
        //fire shots
        if (Input.GetButton(mFireInputName + (_playerNum + 1)))
        {
            if(mTankShot.Fire())
            {
                PlaySFX(_clipShotFired);
            }
        }
    }


    protected void ChangeMovementAudio(AudioClip clip)
    {
        if(_movementSFX.clip != clip)
        {
            _movementSFX.clip = clip;
            _movementSFX.pitch = mOriginalPitch + Random.Range(-_pitchRange, _pitchRange);
            _movementSFX.Play();
        }
    }
    protected void PlaySFX(AudioClip clip)
    {
        _tankSFX.clip = clip;
        //_tankSFX.pitch = mOriginalPitch + Random.Range(-_pitchRange, _pitchRange);
        _tankSFX.Play();
    }

    // Physic update. Update regardless of FPS
    void FixedUpdate()
    {
        if (_ifHuman)
        {
            Move();
            Rotate();
        }
    }

    // Move the tank based on speed
    public void Move()
    {
        Vector3 moveVect = transform.forward * _moveSpeed * Time.deltaTime * mVerticalInputValue;
        mRigidbody.MovePosition(mRigidbody.position + moveVect);
    }

    // Rotate the tank
    public void Rotate()
    {
        float rotationDegree = _rotationSpeed * Time.deltaTime * mHorizontalInputValue;
        Quaternion rotQuat = Quaternion.Euler(0f, rotationDegree, 0f);
        mRigidbody.MoveRotation(mRigidbody.rotation * rotQuat);
    }

    public void TakeDamage(float damage)
    {
        if (mState != State.Inactive || mState != State.Death)
        {
            mHealth = Mathf.Clamp(mHealth-damage, 0, _maxHealth);

            if (mHealth > 0)
                state = State.TakingDamage;
            else
                state = State.Death;
        }
    }

    protected void Death()
    {
        PlaySFX(_clipTankExplode);
        StartCoroutine(ChangeState(State.Idle, 0.20f));
    }

    private void UpdateNearestFriend()
    {
        foreach(EnemyTank mTank in globalData.mEnemyTanks)
        {
            if (mTank == null)
                continue;
            else if (mTank.ifFriend == true)
            {
                if (nearestFriend == null)
                    nearestFriend = mTank;
                else
                {
                    //Already has a nearest friend tank and should offer a comparison
                    float distanceToCurrent = Vector3.Distance(mRigidbody.position, nearestFriend.transform.position);
                    float distanceToCandidate = Vector3.Distance(mRigidbody.position, mTank.transform.position);

                    if (distanceToCandidate < distanceToCurrent)
                        nearestFriend = mTank;
                }
            }
            else
                ;
        }
    }

    private void UpdateNearestEnemy()
    {
        foreach (EnemyTank mTank in globalData.mEnemyTanks)
        {
            if (mTank == null)
                continue;
            else if (mTank.ifFriend == false)
            {
                globalData.metEnemy = 0;
                if (nearestEnemy == null)
                    nearestEnemy = mTank;
                else
                {
                    //Already has a nearest enemy tank and should offer a comparison
                    float distanceToCurrent = Vector3.Distance(mRigidbody.position, nearestEnemy.transform.position);
                    float distanceToCandidate = Vector3.Distance(mRigidbody.position, mTank.transform.position);

                    if (distanceToCandidate < distanceToCurrent)
                        nearestEnemy = mTank;
                }
            }
            else
                ;
        }
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
        _inputIsEnabled = true;

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
                        ChangeMovementAudio(_clipIdle);
                        break;

                    case State.Moving:
                        ChangeMovementAudio(_clipMoving);
                        break;

                    case State.TakingDamage:
                        StartCoroutine(ChangeState(State.Idle, 0.1f));
                        break;

                    case State.Death:
                        globalData.score += -2.5f;
                        AddReward(-2.5f);
                        EndEpisode();
                        Death();
                        break;

                    case State.Inactive:
                        //gameObject.SetActive(false);
                        //dTankDestroyed.Invoke(this);
                        //mRigidbody.isKinematic = true;
                        //_inputIsEnabled = false;
                        //globalData.ifReset = true;
                        break;
                    default:
                        break;
                }

                mState = value;
            }
        }
    }
}
