using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public float autoRestoreHPRate = 10.0f;
    public Int32 initialMaxHP = 300;
    public Color initialBaseColor;
    public Material material;

    public Vector3 velocity;
    public Weapon weapon;
    public Int32 hp = 300;
    public Int32 maxHP = 300;
    public float m = 100.0f;
    public bool hittable = false;
    public float lastHitByEnemyTime = -10000.0f;
    public float availableAutoRestoreHP = 0.0f;

    public Player(int _index, GameObject _obj, PlayerInputManager _playerInputManager)
    {
        index = _index;
        obj = _obj;
        body = _obj.GetComponent<Rigidbody>();
        playerInputManager = _playerInputManager;
        weapon = new Weapon(index);
        material = obj.GetComponent<Renderer>().material;
        initialBaseColor = material.color;
    }

    public void Update()
    {
        AutoRestoreHP();
        UpdateHittableState();
        UpdateMass();
        playerInputManager.Update();
        Shoot();
        UpdateMaterial();
    }

    public void FixedUpdate()
    {
        UpdateVelocity();
    }

    public void UpdateHittableState()
    {
        if (GameManager.gameTime > lastHitByEnemyTime + hitProtectionDuration)
        {
            if (!hittable) weapon.SetLastShootTime(GameManager.gameTime);
            hittable = true;
        }
    }

    public void UpdateMass()
    {
        if (index == 0 && 
            GameManager.playerSkillManager.skills["Player1Skill0"].GetState() == 1)
        {
            m = 10000.0f;
        }
        else
        {
            if (hp > 200) m = 2.0f;
            else if (hp > 100) m = 1.5f;
            else if (hp > 0) m = 1.0f;
            else m = 0.5f;
        }

        GameManager.uiManager.UpdatePlayerMass(index, m);
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
        if (obj.transform.localPosition.y > 0.501f) maxSpeedChange *= 0.3f;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        body.velocity = velocity;
    }

    public void OnProcessPlayerReadbackData(ComputeCenter.PlayerDatum datum)
    {
        // 更新hp
        if (hittable) updateHP(datum.hpChange);

        // 更新速度
        Vector3 dV = new Vector3(datum.hitImpulse.x / 10000.0f, datum.hitImpulse.y / 10000.0f, datum.hitImpulse.z / 10000.0f);
        bool hitByEnemy = datum.hitByEnemy != 0 ? true : false;
        if (hittable)
        {
            if (hitByEnemy)
            {
                lastHitByEnemyTime = GameManager.gameTime;
                hittable = false;
                body.velocity = body.velocity * 0.2f + dV / m;

                float shakeForce = Math.Clamp(1.0f / m, 0.5f, 5.0f);
                GameManager.cameraMotionManager.ShakeByRotation(shakeForce);
            }
            else
            {
                body.velocity = body.velocity + dV / m;
            }
        }
    }

    public void OnProcessPlayerSkillReadbackData(ComputeCenter.PlayerSkillDatum datum)
    {
        updateHP(datum.player2Skill0HPRestoration / 2, true);
    }

    public void updateHP(int hpChange, bool canBreakBound = false)
    {
        if (canBreakBound)
        {
            hp = Mathf.Min(hp + hpChange, initialMaxHP);
        }
        else
        {
            hp = Mathf.Min(hp + hpChange, maxHP);
        }
        hp = Mathf.Max(hp, 0);
        maxHP = Mathf.Max((hp + 99) / 100 * 100, 0);
        GameManager.uiManager.UpdatePlayerHP(index, hp);
    }

    public void AutoRestoreHP()
    {
        if (hittable && body.position.y < 0.5001)
        {
            availableAutoRestoreHP += autoRestoreHPRate * GameManager.deltaTime;
        }
            
        if (availableAutoRestoreHP > 1.0f)
        {
            int hpchange = (int)MathF.Floor(availableAutoRestoreHP);
            availableAutoRestoreHP -= hpchange;
            updateHP(hpchange);
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
