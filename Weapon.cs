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
    public float shootInterval = 0.08f;
    public DateTime lastShootTime;

    public BasicWeapon()
    {
        lastShootTime = GameManager.currentTime;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        if ((GameManager.currentTime - lastShootTime).TotalSeconds > shootInterval)
        {
            lastShootTime = GameManager.currentTime;
            GameManager.computeCenter.AppendPlayerShootRequest(pos, dir, 7.0f, 0.07f, 1, 5, 6.0f, 0.1f);
        }
    }
}

public class Shotgun : Weapon
{
    public float shootInterval = 0.08f;
    public DateTime lastShootTime;

    public int extraBulletsPerSide = 30;
    public float angle = 2.0f;

    public Shotgun()
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
                GameManager.computeCenter.AppendPlayerShootRequest(pos, dirOfThisBullet, 7.0f, 0.07f, 1, 5, 12.0f, 1.0f);
            }
        }
    }
}
