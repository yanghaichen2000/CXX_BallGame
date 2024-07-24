using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerWeapon weapon = new PlayerWeapon();
    public PlayerMovementManager movementManager = new PlayerMovementManager();

    void Start()
    {
        movementManager.Initialize();
        weapon.Initialize();
    }

    void Update()
    {
        movementManager.Update();
        weapon.Update();
    }

    void FixedUpdate()
    {
        movementManager.FixedUpdate();
    }
}
