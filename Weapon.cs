using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;


public class Weapon
{
    public int playerIndex;
    public float shootInterval = 0.06f;
    public float virtualYRange = 0.2f;
    public float virtualYBase = 0.5f;
    public float angleBiasRange = 1.0f;
    public int extraBulletsPerSide = 8;
    public float angle = 3.0f;
    public float speed = 8.0f;
    public float radius = 0.07f;
    public int damage = 1;
    public int bounces = 5;
    public float lifeSpan = 12.0f;
    public float impulse = 1.0f;
    public float renderingBiasY = 0.0f;

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
            impulse = 0.5f;
            extraBulletsPerSide = 20;
            shootInterval = 0.03f;
            angle = 2.0f;
        }
        else
        {
            impulse = 1.0f;
            extraBulletsPerSide = 4;
            shootInterval = 0.02f;
            angle = 2.0f;
            speed = 20.0f;
            angleBiasRange = 0.2f;
            virtualYBase = 10.0f;
            virtualYRange = 0.0f;
            bounces = 2;
            renderingBiasY = 0.1f;
        }
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        float currentShootTime = GameManager.gameTime;
        int currentBulletIndex = (int)Mathf.Floor(currentShootTime / shootInterval);
        int lastBulletIndex = (int)Mathf.Floor(lastShootTime / shootInterval);

        if (currentBulletIndex != lastBulletIndex) 
        {
            dir = dir.normalized;

            for (int bulletIndex = lastBulletIndex + 1; bulletIndex <= currentBulletIndex; bulletIndex++)
            {
                float shootTime = shootInterval * bulletIndex;
                float lerpCoeff = (shootTime - lastShootTime) / (currentShootTime - lastShootTime);
                float randomAngleBias = UnityEngine.Random.Range(-angleBiasRange, angleBiasRange);
                
                for (int i = -extraBulletsPerSide; i <= extraBulletsPerSide; i++)
                {
                    float angleOfThisBullet = i * angle + randomAngleBias;
                    Quaternion rotation = Quaternion.Euler(0, angleOfThisBullet, 0);
                    Vector3 interpolatedShootDir = (lerpCoeff * dir + (1.0f - lerpCoeff) * lastShootDir).normalized;
                    Vector3 dirOfThisBullet = rotation * interpolatedShootDir;
                    Vector3 shootPosOfThisBullet = lerpCoeff * pos + (1.0f - lerpCoeff) * lastShootPos;
                    Vector3 currentPosOfThisBullet = shootPosOfThisBullet + (currentShootTime - shootTime) * speed * dirOfThisBullet;
                    float virtualY = UnityEngine.Random.Range(virtualYBase - virtualYRange, virtualYBase + virtualYRange);
                    GameManager.computeCenter.AppendPlayerShootRequest(currentPosOfThisBullet, dirOfThisBullet, speed, radius, damage, bounces, lifeSpan, impulse, virtualY, playerIndex, renderingBiasY);
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
