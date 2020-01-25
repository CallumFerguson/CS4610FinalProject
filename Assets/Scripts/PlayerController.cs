using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform ship;
    Rigidbody rb;

    float speed = 25f;

    public float rotation = 0;
    float modelHeight = 0.4f;
    float lastGroundedTime = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0, 0, speed);
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //    transform.position += new Vector3(0, 10f, 0);

        RaycastHit hit;
        if (Physics.Raycast(transform.position + new Vector3(0, 5, 0), Vector3.down, out hit, 5 + modelHeight + 0.1f)/* && hit.distance - 5 <= modelHeight*/)
        {
            rotation += Input.GetAxis("Horizontal") * Time.deltaTime * 360f;

            ship.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(new Vector3(0, rotation, 0));
            rb.velocity = ship.transform.forward;

            transform.position = hit.point + new Vector3(0, modelHeight, 0);

            lastGroundedTime = Time.time;
        }
        else
        {
            ship.transform.rotation = Quaternion.LookRotation(rb.velocity);
        }

        rb.velocity = rb.velocity.normalized * speed;
    }
}