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
    public Vector2 playerInput;
    public Vector3 shootTarget;

    public KeyboardInputManager()
    {
        shootTarget = new Vector3(100.0f, 0, 0);
    }

    public void Update()
    {
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
    }

    public Vector2 GetPlayerMovementInput()
    {
        return playerInput;
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