using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float velocity = 12f;
    public int mouseSensitivity ;
    Rigidbody rig;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rig = GetComponent<Rigidbody>();
        mouseSensitivity = 120;
        //mouseSensitivity = OptionMenu.instance.sensivility;
    }


    void FixedUpdate()
    {
        Move();
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
        }

        // Rotaci�n con rat�n
        float movementMouseHorizontal = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * movementMouseHorizontal, Space.World);
    }
}
