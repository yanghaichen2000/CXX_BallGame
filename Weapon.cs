using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public interface Weapon
{
    public void Shoot(Vector3 pos, Vector3 dir);
}


public class BasicWeapon : Weapon
{
    public float shootInterval = 0.06f;
    public DateTime lastShootTime;
    public BulletManager bulletManager;

    public BasicWeapon(BulletManager _bulletManager)
    {
        lastShootTime = GameManager.currentTime;
        bulletManager = _bulletManager;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        if ((GameManager.currentTime - lastShootTime).TotalSeconds > shootInterval)
        {
            lastShootTime = GameManager.currentTime;
            bulletManager.ShootOneBullet(pos, dir, 0.0f, 0.1f, 1.0f);
        }
    }
}

public class Shotgun : Weapon
{
    public float shootInterval = 0.05f;
    public DateTime lastShootTime;
    public BulletManager bulletManager;

    public int extraBulletsPerSide = 12;
    public float angle = 2.0f;

    public Shotgun(BulletManager _bulletManager)
    {
        lastShootTime = GameManager.currentTime;
        bulletManager = _bulletManager;
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
                bulletManager.ShootOneBullet(pos, dirOfThisBullet, 7.0f, 0.07f, 1.0f);
            }
        }
    }
}
