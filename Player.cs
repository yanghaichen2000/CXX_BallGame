using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public Weapon weapon = new Shotgun();
    public PlayerMovementManager movementManager = new PlayerMovementManager();
    public Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));
    public Vector3 shootTarget = new Vector3();

    public void Initialize()
    {
        movementManager.Initialize();
        weapon.Initialize();
    }

    public void Update()
    {
        movementManager.Update();
        Vector3 shootDir = GetShootDir();
        weapon.Shoot(GameManager.playerObj.transform.localPosition, shootDir);
    }

    public void FixedUpdate()
    {
        movementManager.FixedUpdate();
    }

    public Vector3 GetShootDir()
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (gamePlane.Raycast(ray, out var enter))
        {
            shootTarget = ray.GetPoint(enter);
        }
        shootTarget = GameManager.basicTransform.InverseTransformPoint(shootTarget);

        return (shootTarget - GameManager.playerObj.transform.localPosition).normalized;
    }
}
