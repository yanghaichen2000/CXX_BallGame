using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public struct WeaponDatum
{
    public float shootInterval;
    public float virtualYRange;
    public float virtualYBase;
    public float angleBiasRange;
    public int extraBulletsPerSide;
    public float angle;
    public float speed;
    public float radius;
    public int damage;
    public int bounces;
    public float lifeSpan;
    public float impulse;
    public float renderingBiasY;
}

public class WeaponDatumSample
{
    public static WeaponDatum player1Super = new WeaponDatum()
    {
        shootInterval = 0.05f,
        virtualYRange = 0.2f,
        virtualYBase = 0.5f,
        angleBiasRange = 1.0f,
        extraBulletsPerSide = 200,
        angle = 0.5f,
        speed = 8.0f,
        radius = 0.07f,
        damage = 3,
        bounces = 5,
        lifeSpan = 12.0f,
        impulse = 0.5f,
        renderingBiasY = 0.0f,
    };

    public static WeaponDatum player1Weak = new WeaponDatum()
    {
        shootInterval = 0.3f,
        virtualYRange = 0.2f,
        virtualYBase = 0.5f,
        angleBiasRange = 1.0f,
        extraBulletsPerSide = 1,
        angle = 25.0f,
        speed = 8.0f,
        radius = 0.07f,
        damage = 3,
        bounces = 5,
        lifeSpan = 12.0f,
        impulse = 0.5f,
        renderingBiasY = 0.0f,
    };

    public static WeaponDatum player1Initial = new WeaponDatum()
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

    public static WeaponDatum player2Super = new WeaponDatum()
    {
        shootInterval = 0.05f,
        virtualYRange = 0.0f,
        virtualYBase = 10.0f,
        angleBiasRange = 0.5f,
        extraBulletsPerSide = 60,
        angle = 0.2f,
        speed = 20.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 20.0f,
        renderingBiasY = 0.1f,
    };

    public static WeaponDatum player2Weak = new WeaponDatum()
    {
        shootInterval = 0.15f,
        virtualYRange = 0.0f,
        virtualYBase = 10.0f,
        angleBiasRange = 1.0f,
        extraBulletsPerSide = 0,
        angle = 0.4f,
        speed = 20.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 4.0f,
        renderingBiasY = 0.1f,
    };

    public static WeaponDatum player2Initial = new WeaponDatum()
    {
        shootInterval = 0.3f,
        virtualYRange = 0.0f,
        virtualYBase = 10.0f,
        angleBiasRange = 1.0f,
        extraBulletsPerSide = 0,
        angle = 0.4f,
        speed = 15.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 4.0f,
        renderingBiasY = 0.1f,
    };

    public static WeaponDatum bossState1 = new WeaponDatum()
    {
        shootInterval = 0.04f,
        virtualYRange = 0.2f,
        virtualYBase = 0.5f,
        angleBiasRange = 1.0f,
        extraBulletsPerSide = 5,
        angle = 4.0f,
        speed = 13.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 1.0f,
        renderingBiasY = 0.0f,
    };

    public static WeaponDatum bossState2 = new WeaponDatum()
    {
        shootInterval = 0.01f,
        virtualYRange = 0.2f,
        virtualYBase = 0.5f,
        angleBiasRange = 1.0f,
        extraBulletsPerSide = 40,
        angle = 1.5f,
        speed = 35.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 1.0f,
        renderingBiasY = 0.0f,
    };

    public static WeaponDatum bossState3 = new WeaponDatum()
    {
        shootInterval = 0.03f,
        virtualYRange = 0.2f,
        virtualYBase = 0.5f,
        angleBiasRange = 1.0f,
        extraBulletsPerSide = 180,
        angle = 1.0f,
        speed = 12.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 3.0f,
        renderingBiasY = 0.0f,
    };

    public static WeaponDatum bossState6 = new WeaponDatum()
    {
        shootInterval = 0.02f,
        virtualYRange = 0.2f,
        virtualYBase = 0.5f,
        angleBiasRange = 3.0f,
        extraBulletsPerSide = 18,
        angle = 20.0f + 0.1f,
        speed = 17.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 1.0f,
        renderingBiasY = 0.0f,
    };

    public static WeaponDatum bossState7Weak = new WeaponDatum()
    {
        shootInterval = 0.01f,
        virtualYRange = 0.2f,
        virtualYBase = 0.5f,
        angleBiasRange = 0f,
        extraBulletsPerSide = 3,
        angle = 360.0f / 7.0f,
        speed = 50.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 0.1f,
        renderingBiasY = 0.0f,
    };

    public static WeaponDatum bossState7Normal = new WeaponDatum()
    {
        shootInterval = 0.005f,
        virtualYRange = 0.2f,
        virtualYBase = 10.0f,
        angleBiasRange = 0f,
        extraBulletsPerSide = 21,
        angle = 360.0f / 7.0f + 0.15f,
        speed = 50.0f,
        radius = 0.07f,
        damage = 1,
        bounces = 2,
        lifeSpan = 12.0f,
        impulse = 2.0f,
        renderingBiasY = 0.0f,
    };
}

public class Weapon
{
    public int index;
    public WeaponDatum datum;

    public float shootIntervalCoeff = 1.0f;
    public float bulletSpeedCoeff = 1.0f;
    public float lastShootTime;
    public Vector3 lastShootPos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 lastShootDir = new Vector3(1.0f, 0.0f, 0.0f);

    public Weapon(int _playerIndex)
    {
        lastShootTime = -0.001f;
        index = _playerIndex;

        Debug.Assert(index == 0 || index == 1);
        if (index == 0)
        {
            datum = WeaponDatumSample.player1Super;
        }
        else
        {
            datum = WeaponDatumSample.player2Super;
        }
    }

    public Weapon(int _playerIndex, WeaponDatum _datum)
    {
        lastShootTime = GameManager.gameTime - 0.0001f;
        index = _playerIndex;

        Debug.Assert(index == 0 || index == 1);
        datum = _datum;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        float currentShootTime = GameManager.gameTime;
        float currentShootInterval = datum.shootInterval * shootIntervalCoeff;
        if (currentShootTime - lastShootTime > 1.0f)
        {
            lastShootTime = currentShootTime;
            return;
        }
        int currentBulletIndex = (int)Mathf.Floor(currentShootTime / currentShootInterval);
        int lastBulletIndex = (int)Mathf.Floor(lastShootTime / currentShootInterval);

        if (currentBulletIndex >= lastBulletIndex) 
        {
            dir = dir.normalized;
            uint packedColor = GUtils.SRGBColorToLinearUInt(index == 0 ? GameManager.instance.player1BulletColor : GameManager.instance.player2BulletColor);
            bool affectedByPlayer1Skill1 = GameManager.playerSkillManager.skills["Player1Skill1"].GetState() == 2;

            for (int bulletIndex = lastBulletIndex + 1; bulletIndex <= currentBulletIndex; bulletIndex++)
            {
                float shootTime = currentShootInterval * bulletIndex;
                float lerpCoeff = (shootTime - lastShootTime) / (currentShootTime - lastShootTime);
                float randomAngleBias = UnityEngine.Random.Range(-datum.angleBiasRange, datum.angleBiasRange);
                
                for (int i = -datum.extraBulletsPerSide; i <= datum.extraBulletsPerSide; i++)
                {
                    float angleOfThisBullet = i * datum.angle + randomAngleBias;
                    Quaternion rotation = Quaternion.Euler(0, angleOfThisBullet, 0);
                    Vector3 interpolatedShootDir = (lerpCoeff * dir + (1.0f - lerpCoeff) * lastShootDir).normalized;
                    Vector3 dirOfThisBullet = rotation * interpolatedShootDir;
                    Vector3 shootPosOfThisBullet = lerpCoeff * pos + (1.0f - lerpCoeff) * lastShootPos;
                    Vector3 currentPosOfThisBullet = shootPosOfThisBullet + (currentShootTime - shootTime) * datum.speed * dirOfThisBullet;
                    float virtualY = UnityEngine.Random.Range(datum.virtualYBase - datum.virtualYRange, datum.virtualYBase + datum.virtualYRange);
                    GameManager.computeCenter.AppendPlayerShootRequest(currentPosOfThisBullet, dirOfThisBullet, datum.speed * bulletSpeedCoeff, datum.radius, datum.damage, datum.bounces, datum.lifeSpan, datum.impulse, virtualY, index, datum.renderingBiasY, packedColor, affectedByPlayer1Skill1);
                }
            }

            lastShootTime = currentShootTime;
            lastShootPos = pos;
            lastShootDir = dir;
        }
    }

    public void SetLastShootTime(float t)
    {
        lastShootTime = t;
    }
}


public class BossWeapon
{
    public WeaponDatum datum;

    public float shootIntervalCoeff = 1.0f;
    public float bulletSpeedCoeff = 1.0f;
    public float lastShootTime;
    public Vector3 lastShootPos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 lastShootDir = new Vector3(1.0f, 0.0f, 0.0f);

    public BossWeapon(WeaponDatum _datum)
    {
        lastShootTime = GameManager.gameTime - 0.0001f;
        datum = _datum;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        float currentShootTime = GameManager.gameTime;
        float currentShootInterval = datum.shootInterval * shootIntervalCoeff;
        int currentBulletIndex = (int)Mathf.Floor(currentShootTime / currentShootInterval);
        int lastBulletIndex = (int)Mathf.Floor(lastShootTime / currentShootInterval);

        if (currentBulletIndex >= lastBulletIndex)
        {
            dir = dir.normalized;
            uint packedColor = GUtils.SRGBColorToLinearUInt(GameManager.instance.bossBulletColor);
            bool affectedByPlayer1Skill1 = GameManager.playerSkillManager.skills["Player1Skill1"].GetState() == 2;

            for (int bulletIndex = lastBulletIndex + 1; bulletIndex <= currentBulletIndex; bulletIndex++)
            {
                float shootTime = currentShootInterval * bulletIndex;
                float lerpCoeff = (shootTime - lastShootTime) / (currentShootTime - lastShootTime);
                float randomAngleBias = UnityEngine.Random.Range(-datum.angleBiasRange, datum.angleBiasRange);

                for (int i = -datum.extraBulletsPerSide; i <= datum.extraBulletsPerSide; i++)
                {
                    float angleOfThisBullet = i * datum.angle + randomAngleBias;
                    Quaternion rotation = Quaternion.Euler(0, angleOfThisBullet, 0);
                    Vector3 interpolatedShootDir = (lerpCoeff * dir + (1.0f - lerpCoeff) * lastShootDir).normalized;
                    Vector3 dirOfThisBullet = rotation * interpolatedShootDir;
                    Vector3 shootPosOfThisBullet = lerpCoeff * pos + (1.0f - lerpCoeff) * lastShootPos;
                    Vector3 currentPosOfThisBullet = shootPosOfThisBullet + (currentShootTime - shootTime) * datum.speed * dirOfThisBullet;
                    float virtualY = UnityEngine.Random.Range(datum.virtualYBase - datum.virtualYRange, datum.virtualYBase + datum.virtualYRange);
                    GameManager.computeCenter.AppendBossShootRequest(currentPosOfThisBullet, dirOfThisBullet, datum.speed * bulletSpeedCoeff, datum.radius, datum.damage, datum.bounces, datum.lifeSpan, datum.impulse, virtualY, datum.renderingBiasY, packedColor);
                }
            }

            lastShootTime = currentShootTime;
            lastShootPos = pos;
            lastShootDir = dir;
        }
    }

    public void SetLastShootTime(float t)
    {
        lastShootTime = t;
    }
}
