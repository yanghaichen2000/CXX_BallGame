using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class AllLevelPlayerData
{
    public WeaponDatum[] player1WeaponData;
    public WeaponDatum[] player2WeaponData;
    public int[] levelExpList;

    public AllLevelPlayerData()
    {
        levelExpList = new int[20] { 2, 5, 9, 14, 25, 50, 85, 140, 200, 280, 400, 540, 680, 890, 1150, 1450, 1800, 2200, 2700, 3300};
        InitializePlayer1WeaponData();
        InitializePlayer2WeaponData();
    }

    public void InitializePlayer1WeaponData()
    {
        player1WeaponData = new WeaponDatum[21];

        player1WeaponData[0] = new WeaponDatum()
        {
            shootInterval = 0.6f,
            virtualYRange = 0.2f,
            virtualYBase = 0.5f,
            angleBiasRange = 1.0f,
            extraBulletsPerSide = 1,
            angle = 20.0f,
            speed = 8.0f,
            radius = 0.07f,
            damage = 3,
            bounces = 5,
            lifeSpan = 12.0f,
            impulse = 0.5f,
            renderingBiasY = 0.0f,
        };

        player1WeaponData[1] = player1WeaponData[0];
        player1WeaponData[1].shootInterval = 0.5f;

        player1WeaponData[2] = player1WeaponData[1];
        player1WeaponData[2].extraBulletsPerSide = 2;
        player1WeaponData[2].angle = 10.0f;

        player1WeaponData[3] = player1WeaponData[2];
        player1WeaponData[3].shootInterval = 0.35f;
        player1WeaponData[3].speed = 8.5f;

        player1WeaponData[4] = player1WeaponData[3];
        player1WeaponData[4].extraBulletsPerSide = 4;
        player1WeaponData[4].angle = 6.0f;

        player1WeaponData[5] = player1WeaponData[4];
        player1WeaponData[5].shootInterval = 0.25f;
        player1WeaponData[5].speed = 9.0f;

        player1WeaponData[6] = player1WeaponData[5];
        player1WeaponData[6].extraBulletsPerSide = 8;
        player1WeaponData[6].angle = 4.0f;

        player1WeaponData[7] = player1WeaponData[6];
        player1WeaponData[7].shootInterval = 0.18f;
        player1WeaponData[7].speed = 9.5f;

        player1WeaponData[8] = player1WeaponData[7];
        player1WeaponData[8].extraBulletsPerSide = 16;
        player1WeaponData[8].angle = 3.0f;

        player1WeaponData[9] = player1WeaponData[8];
        player1WeaponData[9].shootInterval = 0.10f;
        player1WeaponData[9].speed = 10.0f;

        player1WeaponData[10] = player1WeaponData[9];
        player1WeaponData[10].extraBulletsPerSide = 28;
        player1WeaponData[10].angle = 1.8f;
        player1WeaponData[10].impulse = 0.3f;

        player1WeaponData[11] = player1WeaponData[10];
        player1WeaponData[11].shootInterval = 0.08f;

        player1WeaponData[12] = player1WeaponData[11];
        player1WeaponData[12].shootInterval = 0.075f;
        player1WeaponData[12].extraBulletsPerSide = 40;
        player1WeaponData[12].angle = 1.15f;

        player1WeaponData[13] = player1WeaponData[12];
        player1WeaponData[13].shootInterval = 0.07f;
        player1WeaponData[13].extraBulletsPerSide = 60;
        player1WeaponData[13].angle = 0.9f;

        player1WeaponData[14] = player1WeaponData[13];
        player1WeaponData[14].shootInterval = 0.065f;
        player1WeaponData[14].extraBulletsPerSide = 80;
        player1WeaponData[14].angle = 0.7f;
        player1WeaponData[14].impulse = 0.2f;

        player1WeaponData[15] = player1WeaponData[14];
        player1WeaponData[15].extraBulletsPerSide = 100;
        player1WeaponData[15].shootInterval = 0.06f;
        player1WeaponData[15].angle = 0.6f;

        player1WeaponData[16] = player1WeaponData[15];
        player1WeaponData[16].extraBulletsPerSide = 120;
        player1WeaponData[16].shootInterval = 0.054f;
        player1WeaponData[16].angle = 0.5f;

        player1WeaponData[17] = player1WeaponData[16];
        player1WeaponData[17].extraBulletsPerSide = 160;
        player1WeaponData[17].shootInterval = 0.048f;
        player1WeaponData[17].angle = 0.45f;
        player1WeaponData[17].impulse = 0.1f;

        player1WeaponData[18] = player1WeaponData[17];
        player1WeaponData[18].extraBulletsPerSide = 200;
        player1WeaponData[18].shootInterval = 0.042f;
        player1WeaponData[18].angle = 0.4f;
        player1WeaponData[18].impulse = 0.13f;

        player1WeaponData[19] = player1WeaponData[18];
        player1WeaponData[19].extraBulletsPerSide = 240;
        player1WeaponData[19].shootInterval = 0.038f;
        player1WeaponData[19].angle = 0.35f;

        player1WeaponData[20] = player1WeaponData[19];
        player1WeaponData[20].extraBulletsPerSide = 300;
        player1WeaponData[20].shootInterval = 0.034f;
        player1WeaponData[20].angle = 0.34f;
        player1WeaponData[20].impulse = 0.1f;
    }

    public void InitializePlayer2WeaponData()
    {
        player2WeaponData = new WeaponDatum[21];

        player2WeaponData[0] = new WeaponDatum()
        {
            shootInterval = 0.3f,
            virtualYRange = 0.0f,
            virtualYBase = 10.0f,
            angleBiasRange = 1.0f,
            extraBulletsPerSide = 0,
            angle = 0.4f,
            speed = 4.0f,
            radius = 0.07f,
            damage = 1,
            bounces = 2,
            lifeSpan = 12.0f,
            impulse = 3.0f,
            renderingBiasY = 0.1f,
        };

        player2WeaponData[1] = player2WeaponData[0];
        player2WeaponData[1].shootInterval = 0.3f;
        player2WeaponData[1].speed = 5.0f;

        player2WeaponData[2] = player2WeaponData[1];
        player2WeaponData[2].extraBulletsPerSide = 1;
        player2WeaponData[2].shootInterval = 0.27f;
        player2WeaponData[2].angle = 6.0f;

        player2WeaponData[3] = player2WeaponData[2];
        player2WeaponData[3].shootInterval = 0.24f;
        player2WeaponData[3].speed = 7.0f;

        player2WeaponData[4] = player2WeaponData[3];
        player2WeaponData[4].shootInterval = 0.21f;
        player2WeaponData[4].speed = 8.0f;

        player2WeaponData[5] = player2WeaponData[4];
        player2WeaponData[5].shootInterval = 0.18f;
        player2WeaponData[5].extraBulletsPerSide = 2;
        player2WeaponData[5].angle = 2.0f;
        player2WeaponData[5].speed = 9.0f;

        player2WeaponData[6] = player2WeaponData[5];
        player2WeaponData[6].shootInterval = 0.16f;
        player2WeaponData[6].speed = 10.0f;

        player2WeaponData[7] = player2WeaponData[6];
        player2WeaponData[6].shootInterval = 0.14f;
        player2WeaponData[7].speed = 11.0f;

        player2WeaponData[8] = player2WeaponData[7];
        player2WeaponData[8].shootInterval = 0.11f;
        player2WeaponData[8].speed = 12.0f;
        player2WeaponData[8].impulse = 3.5f;

        player2WeaponData[9] = player2WeaponData[8];
        player2WeaponData[9].extraBulletsPerSide = 3;
        player2WeaponData[9].shootInterval = 0.10f;
        player2WeaponData[9].angle = 1.5f;
        player2WeaponData[9].speed = 13.0f;

        player2WeaponData[10] = player2WeaponData[9];
        player2WeaponData[10].shootInterval = 0.07f;
        player2WeaponData[10].speed = 14.0f;

        player2WeaponData[11] = player2WeaponData[10];
        player2WeaponData[11].shootInterval = 0.055f;
        player2WeaponData[11].speed = 15.0f;

        player2WeaponData[12] = player2WeaponData[11];
        player2WeaponData[12].extraBulletsPerSide = 4;
        player2WeaponData[12].angle = 1.2f;
        player2WeaponData[12].speed = 16.0f;

        player2WeaponData[13] = player2WeaponData[12];
        player2WeaponData[13].shootInterval = 0.048f;
        player2WeaponData[13].extraBulletsPerSide = 5;
        player2WeaponData[13].angle = 1.0f;
        player2WeaponData[13].speed = 17.0f;

        player2WeaponData[14] = player2WeaponData[13];
        player2WeaponData[14].shootInterval = 0.043f;
        player2WeaponData[14].extraBulletsPerSide = 6;
        player2WeaponData[14].speed = 18.0f;

        player2WeaponData[15] = player2WeaponData[14];
        player2WeaponData[15].shootInterval = 0.04f;
        player2WeaponData[15].extraBulletsPerSide = 7;
        player2WeaponData[15].speed = 19.0f;

        player2WeaponData[16] = player2WeaponData[15];
        player2WeaponData[16].shootInterval = 0.037f;
        player2WeaponData[16].extraBulletsPerSide = 8;
        player2WeaponData[16].speed = 20.0f;
        player2WeaponData[16].impulse = 4.0f;

        player2WeaponData[17] = player2WeaponData[16];
        player2WeaponData[17].shootInterval = 0.034f;
        player2WeaponData[17].extraBulletsPerSide = 10;
        player2WeaponData[17].speed = 22.0f;

        player2WeaponData[18] = player2WeaponData[17];
        player2WeaponData[18].shootInterval = 0.01f;
        player2WeaponData[18].extraBulletsPerSide = 12;
        player2WeaponData[18].speed = 24.0f;
        player2WeaponData[18].impulse = 4.5f;

        player2WeaponData[19] = player2WeaponData[18];
        player2WeaponData[19].shootInterval = 0.028f;
        player2WeaponData[19].extraBulletsPerSide = 14;

        player2WeaponData[20] = player2WeaponData[19];
        player2WeaponData[20].shootInterval = 0.025f;
        player2WeaponData[20].extraBulletsPerSide = 16;
        player2WeaponData[20].impulse = 5.0f;
    }

    public Weapon GetWeapon(int player, int level)
    {
        if (player == 0)
        {
            level = Math.Min(level, player1WeaponData.Length - 1);
            return new Weapon(player, player1WeaponData[level]);
        }
        else
        {
            level = Math.Min(level, player2WeaponData.Length - 1);
            return new Weapon(player, player2WeaponData[level]);
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
    public float initialMaxSpeed = 4.0f;
    public float initialMaxAcceleration = 10.0f;
    public float hitProtectionDuration = 3.0f;
    public float autoRestoreHPRate = 3.0f;
    public int initialMaxHP = 300;
    public Color initialBaseColor;
    public Material material;

    public Vector3 velocity;
    public Weapon weapon;
    public float maxSpeed;
    public float maxAcceleration;
    public int exp = 0;
    public int level = 0;
    public int hp = 300;
    public int maxHP = 300;
    public float m = 100.0f;
    public bool hittable = false;
    public float lastHitByEnemyTime = -10000.0f;
    public bool disarmed = false;
    public float lastHitByBossTime = -10000.0f;
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

        maxSpeed = initialMaxSpeed;
        maxAcceleration = initialMaxAcceleration;
    }

    public void Update()
    {
        AutoRestoreHP();
        UpdateHittableState();
        UpdateDisarmedState();
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

    public void UpdateDisarmedState()
    {
        if (GameManager.gameTime > lastHitByBossTime + hitProtectionDuration)
        {
            if (disarmed) weapon.SetLastShootTime(GameManager.gameTime);
            disarmed = false;
        }
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
        if (hittable && !disarmed)
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
