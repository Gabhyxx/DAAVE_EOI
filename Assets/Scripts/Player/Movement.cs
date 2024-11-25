using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public static Movement instance;

    public float velocity = 12f;
    public int mouseSensitivity ;
    public int dashForce;
    public int timeToDash;

    private bool canDash = true;
    private Rigidbody rig;
    public bool FirstTime;
    public bool allDead;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rig = GetComponent<Rigidbody>();
        mouseSensitivity = OptionMenu.instance.sensivility;
    }

    private void Update()
    {
        Move();
        if (!FirstTime) CheckAllDead();
    }

    private void Move()
    {
        // Movimiento con teclas
        float horizontalMove = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");
        Vector3 movement = transform.right * horizontalMove + transform.forward * verticalMove;

        if (movement != Vector3.zero)
        {
            rig.velocity = movement * velocity* Time.deltaTime + Vector3.up * rig.velocity.y;
            //transform.Translate(movement * velocity * Time.deltaTime, Space.World);
            if (Input.GetKeyDown(KeyCode.Space) && canDash)
            {
                Dash();
            }
        }

        // Rotaci�n con rat�n
        float movementMouseHorizontal = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * movementMouseHorizontal, Space.World);
    }

    private void Dash()
    {
        canDash = false;
        rig.velocity = Vector3.zero;
        rig.AddForce(transform.forward * dashForce, ForceMode.Impulse);
        rig.velocity = Vector3.zero; 
        StartCoroutine(ResetDashCooldown());
    }

    public void CheckAllDead()
    {
        if (transform.position.x>102)
        {
            allDead = true;
            
            StartCoroutine(CoroutineAllDead());

            FirstTime = true;
        }
    }

    IEnumerator CoroutineAllDead()
    {
        yield return new WaitForSeconds(10f);
        allDead = false;
    }

    private IEnumerator ResetDashCooldown()
    {
        yield return new WaitForSeconds(timeToDash);
        canDash = true;
    }

    public void ApplyForce(Vector3 force, int forceImpulse)
    {
        rig.AddForce(force * forceImpulse, ForceMode.Impulse);
        rig.velocity = Vector3.zero;
    }

}
