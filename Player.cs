using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class AllLevelPlayerData
{
    public PlayerWeaponDatum[] player1data;
    public PlayerWeaponDatum[] player2data;
    public int[] levelExpList;

    public AllLevelPlayerData()
    {
        levelExpList = new int[6] { 2, 5, 10, 20, 35, 60 };
        InitializePlayer1WeaponData();
        InitializePlayer2WeaponData();
    }

    public void InitializePlayer1WeaponData()
    {
        player1data = new PlayerWeaponDatum[5];

        player1data[0] = PlayerWeaponDatumSample.player1Initial;

        player1data[1] = player1data[0];
        player1data[1].shootInterval *= 0.75f;

        player1data[2] = player1data[1];
        player1data[2].extraBulletsPerSide *= 2;
        player1data[2].angle /= 2;

        player1data[3] = player1data[2];
        player1data[3].shootInterval *= 0.75f;

        player1data[4] = player1data[3];
        player1data[4].extraBulletsPerSide *= 2;
        player1data[4].angle /= 2;
    }

    public void InitializePlayer2WeaponData()
    {
        player2data = new PlayerWeaponDatum[5];

        player2data[0] = PlayerWeaponDatumSample.player2Initial;

        player2data[1] = player2data[0];
        player2data[1].shootInterval *= 0.75f;

        player2data[2] = player2data[1];
        player2data[2].extraBulletsPerSide += 1;
        player2data[2].shootInterval *= 1.5f;
        player2data[2].angle = 3.0f;

        player2data[3] = player2data[2];
        player2data[3].shootInterval *= 0.75f;

        player2data[4] = player2data[3];
        player2data[4].shootInterval *= 0.75f;
    }

    public Weapon GetWeapon(int player, int level)
    {
        if (player == 0)
        {
            level = Math.Min(level, player1data.Length - 1);
            return new Weapon(player, player1data[level]);
        }
        else
        {
            level = Math.Min(level, player2data.Length - 1);
            return new Weapon(player, player2data[level]);
        }
    }

    public int GetCurrentLevel(int exp)
    {
        int lv = 0;
        while (lv < levelExpList.Length && exp >= levelExpList[lv])
        {
            lv++;
        }
        return lv;
    }

    public int GetLevelExp(int lv)
    {
        if (lv == 0)
        {
            return 0;
        }
        else if (lv < levelExpList.Length)
        {
            return levelExpList[lv - 1];
        }
        else
        {
            return 99999;
        }
    }
}


public class Player
{
    public int index;
    public GameObject obj;
    public Rigidbody body;
    public PlayerInputManager playerInputManager;
    public float maxSpeed = 4.0f;
    public float maxAcceleration = 10.0f;
    public float hitProtectionDuration = 3.0f;
    public float autoRestoreHPRate = 3.0f;
    public Int32 initialMaxHP = 300;
    public Color initialBaseColor;
    public Material material;

    public Vector3 velocity;
    public Weapon weapon;
    public int exp = 0;
    public int level = 0;
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
        UpdateLevel();
        Shoot();
        UpdateMaterial();
    }

    public void FixedUpdate()
    {
        UpdateVelocity();
    }

    public void UpdateLevel()
    {
        int currentLevel = GameManager.allLevelPlayerData.GetCurrentLevel(exp);
        if (currentLevel > level)
        {
            level = currentLevel;
            weapon = GameManager.allLevelPlayerData.GetWeapon(index, level);
        }

        GameManager.uiManager.UpdatePlayerLevel(index, exp);
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
