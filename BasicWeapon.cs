using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public interface Weapon
{
    public void Initialize();
    public void Shoot(Vector3 pos, Vector3 dir);
}


public class BasicWeapon : Weapon
{
    public float shootInterval = 0.06f;
    public DateTime lastShootTime;

    public void Initialize()
    {
        lastShootTime = GameManager.currentTime;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        if ((GameManager.currentTime - lastShootTime).TotalSeconds > shootInterval)
        {
            lastShootTime = GameManager.currentTime;

            var bullet = GameManager.bulletPool.Get();
            bullet.Initialize(
                GameManager.currentTime,
                pos,
                dir,
                7.0f,
                0.1f,
                1.0f
                );

            GameManager.bullets.Add(bullet);
        }
    }
}

public class Shotgun : Weapon
{
    public float shootInterval = 0.07f;
    public DateTime lastShootTime;

    public int extraBulletsPerSide = 8;
    public float angle = 2.0f;

    public void Initialize()
    {
        lastShootTime = GameManager.currentTime;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        if ((GameManager.currentTime - lastShootTime).TotalSeconds > shootInterval)
        {
            lastShootTime = GameManager.currentTime;

            float randomDithering = UnityEngine.Random.Range(-1.0f, 1.0f);
            for (int i = -extraBulletsPerSide; i <= extraBulletsPerSide; i++)
            {
                float angleOfThisBullet = i * angle + randomDithering;
                Quaternion rotation = Quaternion.Euler(0, angleOfThisBullet, 0);
                Vector3 dirOfThisBullet = rotation * dir;

                var bullet = GameManager.bulletPool.Get();
                bullet.Initialize(
                    GameManager.currentTime,
                    pos,
                    dirOfThisBullet,
                    7.0f,
                    0.1f,
                    1.0f
                    );

                GameManager.bullets.Add(bullet);
            }
        }
    }
}
