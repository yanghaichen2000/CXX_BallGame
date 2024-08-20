using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Unity.Mathematics;

public class Boss
{
    public const float state1FixedShootStrategyDuration = 5.0f;
    public const float state1FixedShootStrategyRandomExtraDuration = 2.0f;
    public const float state1ShootRotationSpeed = 720.0f;
    public const float state1TransitionToState6Threshold = 5.0f;
    public const float state3Cd = 5.0f;
    public const float state3Duration = 3.0f;
    public const float state3LoadingTime = 1.0f;
    public const float state4InitialStateTransitionPossibility = 0.33f;
    public const float state4Duration = 15.0f;
    public const float state4Cd = 60.0f;
    public const float state5VerticalSpeed = 50.0f;
    public const float state5Duration = 1.0f;
    public const float state6InitialStopDuration = 0.7f;
    public const float state6JumpDuration = 1.0f;
    public const float state6Gravity = 60.0f;
    public const float state7Duration = 2.4f;
    public const float state7WeakDuration = 0.7f;
    public const float state7ShootRotationSpeedWeak = 5.0f;
    public const float state7ShootRotationSpeedNormal = 27.0f;

    public GameObject obj;
    public Rigidbody body;
    public float maxSpeed;
    public float maxAcceleration;
    
    public int maxHP;
    public Player player1;
    public Player player2;

    public bool enabled;
    public BossWeapon weapon;
    public Vector3 velocity;
    public int state; // 0£ºÍ£Ö¹£¬1£ºÅö×²£¬2£º»ØÖÐÐÄ£¬3£ºÐîÁ¦È«ÆÁµ¯Ä»£¬4£ºÕÙ»½Ð¡¹Ö£¬5£ºÂäµØ£¬6£ºÔ¶³ÌÍ»Ï®ÌøÔ¾
    public Vector3 state2Destination;
    public Vector3 lastShootDir;
    public int hp;
    public float mass;
    public float stateStartTime;
    public bool hitPlayer1;
    public float lastState4StartTime;
    public float lastState3StartTime;
    public int state4enemyWaveNum;
    public int state4enemyNum;
    public float state4StateTransitionPossibility;
    public float state1LastChangeShootStrategyTime;
    public int state1ShootStrategy; // 0£ºÃé×¼Ô¶´¦Íæ¼Ò£¬1£º×ªÈ¦
    public float state1LastHitPlayerTime;
    public bool state6TargetPositionComfirmed;
    public Vector3 state6InitialVelocityXZ;
    public float state6InitialVelocityY;
    public Vector3 state6InitialPosition;
    public bool state7Ready;
    public bool state7CanRepeatState6;
    public float state6StopDuration;

    public Boss()
    {
        enabled = true;
        obj = GameObject.Find("Boss");
        body = obj.GetComponent<Rigidbody>();
        maxSpeed = 8.0f;
        maxAcceleration = 25.0f;
        player1 = GameManager.player1;
        player2 = GameManager.player2;
        weapon = new BossWeapon(WeaponDatumSample.bossState1);
        state = 1;
        lastShootDir = new Vector3(1.0f, 0.0f, 0.0f);
        maxHP = 777777;
        hp = maxHP;
        mass = GetMassFromHP(hp);
        stateStartTime = GameManager.gameTime;
        hitPlayer1 = false;
        lastState4StartTime = -99999.0f;
        lastState3StartTime = -99999.0f;
        state4enemyNum = 0;
        state4StateTransitionPossibility = state4InitialStateTransitionPossibility;
        state1LastChangeShootStrategyTime = -99999.0f;
        state1LastHitPlayerTime = -99999.0f;
        state1ShootStrategy = 0;
        state6TargetPositionComfirmed = false;
        state7Ready = false;
        state7CanRepeatState6 = true;
        state6StopDuration = state6InitialStopDuration;
    }

    public void FixedUpdate()
    {
        if (!enabled) return;

        Vector3 pos = body.position;
        if (pos.x > -20.0f && pos.x < 20.0f && pos.z > -15.0f && pos.z < 15.0f)
        {
            pos.y = Mathf.Max(0.0f, pos.y);
            body.position = pos;
        }

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
        else if (state == 5)
        {
            Vector3 desiredVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            body.velocity = desiredVelocity;
        }
        else if (state == 6)
        {
            if (GameManager.gameTime - stateStartTime < state6StopDuration)
            {
                Vector3 desiredVelocity = GUtils.Lerp(body.velocity, Vector3.zero, 8.0f * GameManager.deltaTime);
                UpdateBodyVelocity(desiredVelocity);
            }
            else
            {
                Vector3 desiredVelocity = new Vector3(0.0f, 0.0f, 0.0f);
                body.velocity = desiredVelocity;
            }
        }
        else if (state == 7)
        {
            Vector3 desiredVelocity = new Vector3(0.0f, 0.0f, 0.0f);
            UpdateBodyVelocity(desiredVelocity);
        }
    }

    public void Update()
    {
        if (!enabled) return;

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

                state1LastHitPlayerTime = GameManager.gameTime;
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
                GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, 12.0f, 8, 8, 9, 1.2f, -1.2f, 0.0f);
                GameManager.enemyLegion.SpawnSphereEnemy(16.0f, 12.0f, 8, 8, 8, -1.2f, -1.2f, 0.0f);
                GameManager.enemyLegion.SpawnSphereEnemy(16.0f, -12.0f, 8, 8, 9, -1.2f, 1.2f, 0.0f);

                GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, 13.0f, 21, 4, 8, 1.6f, -1.1f, 12.0f);
                GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, -13.0f, 21, 4, 9, 1.6f, 1.1f, 12.0f);

                state4enemyWaveNum++;
            }
        }
        else if (state == 5)
        {
            float state5Time = GameManager.gameTime - stateStartTime;
            obj.transform.localPosition = new Vector3(0.0f, 0.51f + state5VerticalSpeed * (state5Duration - state5Time), 0.0f);
            body.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        }
        else if (state == 6)
        {
            float state6Time = GameManager.gameTime - stateStartTime;
            if (state6Time < state6StopDuration)
            {
                weapon.Shoot(obj.transform.localPosition, lastShootDir);
            }
            else
            {
                if (!state6TargetPositionComfirmed)
                {
                    GetState6InitialVelocityAndPosition();
                    state6TargetPositionComfirmed = true;

                    GameManager.cameraMotionManager.ShakeByRotation(0.5f);
                    GameManager.cameraMotionManager.ShakeByZDisplacement(-4.0f);
                    body.isKinematic = true;
                }

                float t = state6Time - state6StopDuration;
                Vector3 currentPos = state6InitialPosition + t * state6InitialVelocityXZ;
                currentPos.y = 0.5f + state6InitialVelocityY * t - 0.5f * state6Gravity * t * t;
                obj.transform.localPosition = currentPos;
            }
        }
        else if (state == 7)
        {
            float state7Time = GameManager.gameTime - stateStartTime;
            if (state7Time > state7WeakDuration && !state7Ready)
            {
                weapon = new BossWeapon(WeaponDatumSample.bossState7Normal);
                state7Ready = true;
            }
            lastShootDir = GetState7ShootDir();
            weapon.Shoot(obj.transform.localPosition, lastShootDir);
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
                if (hitPlayer1 && GUtils.RandomBool(0.33f) && GameManager.gameTime - lastState3StartTime > state3Cd)
                {
                    state = 3;
                    lastState3StartTime = GameManager.gameTime;
                    UpdateStateStartTime();
                    weapon = new BossWeapon(WeaponDatumSample.bossState3);
                    weapon.lastShootTime = GameManager.gameTime + state3LoadingTime;
                }
                else if (GameManager.gameTime - state1LastHitPlayerTime > state1TransitionToState6Threshold
                    && GameManager.gameTime - stateStartTime > state1TransitionToState6Threshold)
                {
                    if (GUtils.RandomBool(0.5f))
                    {
                        state1LastHitPlayerTime = GameManager.gameTime - state1TransitionToState6Threshold + 0.5f;
                    }
                    else
                    {
                        state = 6;
                        UpdateStateStartTime();
                        weapon = new BossWeapon(WeaponDatumSample.bossState6);
                        state6StopDuration = state6InitialStopDuration;
                        state7CanRepeatState6 = true;
                        state6TargetPositionComfirmed = false;
                    }
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
                //if (GUtils.RandomBool(1.0f))
                if (GUtils.RandomBool(0.5f) && GameManager.gameTime - lastState3StartTime > state3Cd)
                {
                    state = 3;
                    lastState3StartTime = GameManager.gameTime;
                    UpdateStateStartTime();
                    weapon = new BossWeapon(WeaponDatumSample.bossState3);
                    weapon.lastShootTime = GameManager.gameTime + state3LoadingTime;
                }
                //else if (GUtils.RandomBool(1.0f))
                else if (hp < maxHP * 0.625f && GUtils.RandomBool(0.3f))
                {
                    state = 6;
                    UpdateStateStartTime();
                    weapon = new BossWeapon(WeaponDatumSample.bossState6);
                    state6StopDuration = state6InitialStopDuration;
                    state7CanRepeatState6 = true;
                    state6TargetPositionComfirmed = false;
                }
                else
                {
                    state = 1;
                    UpdateStateStartTime();
                    weapon = new BossWeapon(WeaponDatumSample.bossState1);
                }               
            }
        }
        else if (state == 3)
        {
            if (GameManager.gameTime - stateStartTime > state3Duration)
            {
                //if (GUtils.RandomBool(1.0f))
                if (hp < maxHP * 0.8f && GUtils.RandomBool(state4StateTransitionPossibility) && GameManager.gameTime - lastState4StartTime > state4Cd)
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
            if (GameManager.gameTime - stateStartTime > 22.0f ||
                (GameManager.gameTime - stateStartTime > 17.0f && state4enemyNum == 0))
            {
                state = 5;
                body.isKinematic = true;
                UpdateStateStartTime();
            }
        }
        else if (state == 5)
        {
            if (GameManager.gameTime - stateStartTime >= state5Duration)
            {
                state = 1;
                body.isKinematic = false;
                UpdateStateStartTime();
                lastState3StartTime = GameManager.gameTime;
                GameManager.computeManager.knockOutAllEnemyRequest = true;
                weapon = new BossWeapon(WeaponDatumSample.bossState1);
                GameManager.cameraMotionManager.ShakeByRotation(3.5f);
                GameManager.cameraMotionManager.ShakeByZDisplacement(-8.0f);
            }
        }
        else if (state == 6)
        {
            if (GameManager.gameTime - stateStartTime >= state6StopDuration + state6JumpDuration)
            {
                //if (GUtils.RandomBool(1.0f))
                if (hp < maxHP * 0.7f && GUtils.RandomBool(0.8f))
                {
                    state = 7;
                    body.isKinematic = false;
                    body.velocity = Vector3.zero;
                    UpdateStateStartTime();
                    state7Ready = false;
                    weapon = new BossWeapon(WeaponDatumSample.bossState7Weak);
                    GameManager.cameraMotionManager.ShakeByRotation(2.0f);
                    GameManager.cameraMotionManager.ShakeByZDisplacement(-4.0f);
                }
                else
                {
                    state = 1;
                    body.isKinematic = false;
                    body.velocity = Vector3.zero;
                    UpdateStateStartTime();
                    lastState3StartTime = GameManager.gameTime;
                    weapon = new BossWeapon(WeaponDatumSample.bossState1);
                    GameManager.cameraMotionManager.ShakeByRotation(2.0f);
                    GameManager.cameraMotionManager.ShakeByZDisplacement(-4.0f);
                }
            }
        }
        else if (state == 7)
        {
            if (GameManager.gameTime - stateStartTime >= state7Duration)
            {
                //if (state7CanRepeatState6)
                if (state7CanRepeatState6 && hp < maxHP * 0.5f && GUtils.RandomBool(0.7f))
                {
                    state7CanRepeatState6 = false;
                    state = 6;
                    state6StopDuration = 0.0f;
                    UpdateStateStartTime();
                    state6TargetPositionComfirmed = false;
                }
                //else if (GUtils.RandomBool(1.0f))
                else if (hp < maxHP * 0.7f && GUtils.RandomBool(state4StateTransitionPossibility) && GameManager.gameTime - lastState4StartTime > state4Cd)
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
    }

    public void OnProcessBossReadbackData(ComputeManager.BossDatum datum)
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

        if (GameManager.gameTime - state1LastChangeShootStrategyTime > 
            state1FixedShootStrategyDuration + UnityEngine.Random.Range(0.0f, 1.0f) * state1FixedShootStrategyRandomExtraDuration)
        {
            state1ShootStrategy = 1 - state1ShootStrategy;
            state1LastChangeShootStrategyTime = GameManager.gameTime;
        }

        if (state1ShootStrategy == 0)
        {
            if (player1Pos.y >= 0.1 && player1Pos.y <= 0.6)
            {
                if (GameManager.playerSkillManager.skills["SharedSkill1"].GetState() == 1)
                {
                    Vector3 desiredShootDir = -player1Dir;
                    desiredShootDir.y = 0;
                    desiredShootDir = desiredShootDir.normalized;
                    return GUtils.Lerp(lastShootDir, desiredShootDir, 0.9f);
                }
                else if (player2Pos.y >= 0.1 && player2Pos.y <= 0.6)
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
        else
        {
            Quaternion rotation = Quaternion.Euler(0, state1ShootRotationSpeed * GameManager.deltaTime, 0);
            Vector3 desiredShootDir = rotation * lastShootDir;
            desiredShootDir.y = 0;
            desiredShootDir = desiredShootDir.normalized;
            return desiredShootDir;
        }
    }

    public Vector3 GetState2ShootDir()
    {
        Vector3 desiredShootDir = -GetState2DesiredVelocity().normalized;
        desiredShootDir.y = 0;
        desiredShootDir = desiredShootDir.normalized;
        return GUtils.Lerp(lastShootDir, desiredShootDir, 0.8f).normalized;
    }

    public Vector3 GetState7ShootDir()
    {
        float state7Time = GameManager.gameTime - stateStartTime;
        float rotationSpeed = state7Time < state7WeakDuration ?
            state7ShootRotationSpeedWeak : state7ShootRotationSpeedNormal;
        Quaternion rotation = Quaternion.Euler(0, rotationSpeed * GameManager.deltaTime, 0);
        Vector3 desiredShootDir = rotation * lastShootDir;
        desiredShootDir.y = 0;
        desiredShootDir = desiredShootDir.normalized;
        return desiredShootDir;
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

        if (GameManager.playerSkillManager.skills["SharedSkill1"].GetState() == 1)
        {
            interestOnPlayer1 -= 1000000.0f;
        }
            
        Vector3 desiredVelocity = interestOnPlayer1 > interestOnPlayer2 ?
            player1Dir * maxSpeed : player2Dir * maxSpeed;

        if ((player1Pos.y <= 0.4 || player1Pos.y >= 0.8) && (player2Pos.y <= 0.4 || player2Pos.y >= 0.8))
        {
            desiredVelocity = Vector3.zero;
        }

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

    public void GetState6InitialVelocityAndPosition()
    {
        Vector3 player1Pos = player1.GetPos();
        Vector3 player2Pos = player2.GetPos();
        float player1Distance = (player1Pos - obj.transform.localPosition).magnitude;
        float player2Distance = (player2Pos - obj.transform.localPosition).magnitude;

        Vector3 targetPos;
        if (player1Pos.x > -20.5f && player1Pos.x < 20.5f && player1Pos.z > -15.5f && player1Pos.z < 15.5f && player1Pos.y > 0.4f)
        {
            if (GameManager.playerSkillManager.skills["SharedSkill1"].GetState() == 1)
            {
                targetPos = player2Pos;
            }
            else if (player2Pos.x > -20.5f && player2Pos.x < 20.5f && player2Pos.z > -15.5f && player2Pos.z < 15.5f && player2Pos.y > 0.4f)
            {
                targetPos = player1Distance < player2Distance ? player2Pos : player1Pos;
            }
            else
            {
                targetPos = player1Pos;
            }
        }
        else
        {
            if (player2Pos.x > -20.5f && player2Pos.x < 20.5f && player2Pos.z > -15.5f && player2Pos.z < 15.5f && player2Pos.y > 0.4f)
            {
                targetPos = player2Pos;
            }
            else
            {
                targetPos = new Vector3(0.0f, 0.5f, 0.0f);
            }
        }

        float theta = UnityEngine.Random.Range(0.0f, 2 * Mathf.PI);
        float r = UnityEngine.Random.Range(0.2f, 0.3f);
        Vector3 targetPosBias = new Vector3(r * Mathf.Cos(theta), 0.0f, r * Mathf.Sin(theta));

        targetPos = GUtils.Clamp(targetPos, new Vector3(-16.5f, 0.5f, -11.5f), new Vector3(16.5f, 0.5f, 11.5f));
        targetPos += targetPosBias;
        Vector3 RelativePosXZ = targetPos - obj.transform.localPosition;
        RelativePosXZ.y = 0.0f;
        state6InitialVelocityXZ = RelativePosXZ / state6JumpDuration;
        state6InitialVelocityY = state6Gravity * state6JumpDuration * 0.5f;
        state6InitialPosition = obj.transform.localPosition;
        state6InitialPosition.y = 0.50001f;
    }

    public float GetMassFromHP(int hp)
    {
        return Mathf.Pow(hp / 100000.0f, 5.0f) + 1;
    }

    public void UpdateStateStartTime()
    {
        stateStartTime = GameManager.gameTime;
    }

    public void Remove()
    {
        enabled = false;
        obj.transform.localPosition = new Vector3(-100.0f, 0.5f, -100.0f);
        body.isKinematic = true;
        GameManager.uiManager.RemoveBossUI();
    }

    public void Load()
    {
        enabled = true;
        state = 5;
        body.isKinematic = true;
        UpdateStateStartTime();
        GameManager.uiManager.ShowBossUI();
    }
}

