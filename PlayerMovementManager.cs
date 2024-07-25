using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementManager
{
    float maxSpeed = 3.0f;
    float maxAcceleration = 10.0f;

    Rigidbody body;
    Vector3 velocity, desiredVelocity;

    public void Initialize()
    {
        body = GameManager.playerObj.GetComponent<Rigidbody>();
    }

    public void Update()
    {
        Vector2 playerInput;

        // update desired velocity
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
    }

    public void FixedUpdate()
    {
        // update velocity
        velocity = body.velocity;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        body.velocity = velocity;
    }
}
