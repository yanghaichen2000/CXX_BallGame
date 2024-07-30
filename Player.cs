using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public GameObject obj;
    public Rigidbody body;
    public PlayerInputManager playerInputManager;
    public float maxSpeed = 4.0f;
    public float maxAcceleration = 10.0f;
    public float hitProtectionDuration = 3.0f;
    public Color initialBaseColor;
    public Material material;

    public Vector3 velocity, desiredVelocity;
    public Weapon weapon;
    public Int32 hp = 100;
    public bool hittable = false;
    public float lastHitByEnemyTime = -10000.0f;

    public Player(GameObject _obj, PlayerInputManager _playerInputManager)
    {
        obj = _obj;
        body = _obj.GetComponent<Rigidbody>();
        playerInputManager = _playerInputManager;
        weapon = new Shotgun(GameManager.bulletManager);
        material = obj.GetComponent<Renderer>().material;
        initialBaseColor = material.color;
    }

    public void Update()
    {
        UpdateBasicCondition();
        playerInputManager.Update();
        UpdateDesiredVelocity();
        Shoot();
        UpdateMaterial();
    }

    public void FixedUpdate()
    {
        UpdateVelocity();
    }

    public void UpdateBasicCondition()
    {
        if (GameManager.gameTime > lastHitByEnemyTime + hitProtectionDuration)
        {
            hittable = true;
        }
    }

    public void Shoot()
    {
        if (hittable)
        {
            Vector3 shootDir = playerInputManager.GetShootDir(obj.transform.localPosition);
            weapon.Shoot(obj.transform.localPosition, shootDir);
        }
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

    public void OnProcessReadbackData(ComputeCenter.PlayerDatum datum)
    {
        Vector3 dV = datum.hitMomentum;
        int newHp = datum.hp;

        bool hit = false;
        if (dV.magnitude > 0.0001f) hit = true;

        if (hit && hittable)
        {
            lastHitByEnemyTime = GameManager.gameTime;
            hittable = false;
            body.velocity = body.velocity * 0.2f + dV;
            hp = newHp;
        }
    }

    public void UpdateMaterial()
    {
        if (!hittable)
        {
            float time = (GameManager.gameTime - lastHitByEnemyTime);
            if (time - Math.Floor(time) < 0.5)
                material.color = initialBaseColor * new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
            else
                material.color = initialBaseColor;
        }
        else
        {
            material.color = initialBaseColor;
        }
    }

    public Vector3 GetPos()
    {
        return obj.transform.localPosition;
    }
}
