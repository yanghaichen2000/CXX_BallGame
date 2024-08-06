using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
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
            datum.shootInterval = 0.06f;
            datum.virtualYRange = 0.2f;
            datum.virtualYBase = 0.5f;
            datum.angleBiasRange = 1.0f;
            datum.extraBulletsPerSide = 60;
            datum.angle = 1.5f;
            datum.speed = 8.0f;
            datum.radius = 0.07f;
            datum.damage = 3;
            datum.bounces = 5;
            datum.lifeSpan = 12.0f;
            datum.impulse = 0.5f;
            datum.renderingBiasY = 0.0f;
        }
        else
        {
            datum.shootInterval = 0.06f;
            datum.virtualYRange = 0.0f;
            datum.virtualYBase = 10.0f;
            datum.angleBiasRange = 1.0f;
            datum.extraBulletsPerSide = 10;
            datum.angle = 1.0f;
            datum.speed = 20.0f;
            datum.radius = 0.07f;
            datum.damage = 1;
            datum.bounces = 2;
            datum.lifeSpan = 12.0f;
            datum.impulse = 4.0f;
            datum.renderingBiasY = 0.1f;
        }
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
                    GameManager.computeCenter.AppendPlayerShootRequest(currentPosOfThisBullet, dirOfThisBullet, datum.speed, datum.radius, datum.damage, datum.bounces, datum.lifeSpan, datum.impulse, virtualY, playerIndex, datum.renderingBiasY);
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
