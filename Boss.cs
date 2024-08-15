using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEditor.PlayerSettings;
using Unity.VisualScripting;
using static UnityEditor.Experimental.GraphView.GraphView;
using Unity.Mathematics;

public class Boss
{
    public const float state3Duration = 3.5f;
    public const float state3LoadingTime = 1.7f;
    public const float state4InitialStateTransitionPossibility = 0.33f;
    public const float state4Duration = 15.0f;
    public const float state4Cd = 60.0f;
    public const float state5VerticalSpeed = 50.0f;
    public const float state5Duration = 1.0f;


    public GameObject obj;
    public Rigidbody body;
    public float maxSpeed;
    public float maxAcceleration;
    public float state1ShootRotationSpeed;
    public int maxHP;

    public Player player1;
    public Player player2;

    public BossWeapon weapon;
    public Vector3 velocity;
    public int state; // 0£ºÍ£Ö¹£¬1£ºÅö×²£¬2£º»ØÖÐÐÄ£¬3£ºÐîÁ¦È«ÆÁµ¯Ä»£¬4£ºÕÙ»½Ð¡¹Ö£¬5£ºÂäµØ
    public Vector3 state2Destination;
    public Vector3 lastShootDir;
    public int hp;
    public float mass;
    public float stateStartTime;
    public bool hitPlayer1;
    public float lastState4StartTime;
    public int state4enemyWaveNum;
    public int state4enemyNum;
    public float state4StateTransitionPossibility;

    public Boss()
    {
        obj = GameObject.Find("Boss");
        body = obj.GetComponent<Rigidbody>();
        maxSpeed = 10.0f;
        maxAcceleration = 20.0f;
        state1ShootRotationSpeed = 720.0f;
        player1 = GameManager.player1;
        player2 = GameManager.player2;
        weapon = new BossWeapon(WeaponDatumSample.bossState1);
        state = 1;
        lastShootDir = new Vector3(1.0f, 0.0f, 0.0f);
        maxHP = 1000000;
        hp = maxHP;
        mass = GetMassFromHP(hp);
        stateStartTime = GameManager.gameTime;
        hitPlayer1 = false;
        lastState4StartTime = -99999.0f;
        state4enemyNum = 0;
        state4StateTransitionPossibility = state4InitialStateTransitionPossibility;
    }

    public void FixedUpdate()
    {
        // update velocity
        if (state == 1)
        {
            Vector3 desiredVelocity = GetState1DesiredVelocity();
            UpdateBodyVelocity(desiredVelocity);
        }
        else if (state == 2)
        {
            Vector3 desiredVelocity = GetState2DesiredVelocity();
            UpdateBodyVelocity(desiredVelocity);
        }
        else if (state == 3)
        {
            Vector3 desiredVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            UpdateBodyVelocity(desiredVelocity);
        }
        else if (state == 4)
        {
            Vector3 desiredVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            UpdateBodyVelocity(desiredVelocity);
        }
    }

    public void Update()
    {
        // check collision
        hitPlayer1 = false;
        foreach (Player player in new Player[]{ player1, player2 })
        {
            Vector3 playerPos = player.GetPos();
            float playerDistance = (playerPos - obj.transform.localPosition).magnitude;
            if (playerDistance <= 1.05f && GameManager.gameTime - player.lastHitByEnemyTime > 0.1f)
            {
                Vector3 playerDir = (playerPos - obj.transform.localPosition).normalized;
                Vector3 dV = playerDir;
                dV.y = 0.0f;
                dV.x += UnityEngine.Random.Range(-0.1f, 0.1f);
                dV.z += UnityEngine.Random.Range(-0.1f, 0.1f);
                dV = dV.normalized * 12.0f;
                dV.y = 8.0f;
                player.body.velocity = dV / player.m / 2.0f + dV / 2.0f / 2.0f;
                player.obj.transform.localPosition = playerPos + playerDir * Mathf.Max(1.0001f - playerDistance, 0.0f);
                player.lastHitByEnemyTime = GameManager.gameTime;
                player.hp -= 30;
                player.disarmed = true;
                player.lastHitByBossTime = GameManager.gameTime;
                float shakeForce = Math.Clamp(1.0f / player.m, 0.5f, 5.0f);
                GameManager.cameraMotionManager.ShakeByRotation(shakeForce);

                if (player.index == 0)
                {
                    hitPlayer1 = true;
                }
            }
        }
        
        // common
        if (state == 1)
        {
            lastShootDir = GetState1ShootDir();
            weapon.Shoot(obj.transform.localPosition, lastShootDir);
        }
        else if (state == 2)
        {
            lastShootDir = GetState2ShootDir();
            weapon.Shoot(obj.transform.localPosition, lastShootDir);
        }
        else if (state == 3)
        {
            weapon.Shoot(obj.transform.localPosition, lastShootDir);
        }
        else if (state == 4)
        {
            float state4Time = GameManager.gameTime - stateStartTime;
            if (state4Time > 0.1f && state4Time < 1.0f)
            {
                obj.transform.localPosition += new Vector3(0.0f, state5VerticalSpeed * GameManager.deltaTime, 0.0f);
            }
            else if (state4Time > 1.0f && state4enemyWaveNum == 0)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, -12.0f, 8, 8, 8, 1.2f, 1.2f, 0.0f);
                GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, 12.0f, 8, 8, 8, 1.2f, -1.2f, 0.0f);
                GameManager.enemyLegion.SpawnSphereEnemy(16.0f, 12.0f, 8, 8, 8, -1.2f, -1.2f, 0.0f);
                GameManager.enemyLegion.SpawnSphereEnemy(16.0f, -12.0f, 8, 8, 8, -1.2f, 1.2f, 0.0f);

                GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, 13.0f, 21, 4, 9, 1.6f, -1.1f, 12.0f);
                GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, -13.0f, 21, 4, 9, 1.6f, 1.1f, 12.0f);

                state4enemyWaveNum++;
            }
        }
        else if (state == 5)
        {
            float state5Time = GameManager.gameTime - stateStartTime;
            obj.transform.localPosition = new Vector3(0.0f, 0.5f + state5VerticalSpeed * (state5Duration - state5Time), 0.0f);
        }


        // update state
        if (state == 1)
        {
            Vector3 pos = obj.transform.localPosition;
            if (pos.x < -17.0f || pos.x > 17.0f || pos.z < -12.0f || pos.z > 12.0f)
            {
                state = 2;
                UpdateStateStartTime();
                weapon = new BossWeapon(WeaponDatumSample.bossState2);
                SetState2Destination();
            }
            else
            {
                if (hitPlayer1 && GUtils.RandomBool(0.33f))
                {
                    state = 3;
                    UpdateStateStartTime();
                    weapon = new BossWeapon(WeaponDatumSample.bossState3);
                    weapon.lastShootTime = GameManager.gameTime + state3LoadingTime;
                }
            }
        }
        else if (state == 2)
        {
            Vector3 pos = obj.transform.localPosition;
            Vector3 destRelativePos = pos - state2Destination;
            if (destRelativePos.magnitude < 1.0f ||
                (pos.x > -14.0f && pos.x < 14.0f && pos.z > -10.0f && pos.z < 10.0f))
            {
                if (GUtils.RandomBool(0.33f))
                {
                    state = 1;
                    UpdateStateStartTime();
                    weapon = new BossWeapon(WeaponDatumSample.bossState1);
                }
                else
                {
                    state = 3;
                    UpdateStateStartTime();
                    weapon = new BossWeapon(WeaponDatumSample.bossState3);
                    weapon.lastShootTime = GameManager.gameTime + state3LoadingTime;
                }
            }
        }
        else if (state == 3)
        {
            if (GameManager.gameTime - stateStartTime > state3Duration)
            {
                if (hp < maxHP * 0.7 && GUtils.RandomBool(state4StateTransitionPossibility) && GameManager.gameTime - lastState4StartTime > state4Cd)
                //if (GUtils.RandomBool(1.0f))
                {
                    state = 4;
                    UpdateStateStartTime();
                    lastState4StartTime = GameManager.gameTime;
                    state4enemyWaveNum = 0;
                    state4StateTransitionPossibility = state4InitialStateTransitionPossibility;
                    GameManager.cameraMotionManager.ShakeByZDisplacement(-5.0f);
                    GameManager.cameraMotionManager.ShakeByRotation(0.5f);
                }
                else
                {
                    state4StateTransitionPossibility = Mathf.Lerp(state4StateTransitionPossibility, 1.0f, 0.5f);

                    Vector3 pos = obj.transform.localPosition;
                    if (pos.x < -17.0f || pos.x > 17.0f || pos.z < -12.0f || pos.z > 12.0f)
                    {
                        state = 2;
                        UpdateStateStartTime();
                        weapon = new BossWeapon(WeaponDatumSample.bossState2);
                        SetState2Destination();
                    }
                    else
                    {
                        state = 1;
                        UpdateStateStartTime();
                        weapon = new BossWeapon(WeaponDatumSample.bossState1);
                    }
                }
            }
        }
        else if (state == 4)
        {
            if (GameManager.gameTime - stateStartTime > 17.0f && state4enemyNum == 0)
            {
                state = 5;
                UpdateStateStartTime();
            }
        }
        else if (state == 5)
        {
            if (GameManager.gameTime - stateStartTime >= state5Duration)
            {
                state = 1;
                UpdateStateStartTime();
                weapon = new BossWeapon(WeaponDatumSample.bossState1);
                GameManager.cameraMotionManager.ShakeByRotation(2.0f);
            }
        }
    }

    public void OnProcessBossReadbackData(ComputeCenter.BossDatum datum)
    {
        UpdateHPAndMass(datum.hpChange);
        Vector3 dV = new Vector3(datum.hitImpulse.x / 10000.0f, datum.hitImpulse.y / 10000.0f, datum.hitImpulse.z / 10000.0f);
        body.velocity = body.velocity + dV / mass;
    }

    public void UpdateHPAndMass(int hpChange)
    {
        hp = Math.Clamp(hp + hpChange, 0, maxHP);
        mass = GetMassFromHP(hp);
        GameManager.uiManager.UpdateBossHPAndMass();
    }

    public void UpdateBodyVelocity(Vector3 desiredVelocity)
    {
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        if (obj.transform.localPosition.y > 0.501f) maxSpeedChange *= 0.3f;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        body.velocity = velocity;
    }

    public void SetState2Destination()
    {
        Vector3 player1Pos = player1.GetPos();
        Vector3 player2Pos = player2.GetPos();

        if (player1Pos.x > -14.0f && player1Pos.x < 14.0f && player1Pos.z > -10.0f && player1Pos.z < 10.0f)
        {
            if (player2Pos.x > -14.0f && player2Pos.x < 14.0f && player2Pos.z > -10.0f && player2Pos.z < 10.0f)
            {
                state2Destination = UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? player1Pos : player2Pos;
            }
            else
            {
                state2Destination = player1Pos;
            }
        }
        else if (player2Pos.x > -14.0f && player2Pos.x < 14.0f && player2Pos.z > -10.0f && player2Pos.z < 10.0f)
        {
            state2Destination = player2Pos;
        }
        else
        {
            state2Destination = new Vector3(UnityEngine.Random.Range(-10.0f, 10.0f), 0.5f, UnityEngine.Random.Range(-6.0f, 6.0f));
        }
    }

    public Vector3 GetState1ShootDir()
    {
        Vector3 player1Pos = player1.GetPos();
        Vector3 player2Pos = player2.GetPos();
        float player1Distance = (player1Pos - obj.transform.localPosition).magnitude;
        float player2Distance = (player2Pos - obj.transform.localPosition).magnitude;
        Vector3 player1Dir = (player1Pos - obj.transform.localPosition).normalized;
        Vector3 player2Dir = (player2Pos - obj.transform.localPosition).normalized;

        if (player1Pos.y >= 0.1 && player1Pos.y <= 0.6)
        {
            if (player2Pos.y >= 0.1 && player2Pos.y <= 0.6)
            {
                Vector3 desiredShootDir = player1Distance < player2Distance ? player2Dir : player1Dir;
                desiredShootDir.y = 0;
                desiredShootDir = desiredShootDir.normalized;
                return GUtils.Lerp(lastShootDir, desiredShootDir, 0.9f);
            }
            else
            {
                Vector3 desiredShootDir = player1Dir;
                desiredShootDir.y = 0;
                desiredShootDir = desiredShootDir.normalized;
                return GUtils.Lerp(lastShootDir, desiredShootDir, 0.9f);
            }
        }
        else
        {
            if (player2Pos.y >= 0.1 && player2Pos.y <= 0.6)
            {
                Vector3 desiredShootDir = player2Dir;
                desiredShootDir.y = 0;
                desiredShootDir = desiredShootDir.normalized;
                return GUtils.Lerp(lastShootDir, desiredShootDir, 0.9f);
            }
            else
            {
                Quaternion rotation = Quaternion.Euler(0, state1ShootRotationSpeed * GameManager.deltaTime, 0);
                Vector3 desiredShootDir = rotation * lastShootDir;
                desiredShootDir.y = 0;
                desiredShootDir = desiredShootDir.normalized;
                return desiredShootDir;
            }
        }
    }

    public Vector3 GetState2ShootDir()
    {
        Vector3 desiredShootDir = -GetState2DesiredVelocity().normalized;
        desiredShootDir.y = 0;
        desiredShootDir = desiredShootDir.normalized;
        return GUtils.Lerp(lastShootDir, desiredShootDir, 0.8f).normalized;
    }

    public Vector3 GetState1DesiredVelocity()
    {
        velocity = body.velocity;

        Vector3 player1Pos = player1.GetPos();
        Vector3 player2Pos = player2.GetPos();
        float player1Distance = (player1Pos - obj.transform.localPosition).magnitude;
        float player2Distance = (player2Pos - obj.transform.localPosition).magnitude;
        Vector3 player1Dir = (player1Pos - obj.transform.localPosition).normalized;
        Vector3 player2Dir = (player2Pos - obj.transform.localPosition).normalized;

        float interestOnPlayer1 = -(player1Distance + Mathf.Abs(player1Pos.y - 0.5f) * 2.0f);
        float interestOnPlayer2 = -(player2Distance + Mathf.Abs(player2Pos.y - 0.5f) * 2.0f);

        Vector3 desiredVelocity = interestOnPlayer1 > interestOnPlayer2 ?
            player1Dir * maxSpeed : player2Dir * maxSpeed;

        return desiredVelocity;
    }

    public Vector3 GetState2DesiredVelocity()
    {
        velocity = body.velocity;
        Vector3 pos = obj.transform.localPosition;

        Vector3 currentDir = body.velocity.normalized;
        float currentDirWeightX = Math.Clamp(Mathf.Min(pos.x + 20.0f, 20.0f - pos.x), 0.1f, 1.0f);
        float currentDirWeightZ = Math.Clamp(Mathf.Min(pos.z + 20.0f, 20.0f - pos.z), 0.1f, 1.0f);
        Vector3 currentDirWeight = new Vector3(currentDirWeightX, 0.0f, currentDirWeightZ);

        Vector3 desiredDir = (GUtils.Mul(currentDir, currentDirWeight) * 0.5f + (state2Destination - pos).normalized).normalized;
        Vector3 desiredVelocity = desiredDir * maxSpeed;

        return desiredVelocity;
    }

    public float GetMassFromHP(int hp)
    {
        return Mathf.Pow(hp / 100000.0f, 5.0f) + 1;
    }

    public void UpdateStateStartTime()
    {
        stateStartTime = GameManager.gameTime;
    }
}


