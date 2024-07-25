using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlayerInputManager
{
    public void Update();

    public Vector2 GetPlayerMovementInput();
    public Vector3 GetShootDir(Vector3 playerPos);
}


public class KeyboardInputManager : PlayerInputManager
{
    public Vector2 playerMovementInput;
    public Vector3 shootTarget;

    public KeyboardInputManager()
    {
        shootTarget = new Vector3(100.0f, 0, 0);
    }

    public void Update()
    {
        playerMovementInput.x = Input.GetAxis("Horizontal");
        playerMovementInput.y = Input.GetAxis("Vertical");
        playerMovementInput = Vector2.ClampMagnitude(playerMovementInput, 1f);
    }

    public Vector2 GetPlayerMovementInput()
    {
        return playerMovementInput;
    }

    public Vector3 GetShootDir(Vector3 playerPos)
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (GameManager.gamePlane.Raycast(ray, out var enter))
        {
            shootTarget = ray.GetPoint(enter);
        }
        shootTarget = GameManager.basicTransform.InverseTransformPoint(shootTarget);

        return (shootTarget - playerPos).normalized;
    }
}

public class ControllerInputManager : PlayerInputManager
{
    public Vector2 playerMovementInput;
    public Vector3 shootTarget;

    public ControllerInputManager()
    {
        shootTarget = new Vector3(0, 0, 0);
    }

    public void Update()
    {
        playerMovementInput = new Vector2(0.0f, 0.0f);
    }

    public Vector2 GetPlayerMovementInput()
    {
        playerMovementInput.x = Input.GetAxis("J_Horizontal");
        playerMovementInput.y = Input.GetAxis("J_Vertical");
        playerMovementInput = Vector2.ClampMagnitude(playerMovementInput, 1f);
        return playerMovementInput;
    }

    public Vector3 GetShootDir(Vector3 playerPos)
    {
        float rightStickHorizontal = Input.GetAxis("R_Horizontal");
        float rightStickVertical = Input.GetAxis("R_Vertical");
        Vector3 shootDir = new Vector3(rightStickHorizontal, 0.0f, rightStickVertical).normalized;
        return shootDir;
    }
}