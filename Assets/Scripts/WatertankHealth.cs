using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatertankHealth : MonoBehaviour
{
    public float _maxHealth = 300;
    public float mHealth;

    //delegate
    public delegate void GotDestroyed(WatertankHealth target);
    public GotDestroyed dGotDestroyed;

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

    // Health
    public Slider HPStrip;
    public Image fill;
    private float rate = 0.50f;

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
        mRigidbody = GetComponent<Rigidbody>();
        mHealth = _maxHealth;
        InitHP();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHP();
        switch (state)
        {
            case State.Idle:
            
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
        StartCoroutine(ChangeState(State.Inactive, 1f));
    }

    public void Restart(Vector3 pos, Quaternion rot)
    {
        // Reset position, rotation and health
        transform.position = pos;
        transform.rotation = rot;
        mHealth = _maxHealth;

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

                    case State.TakingDamage:
                        StartCoroutine(ChangeState(State.Idle, 1f));
                        break;

                    case State.Death:
                        Death();
                        break;

                    case State.Inactive:
                        gameObject.SetActive(false);
                        dGotDestroyed.Invoke(this);
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
