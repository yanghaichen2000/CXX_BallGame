using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class Player
{
    public int index;
    public GameObject obj;
    public Rigidbody body;
    public PlayerInputManager playerInputManager;
    public float maxSpeed = 4.0f;
    public float maxAcceleration = 10.0f;
    public float hitProtectionDuration = 3.0f;
    public Color initialBaseColor;
    public Material material;

    public Vector3 velocity;
    public Weapon weapon;
    public Int32 hp = 300;
    public float m = 100.0f;
    public bool hittable = false;
    public float lastHitByEnemyTime = -10000.0f;

    public Player(int _index, GameObject _obj, PlayerInputManager _playerInputManager)
    {
        index = _index;
        obj = _obj;
        body = _obj.GetComponent<Rigidbody>();
        playerInputManager = _playerInputManager;
        weapon = new Weapon(index);
        material = obj.GetComponent<Renderer>().material;
        initialBaseColor = material.color;

        if (index == 1) hp = 500;
    }

    public void Update()
    {
        UpdateBasicCondition();
        playerInputManager.Update();
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
            if (!hittable) weapon.SetLastShootTime(GameManager.gameTime);
            hittable = true;
        }

        if (hp > 200) m = 2.0f;
        else if (hp > 100) m = 1.5f;
        else if (hp > 0) m = 1.0f;
        else m = 0.5f;
    }

    public void Shoot()
    {
        if (hittable)
        {
            Vector3 shootDir = playerInputManager.GetShootDir(obj.transform.localPosition);
            weapon.Shoot(obj.transform.localPosition, shootDir);
        }
    }

    public void UpdateVelocity()
    {
        velocity = body.velocity;

        Vector2 playerInput = playerInputManager.GetPlayerMovementInput();
        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        body.velocity = velocity;
    }

    public void OnProcessReadbackData(ComputeCenter.PlayerDatum datum)
    {
        if (hittable) hp += datum.hpChange;
        GameManager.uiManager.UpdatePlayerHP(index, hp);

        Vector3 dV = new Vector3(datum.hitImpulse.x / 10000.0f, datum.hitImpulse.y / 10000.0f, datum.hitImpulse.z / 10000.0f);
        bool hitByEnemy = datum.hitByEnemy != 0 ? true : false;
        if (hittable)
        {
            if (hitByEnemy)
            {
                lastHitByEnemyTime = GameManager.gameTime;
                hittable = false;
                body.velocity = body.velocity * 0.2f + dV / m;
            }
            else
            {
                body.velocity = body.velocity + dV / m;
            }
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
