using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyMovement : MonoBehaviour
{
    [SerializeField]
    float maxSpeed = 3.0f;

    [SerializeField]
    float maxAcceleration = 10.0f, maxAirAcceleration = 5f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2.0f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 1;

    [SerializeField, Range(0f, 1f)]
    float airJumpMinimumSpeedRatio = 0.7f;

    Rigidbody body;
    Vector3 velocity, desiredVelocity;
    bool desiredJump;
    bool onGround;
    int jumpPhase;


    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        velocity = body.velocity;

        if (onGround)
        {
            jumpPhase = 0;
        }

        float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        body.velocity = velocity;
    }

    void Jump()
    {
        if (onGround || jumpPhase <= maxAirJumps)
        {
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            velocity.y = Mathf.Clamp(velocity.y + jumpSpeed, airJumpMinimumSpeedRatio * jumpSpeed, jumpSpeed);
            jumpPhase += 1;
            onGround = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            onGround |= normal.y > 0.2f;
        }
    }
}
