using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


public class PlayerWeapon
{
    public Texture2D UVTexture;

    public Vector3 pos;
    public Vector3 dir;
    public bool desiredShoot;
    [SerializeField] public float shootInterval = 0.06f;
    public DateTime lastShootTime;
    public DateTime currentTime;

    public void Initialize()
    {
        lastShootTime = DateTime.Now;
        UVTexture = Resources.Load<Texture2D>("UV");
    }

    public void Update()
    {
        currentTime = GameManager.currentTime;
        pos = GameManager.playerManager.transform.localPosition;

        desiredShoot = true;

        GetShootDir();
        Shoot();
    }

    public void GetShootDir()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector2 mouseScreenUV = new Vector2(Input.mousePosition.x / Screen.width, 1.0f - Input.mousePosition.y / Screen.height);
        Color pixelColor = UVTexture.GetPixelBilinear(mouseScreenUV.x, mouseScreenUV.y);
        float x = -(pixelColor.r - 0.5f) * 10.0f * 3.3f;
        float z = -(pixelColor.g - 0.5f) * 10.0f * 3.3f;
        dir = (new Vector3(x, 0.5f, z) - GameManager.playerManager.transform.localPosition).normalized;
    }

    public void Shoot()
    {
        if (desiredShoot && (currentTime - lastShootTime).TotalSeconds > shootInterval)
        {
            desiredShoot = false;
            lastShootTime = currentTime;

            var bullet = GameManager.bulletPool.Get();
            bullet.Initialize(
                currentTime,
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
