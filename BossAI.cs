using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BossAI : MonoBehaviour
{
    public int startAttackTimer;
    private bool getTierd = false;
    public int attackingTimes= 3;
    public static float CHASE_SPEED = 5f;
    public static float ATTACK_SPEED = 4f;
    public static float SHOUT_SPEED = 0f;
    public static float ATTACK_DISTANCE = 2f;
    private const float SHOUT_TIME = 4f;
    private float hitDistance = 3f;
    public LayerMask playerLayer;
    private AudioSource AS;

    private int waitCountTime;
    private int hittedCountTime;

    private int attackCountTime;
    private int shoutCountTime;
    private int chaseCountTime;
    private int jumpCountTime;
    public Canvas youWinCanvas;

    private NavMeshAgent agent;
    private delegate void State(); //create delegate
    private State stateMethod; //create holder for delegate
    private int HitsPerPhase = 5;
    bool canBeAttacked = false;
    public float gameTimer;
    private int timerSeconds = 0;

    public AudioClip waitingSound;
    public AudioClip hittedSound;
    public AudioClip attackSound;
    public GameObject player;
    public int health = 4;
    private int hitsToGetHit = 5;
    private const int regularDamage = 30;
    private const int jumpDamage = 50;
    public AudioClip chaseSound;
    private Animator anim;

    /*
     * States:
     * Attacking - boss is attacking, is not vulnerable
     * Waiting - boss is not attacking, is vulnerable
     */

    // Use this for initialization
    void Start()
    {
        this.AS = GetComponent<AudioSource>();
        youWinCanvas.enabled = false;
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(player.transform.position);
        anim = GetComponentInChildren<Animator>();
        
        stateMethod = new State(EnterStateShout);
        gameTimer = Time.time;

    }


    void FixedUpdate()
    {
        gameTimer += Time.deltaTime;
        timerSeconds = (int)(gameTimer % 60);
       
        stateMethod();
    }
   

   
    private void EnterStateChase()
    {
        AS.Stop();
        AS.PlayOneShot(chaseSound);
        chaseCountTime = timerSeconds;
        print("Chase State");
        agent.destination = player.transform.position;
        agent.speed = CHASE_SPEED; 
        setAnimTriggers("Chase");
        canBeAttacked = false;
        stateMethod = new State(Chase);
        stateMethod(); //start in same frame (comment out to delay)
    }
    private void EnterStateAttacking()
    {
        AS.Stop();
        AS.PlayOneShot(attackSound);
        attackCountTime = timerSeconds;
        attackingTimes--;

        print("Attack state enter");
        startAttackTimer= timerSeconds;

        //    attackSound.Play();
        agent.destination = transform.position;
        agent.speed = SHOUT_SPEED;
        setAnimTriggers("Attack");
        canBeAttacked = false;
        stateMethod = new State(Attacking);
        stateMethod(); //start in same frame (comment out to delay)
    }
    private void EnterStateWaiting()
    {
        AS.Stop();
        waitCountTime = timerSeconds;
        AS.PlayOneShot(waitingSound);
        setAnimTriggers("Waiting");
        agent.destination = transform.position;
        agent.speed = SHOUT_SPEED; canBeAttacked = true;
        stateMethod = new State(Waiting);
        stateMethod();
    }
    private void EnterStateHitted()
    {
        AS.Stop();
        AS.PlayOneShot(hittedSound);
        hittedCountTime = timerSeconds;
        agent.destination = transform.position;
        agent.speed = SHOUT_SPEED;
        setAnimTriggers("Hitted");
        print("health: " + health);
        health--;
        canBeAttacked = false;
        stateMethod = new State(Hitted);
        stateMethod();

    }
    private void EnterStateShout()
    {
        AS.Stop();
        AS.PlayOneShot(attackSound);
        shoutCountTime = timerSeconds;
        agent.speed = SHOUT_SPEED;
        agent.destination = transform.position;
        setAnimTriggers("Shout");
        canBeAttacked = false;
        stateMethod = new State(Shout);
        stateMethod();
    }
    private void EnterStateDead()
    {
        youWinCanvas.enabled = true;
        agent.speed = SHOUT_SPEED;
        agent.destination = transform.position;
        setAnimTriggers("Dead");
        canBeAttacked = false;
        stateMethod = new State(Dead);
        stateMethod();
    }
    private void EnterStateJump()
    {
        AS.Stop();
        jumpCountTime = timerSeconds;
        agent.speed = SHOUT_SPEED;
        agent.destination = transform.position;
        setAnimTriggers("Jump");
        canBeAttacked = false;
        stateMethod = new State(Jump);
        stateMethod();
    }
    private void Waiting()
    {

        if ((timerSeconds - waitCountTime) % 4 == 0 && !AS.isPlaying)
            AS.PlayOneShot(waitingSound);

        if (timerSeconds - waitCountTime >= 5)
        {
            stateMethod = new State(EnterStateChase);
            stateMethod();
        }
        
    }
    private void Hitted()
    {
        if ((timerSeconds - hittedCountTime) % 4 == 0 && !AS.isPlaying)
            AS.PlayOneShot(hittedSound);
        if (health <= 0)
        {
            print("dead");
            stateMethod = new State(EnterStateDead);
            stateMethod();
        }
        else if (timerSeconds - hittedCountTime > 1)
        {
            stateMethod = new State(EnterStateShout);
            stateMethod();
        }
    }
    private void Chase()
    {
        agent.destination = player.transform.position;
        if((timerSeconds - chaseCountTime)%2==0&&!AS.isPlaying)
           AS.PlayOneShot(chaseSound);

        if (Vector3.Distance(transform.position, player.transform.position) < ATTACK_DISTANCE)
        {
            stateMethod = new State(EnterStateAttacking);
            stateMethod();
        }
       else if (timerSeconds - chaseCountTime >= 20)
        {
            Debug.Log("Trasmition to wait from chase");
            stateMethod = new State(EnterStateWaiting);
            stateMethod();
        }
    }

    private void Attacking()
    {
        if ((timerSeconds - attackCountTime) % 2 == 0 && !AS.isPlaying)
            AS.PlayOneShot(attackSound);
        if (timerSeconds - attackCountTime >= 2)
        {
            if (attackingTimes == 0)
            {
                Debug.Log("Trasmition to wait from attack");
                attackingTimes = 2;
                //Change phase
                stateMethod = new State(EnterStateWaiting);
                stateMethod();
            }
            else if(Vector3.Distance(transform.position, player.transform.position) < ATTACK_DISTANCE)
            {
                Debug.Log("attak again");
                stateMethod = new State(EnterStateAttacking);
                stateMethod();
            }
            else
            {
                Debug.Log("Trasmition to chase");
                stateMethod = new State(EnterStateChase);
                stateMethod();
            }
        }
    }
    private void Dead()
    {
        StartCoroutine(GameOver());
    }
    IEnumerator GameOver()
    {
        
        yield return new WaitForSeconds(5);

        SceneManager.LoadScene("MainMenu");

    }
    public void Attack()
    {
        RaycastHit rHit;
        if (Physics.SphereCast(transform.position, 3f, transform.TransformDirection(Vector3.forward), out rHit, playerLayer))
        {
            if (Vector3.Distance(player.transform.position, transform.position) < hitDistance)
            {
                print("player take damage");
                player.transform.GetComponent<PlayerHealth>().TakeDamage(regularDamage);
            }
        }
    }

    private void Shout()
    {
        if ((timerSeconds - shoutCountTime) % 4 == 0 && !AS.isPlaying)
            AS.PlayOneShot(attackSound);
        if (timerSeconds - shoutCountTime >= 4)
        {
            if (health <= 2)
            {
                stateMethod = new State(EnterStateJump);

            }
            else
            {
                stateMethod = new State(EnterStateChase);
            }
            stateMethod();
        }

    }
    private void Jump()
    {
        if (timerSeconds - jumpCountTime >= 1)
        {

            stateMethod = new State(EnterStateChase);
            stateMethod();
        }

    }
    public void JumpDamage()
    {
        RaycastHit rHit;
        if (Physics.SphereCast(transform.position, 3f, transform.TransformDirection(Vector3.forward), out rHit, playerLayer))
        {
            if (Vector3.Distance(player.transform.position, transform.position) < 10f)
            {
                print("player take damage");
                player.transform.GetComponent<PlayerHealth>().TakeDamage(jumpDamage);
            }
        }
    }
    public void OnHit(int damage)
    {
        if (canBeAttacked)
        {
            hitsToGetHit--;
            if (hitsToGetHit <= 0)
            {
                hitsToGetHit = 5;
                stateMethod = new State(EnterStateHitted);
                stateMethod();
            }
        }
    }
    private void setAnimTriggers(string state)
    {
        switch (state)
        {
            case "Chase":
                anim.SetBool("Chase", true);
                anim.SetBool("Attack", false);
                anim.SetBool("Waiting", false);
                anim.SetBool("Hitted", false);
                anim.SetBool("Shout", false);
                anim.SetBool("Jump", false);
                break;
            case "Attack":
                anim.SetBool("Chase", false);
                anim.SetBool("Attack", true);
                anim.SetBool("Waiting", false);
                anim.SetBool("Hitted", false);
                anim.SetBool("Shout", false);
                anim.SetBool("Jump", false);
                break;
            case "Waiting":
                anim.SetBool("Chase", false);
                anim.SetBool("Attack", false);
                anim.SetBool("Waiting", true);
                anim.SetBool("Hitted", false);
                anim.SetBool("Shout", false);
                anim.SetBool("Jump", false);
                break;
            case "Hitted":
                anim.SetBool("Chase", false);
                anim.SetBool("Attack", false);
                anim.SetBool("Waiting", false);
                anim.SetBool("Hitted", true);
                anim.SetBool("Shout", false);
                break;
            case "Shout":
                anim.SetBool("Chase", false);
                anim.SetBool("Attack", false);
                anim.SetBool("Waiting", false);
                anim.SetBool("Hitted", false);
                anim.SetBool("Shout", true);
                anim.SetBool("Jump", false);
                break;
            case "Dead":
                anim.SetBool("Chase", false);
                anim.SetBool("Attack", false);
                anim.SetBool("Waiting", false);
                anim.SetBool("Hitted", false);
                anim.SetBool("Shout", false);
                anim.SetBool("Dead", true);
                anim.SetBool("Jump", false);
                break;
            case "Jump":
                anim.SetBool("Chase", false);
                anim.SetBool("Attack", false);
                anim.SetBool("Waiting", false);
                anim.SetBool("Hitted", false);
                anim.SetBool("Shout", false);
                anim.SetBool("Dead", false);
                anim.SetBool("Jump", true);
                break;
        }
    }
}
