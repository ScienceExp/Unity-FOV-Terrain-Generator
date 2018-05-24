using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float walkSpeed=100;

    Rigidbody rb;
    Vector3 moveDirection;
    //Vector3 moveDirection { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

	// Update is called once per frame
	void Update () {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");
        moveDirection = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;
    }

    private void FixedUpdate()
    {
        //todo add back in Y component if jump needed
        //Vector3 vy = new Vector3(0, rb.velocity.y, 0);
        rb.velocity = moveDirection * walkSpeed * Time.deltaTime;
        //rb.velocity += vy;
    }
}
