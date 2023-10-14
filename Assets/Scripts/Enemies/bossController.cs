using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class bossController : MonoBehaviour
{
    public Transform player;

    bool isHunting;

    public float alertRange, attackRange;
    public bool canShoot;
    public LayerMask playerMask;
    NavMeshAgent agent;
    Animator anim;

    public float health;

    public GameObject healthText;
    public int bodyDamage;
    public int timeCounter;
    public Slider sliderHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        sliderHealth.maxValue = health;
        sliderHealth.gameObject.SetActive(false);

    }
    void Update()
    {
        if (health > 0)
        { //Chequea Estado
            checkState();
        }
    }

    private void checkState()
    {
        //detecta Cercania
        isHunting = Physics.CheckSphere(transform.position, alertRange, playerMask);
        float distance = Vector3.Distance(transform.position, player.position);

        // El enemigo te sigue
        if (isHunting && VisionLine())
        {
            anim.SetBool("Hunting", true);
            anim.SetFloat("velocity", agent.velocity.magnitude);

            agent.destination = player.position;

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
            if (hitInfo.collider.CompareTag("HitboxPlayer"))
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
        if (!canShoot)
        {
            anim.SetBool("Attacking", true);
            BodyDamage();

        }
    }

    private void BodyDamage()
    {
        if (timeCounter % 120 == 0)
        {
            healthText.GetComponent<HealthInfo>().TakeDamage(bodyDamage);
            timeCounter = 1;
        }
        timeCounter++;
    }

    public void GetHurt(float damage)
    {
        if (health > 0)
        {
            //Enemigo sufre da�o
            sliderHealth.gameObject.SetActive(true);
            health -= damage;
            sliderHealth.value = health;
            StartCoroutine(ResetHurtState());
        }
    }

    private IEnumerator ResetHurtState()
    {
        anim.SetBool("Hurt", true);
        agent.enabled = false;
        yield return new WaitForSeconds(0.65f);
        agent.enabled = true;
        anim.SetBool("Hurt", false);

        //Enemigo muere
        if (health <= 0)
        {
            sliderHealth.gameObject.SetActive(false);
            bool randomDead = UnityEngine.Random.Range(0, 2) == 0;
            if (randomDead)
            {
                anim.SetBool("Dead", true);
                anim.SetBool("randomDead", true);
            }
            else
            {
                anim.SetBool("Dead", true);
                anim.SetBool("randomDead", false);
            }
            yield return new WaitForSeconds(4);
            agent.enabled = false;

            for (int i = 0; i < 40; i++)
            {
                transform.position -= Vector3.up * 0.05f;
                yield return new WaitForSeconds(0.05f);
            }
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alertRange);


        if (canShoot)
            Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (!canShoot)
            Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

}
