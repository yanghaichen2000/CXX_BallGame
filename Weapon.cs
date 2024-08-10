using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public struct PlayerWeaponDatum
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

public class PlayerWeaponDatumSample
{
    public static PlayerWeaponDatum player1Super = new PlayerWeaponDatum()
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

    public static PlayerWeaponDatum player1Weak = new PlayerWeaponDatum()
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

    public static PlayerWeaponDatum player1Initial = new PlayerWeaponDatum()
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

    public static PlayerWeaponDatum player2Super = new PlayerWeaponDatum()
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
        impulse = 5.0f,
        renderingBiasY = 0.1f,
    };

    public static PlayerWeaponDatum player2Weak = new PlayerWeaponDatum()
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

    public static PlayerWeaponDatum player2Initial = new PlayerWeaponDatum()
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
}

public class Weapon
{
    public int playerIndex;
    PlayerWeaponDatum datum;

    public float lastShootTime;
    public Vector3 lastShootPos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 lastShootDir = new Vector3(1.0f, 0.0f, 0.0f);

    public Weapon(int _playerIndex)
    {
        lastShootTime = -0.001f;
        playerIndex = _playerIndex;

        Debug.Assert(playerIndex == 0 || playerIndex == 1);
        if (playerIndex == 0)
        {
            datum = PlayerWeaponDatumSample.player1Super;
        }
        else
        {
            datum = PlayerWeaponDatumSample.player2Super;
        }
    }

    public Weapon(int _playerIndex, PlayerWeaponDatum _datum)
    {
        lastShootTime = GameManager.gameTime - 0.0001f;
        playerIndex = _playerIndex;

        Debug.Assert(playerIndex == 0 || playerIndex == 1);
        datum = _datum;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        float currentShootTime = GameManager.gameTime;
        int currentBulletIndex = (int)Mathf.Floor(currentShootTime / datum.shootInterval);
        int lastBulletIndex = (int)Mathf.Floor(lastShootTime / datum.shootInterval);

        if (currentBulletIndex != lastBulletIndex) 
        {
            dir = dir.normalized;

            for (int bulletIndex = lastBulletIndex + 1; bulletIndex <= currentBulletIndex; bulletIndex++)
            {
                float shootTime = datum.shootInterval * bulletIndex;
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
                    UInt32 packedColor = GUtils.SRGBColorToLinearUInt32(playerIndex == 0 ? GameManager.instance.player1BulletColor : GameManager.instance.player2BulletColor);
                    GameManager.computeCenter.AppendPlayerShootRequest(currentPosOfThisBullet, dirOfThisBullet, datum.speed, datum.radius, datum.damage, datum.bounces, datum.lifeSpan, datum.impulse, virtualY, playerIndex, datum.renderingBiasY, packedColor);
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
