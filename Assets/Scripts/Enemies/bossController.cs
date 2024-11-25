using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class bossController : MonoBehaviour
{
    public static bossController instance; 

    public Transform player;

    int counterDead;

    public float attackRange;
    public LayerMask playerMask;
    NavMeshAgent agent;
    Animator anim;

    public float health;

    public GameObject healthText;
    public int bodyDamage;
    public int timeCounter;
    public Slider sliderHealth;

    private bool isRecentlyHurt = false;
    private float lastHurtTime;

    private bool isDying = false;
    public bool allZombieDead = false;
    private bool oleade3Begin = false;

    public List<GameObject> oleade3;

    public bool bossEncounter;

    public AudioSource getHurt;
    public AudioSource punch;
    public AudioSource growl;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        sliderHealth.maxValue = health;
        sliderHealth.gameObject.SetActive(false);

    }

    void Update()
    {
        if (health > 0) checkState(); //Chequea estado
        checkSanity();
    }

    private void checkSanity()
    {
        if ((isRecentlyHurt && Time.time - lastHurtTime < 1f && counterDead ==0)) anim.SetBool("Hurt", true);
        else
        {
            anim.SetBool("Hurt", false);
            isRecentlyHurt = false;
        }
    }

    private void checkState()
    {
        //detecta Cercania
        float distance = Vector3.Distance(transform.position, player.position);

        // El enemigo te sigue
        if (VisionLine())
        {
            bossEncounter = true;
            if (!oleade3Begin && counterDead == 2) Generate(oleade3);

            anim.SetBool("Hunting", true);
            anim.SetFloat("velocity", agent.velocity.magnitude);

            if (agent.enabled) agent.destination = player.position;

            if (Vector3.Distance(transform.position, player.position) <= attackRange)
            {
                Attacking();
            }
            else anim.SetBool("Attacking", false);
        }

        // El enemigo deja de seguirte
        if (agent.destination == transform.position)
        {
            anim.SetBool("Hunting", false);
        }
    }

    public void Generate(List<GameObject> numberOleade)
    {
        oleade3Begin = true;
        foreach (GameObject zombie in numberOleade)
        {
            if (zombie != null && !zombie.activeSelf)
            {
                zombie.SetActive(true);
            }
        }
    }

    bool VisionLine()
    {


        Vector3 direction = (player.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, player.position);

        Debug.DrawRay(transform.position, direction * distance, Color.red);


        RaycastHit hitInfo;

        // Lanza un rayo desde la posici�n del enemigo en la direcci�n del objetivo
        if (Physics.Raycast(transform.position, direction, out hitInfo, distance, ~LayerMask.GetMask("Trigger"), QueryTriggerInteraction.Ignore))
        {
            // Si el rayo golpea al player devuelve true
            if (hitInfo.collider.CompareTag("Player"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

    }

    private void Attacking()
    {
        agent.velocity = Vector3.zero;
        anim.SetBool("Attacking", true);
        BodyDamage();
    }

    private void BodyDamage()
    {
        if (timeCounter % 60 == 0)
        {

            punch.Play();
            healthText.GetComponent<HealthInfo>().TakeDamage(bodyDamage);
            timeCounter = 1;
            

        }
        timeCounter++;
    }

    private void Push()
    {
        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f; 
        direction.Normalize();

        Movement.instance.ApplyForce(direction, 15);
    }

    public void GetHurt(float damage)
    {
        if (health > 0)
        {
            // Enemigo sufre da�o
            sliderHealth.gameObject.SetActive(true);
            if (!getHurt.isPlaying) getHurt.Play();
            health -= damage;
            sliderHealth.value = health;

            // Guardar el tiempo en el que fue herido
            lastHurtTime = Time.time;
            isRecentlyHurt = true;

            StartCoroutine(ResetHurtState());
        }
    }


    private IEnumerator ResetHurtState()
    {
        if ( health > 0 && counterDead == 0) {
            anim.SetBool("Hurt", true);
            agent.enabled = false;
            yield return new WaitForSeconds(2f);
            agent.enabled = true;
            anim.SetBool("Hurt", false);
        }


        //Enemigo muere
        if (health <= 0 && !isDying)
        {
            StartCoroutine(allDead());
            if (counterDead < 2)
            {
                isDying = true;
                agent.enabled = false;
                anim.SetBool("Dead", true);
                yield return new WaitForSeconds(5);
                anim.SetBool("revive", true);

                yield return new WaitForSeconds(2);

                // Empujamos al jugador
                growl.Play();

                Vector3 direction = player.transform.position - transform.position;
                direction.y = 0f;
                direction.Normalize();
                if (counterDead == 0) Movement.instance.ApplyForce(direction, 15);
                if (counterDead == 1) Movement.instance.ApplyForce(direction, 30);

                yield return new WaitForSeconds(1);

                if (counterDead == 0)
                {
                    health = 100;
                    sliderHealth.maxValue = health;
                    sliderHealth.value = health;
                    agent.speed = 16;
                    bodyDamage = 25;
                }
                if (counterDead == 1)
                {
                    health = 1;
                    sliderHealth.maxValue = health;
                    sliderHealth.value = health;
                    agent.speed = 18;
                    bodyDamage = 30;
                }

                anim.SetBool("Dead", false);
                anim.SetBool("revive", false);
                counterDead++;
                agent.enabled = true;
                isDying = false;

            }


            else
            {
                anim.SetBool("Dead", true);
                yield return new WaitForSeconds(8);
                sliderHealth.gameObject.SetActive(false);

                agent.enabled = false;

                for (int i = 0; i < 40; i++)
                {
                    transform.position -= Vector3.up * 0.05f;
                    yield return new WaitForSeconds(0.05f);
                }
                SceneManager.LoadScene(2, LoadSceneMode.Single);
                Destroy(gameObject);
                
            }
        }
    }

    private IEnumerator allDead()
    {


        allZombieDead = true;
        yield return new WaitForSeconds(2f);
        allZombieDead = false;
    }

    private void Smash()
    {
        agent.enabled = false;
        if (counterDead == 0) cameraMovement.instance.ShakeVertical(1.5f, .5f);
        if (counterDead == 1) cameraMovement.instance.ShakeVertical(2.5f, .5f);
        agent.enabled = true;
    }

    private void OnDrawGizmos()
    {

            Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Bullet"))
        {
            GetHurt(other.gameObject.GetComponent<ProjectileBullet>().damage);
        }
        else if (other.CompareTag("Arrow"))
        {
            GetHurt(other.gameObject.GetComponent<ArrowBullet>().damage);
        }
    }

}
