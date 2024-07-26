using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public GameObject obj;
    Rigidbody body;
    public Weapon weapon;
    public PlayerInputManager playerInputManager;
    Vector3 velocity, desiredVelocity;
    float maxSpeed = 4.0f;
    float maxAcceleration = 10.0f;

    public Player(GameObject _obj, PlayerInputManager _playerInputManager)
    {
        obj = _obj;
        body = _obj.GetComponent<Rigidbody>();
        playerInputManager = _playerInputManager;
        weapon = new BasicWeapon(GameManager.bulletManager);
    }

    public void Update()
    {
        playerInputManager.Update();
        UpdateDesiredVelocity();
        Vector3 shootDir = playerInputManager.GetShootDir(obj.transform.localPosition);
        using (new BallGameUtils.Profiler("Player.Weapon.Shoot"))
        {
            weapon.Shoot(obj.transform.localPosition, shootDir);
        }
            
    }

    public void FixedUpdate()
    {
        UpdateVelocity();
    }

    public void UpdateDesiredVelocity()
    {
        Vector2 playerInput = playerInputManager.GetPlayerMovementInput();
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
    }

    public void UpdateVelocity()
    {
        velocity = body.velocity;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        body.velocity = velocity;
    }
}
