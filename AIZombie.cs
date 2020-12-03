using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using UnityStandardAssets.Characters.FirstPerson;



public class AIZombie : MonoBehaviour
{
    public bool isDead = false;
    public int health = 50;
    public float wanderRadios = 10f;
    public GameObject fpsc;
    public float wanderSpeed = 1.25f;
    public float chaseSpeed = 5f;
    private Vector3 wanderPoint;
    private Collider[] cBody;
    private Rigidbody[] rBody;
    private NavMeshAgent agent;
    private bool isAware = false;
    private bool isAttacking = false;
    private float viewDistance = 8f;
    public float attackDistance;
    public float HitDistance;
    private Animator animator;
    public MeshRenderer meshRenderer;
    public AudioClip wanderingSound;
    public AudioClip ChaseSound;
    public int attackDamage = 30;
    public Transform sphereCastSpawn;
    private CapsuleCollider zombieCollider;
    public LayerMask playerLayer;
    private bool isTriggered;
    public bool isWating;
    public SkinnedMeshRenderer pants;
    public Material pantMaterials;
    public SkinnedMeshRenderer shirt;
    public Material[] shirtMaterials;
    private bool isHitted;
    private static int pantsIndex = 0;
    private int timerSeconds;
    private float remainingDistance;
    private float gameTimer;
    
    // Start is called before the first frame update
    void Start()
    {

        gameTimer = Time.time;

    



        remainingDistance = -1f;
        if (pantsIndex == 3)
            pantsIndex = 0;
        
            
        isWating = false;
        isTriggered = false;
        isHitted = false;
        /*       this.pants = this.transform.FindChild("RetopoPantsUV").transform.GetComponent<Material>();
               this.pants = pantMaterials;*/
        this.zombieCollider = GetComponent<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();
        wanderPoint = RandomWanderingPoint();
        animator = GetComponentInChildren<Animator>();
        cBody = GetComponentsInChildren<Collider>();
        rBody = GetComponentsInChildren<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        //ac = GetComponent<AudioClip>();
        
        
        foreach (Collider col in cBody)
        {
            if (!col.CompareTag("Zombie"))
                col.enabled = false;
        }
        foreach (Rigidbody rb in rBody)
        {
            rb.isKinematic = true;
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        gameTimer += Time.deltaTime;
        timerSeconds = (int)(gameTimer % 60);

        if (!zombieCollider)//dead
            return;
        if (isWating)
        {
            if (isTriggered)
                isWating = false;
            return;
        }

        if (health <= 0)
        {
            Die();

            return;
        }
       
        searchForPlayer();
        if (isAttacking)
        {
           // Attack(); called from Attack Event
           
            agent.speed = 0;
            animator.SetBool("Attack", true);
            animator.SetBool("Aware", false);
            animator.SetBool("Hitted", false);
        }
        else if (isAware || isTriggered)
        {
            print("Awara!");
            animator.SetBool("Aware", true);
            animator.SetBool("Attack", false);
            animator.SetBool("Hitted", false);
           
            agent.speed = chaseSpeed;
            if (remainingDistance != -1f)
            {
                if (remainingDistance - agent.remainingDistance < 0.01f && timerSeconds % 2 == 0)
                {
                    agent.SetDestination(new Vector3(agent.destination.x+0.1f,agent.destination
                        .y,agent.destination.z));
                }
            }
            if (agent.SetDestination(fpsc.transform.position))
            {
               
                print("player chase!");
            }


        }
        else
        {
            agent.speed = wanderSpeed;
            animator.SetBool("Attack", false);
            animator.SetBool("Aware", false);
            animator.SetBool("Hitted", false);

            wander();

        }
        remainingDistance = agent.remainingDistance;
    }
    public Vector3 RandomWanderingPoint()
    {
        //TODO

        AudioSource.PlayClipAtPoint(wanderingSound, transform.position,0.1f);
        Vector3 randomPoint = new Vector3();
        randomPoint = (UnityEngine.Random.insideUnitSphere * wanderRadios) + transform.position;
        NavMeshHit nHit;

        if (NavMesh.SamplePosition(randomPoint, out nHit, wanderRadios, NavMesh.AllAreas))
        {
            
            return new Vector3(nHit.position.x, transform.position.y, nHit.position.z);

        }
        print("Xero!");
        return Vector3.zero;
    }
    public void wander()
    {
        if (remainingDistance != -1f)
        {
            if (remainingDistance - agent.remainingDistance < 0.05f&&timerSeconds%10==0)
            {
                wanderPoint = RandomWanderingPoint();

            }
        }
        if (Vector3.Distance(transform.position, wanderPoint) < 0.5f)
        {
            wanderPoint = RandomWanderingPoint();
        }
        else
        {

            agent.SetDestination(wanderPoint);
        }

    }
    public void searchForPlayer()
    {
        if (Vector3.Distance(fpsc.transform.position, transform.position) < attackDistance)
        {
            
            isAttacking = true;
            isAware = false;
        }
        else if(animator.GetBool("Aware")&& Vector3.Distance(fpsc.transform.position, transform.position) + 3f < viewDistance)
        {
            return;
        }
        else if (Vector3.Distance(fpsc.transform.position, transform.position) < viewDistance)
        {
            if (isAware == false)
            {
                AudioSource.PlayClipAtPoint(ChaseSound, transform.position);
                isAware = true;
            }
            isAttacking = false;

        }
        else
        {
            isAware = false;
            isAttacking = false;
        }
    }
    public void OnHit(int damage)
    {
        agent.speed = 0f;
        agent.destination = transform.position;
        health -= damage;
       /* this.isAware = false;
        this.isAttacking = false;*/
        this.isHitted = true;
        /*animator.SetBool("Aware", false);
        animator.SetBool("Attack", false);*/
        animator.SetBool("Hitted", true);

    }
    public void Die()
    {
        agent.speed = 0;
        animator.enabled = false;

        foreach (Collider col in cBody)
        {
            col.enabled = true;

        }
        foreach (Rigidbody rb in rBody)
        {
            rb.isKinematic = false;
        }
        Destroy(this.zombieCollider);
        isDead = true;
    }
    public void Attack()
    {
        agent.speed = 0;
        RaycastHit rHit;
        if (Physics.SphereCast(sphereCastSpawn.position, 0.5f, sphereCastSpawn.TransformDirection(Vector3.forward), out rHit, playerLayer))
        {
            if (Vector3.Distance(fpsc.transform.position, transform.position) < attackDistance)
            {
                fpsc.transform.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
            }
        }
    }
    public void triggerZombie()
    {
        this.isTriggered = true;
    }
    public void waitZombie()
    {
        this.isWating = true;
    }
 
}
