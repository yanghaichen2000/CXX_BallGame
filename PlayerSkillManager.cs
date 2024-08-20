using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static ComputeManager;

public class PlayerSkillManager
{
    public Dictionary<string, Skill> skills;

    public PlayerSkillManager()
    {
        skills = new Dictionary<string, Skill>();
        skills["Player1Skill0"] = new Player1Skill0();
        skills["Player1Skill1"] = new Player1Skill1();
        skills["Player2Skill0"] = new Player2Skill0();
        skills["Player2Skill1"] = new Player2Skill1();
        skills["SharedSkill0"] = new SharedSkill0();
        skills["SharedSkill1"] = new SharedSkill1();
    }

    public void Update()
    {
        foreach (var pair in skills)
        {
            Skill skill = pair.Value;
            skill.UpdateState();
            skill.UpdateGPUDataAndBuffer();
        }
    }
}


public interface Skill
{
    public void UpdateState();
    public void UpdateGPUDataAndBuffer();

    public int GetState();
}


public class Player1Skill0 : Skill
{
    public float cd = 15.0f;
    public float duration = 5.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    Material player1mat;
    Color player1Color;

    public Player1Skill0()
    {
        player1mat = GameManager.player1.obj.GetComponent<MeshRenderer>().material;
        player1Color = player1mat.GetColor("_BaseColor");
    }

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 0, false);
            if (Input.GetKey(KeyCode.E))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // 已触发
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 0, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);

            float ramainingTime = lastTriggeredTime + duration - GameManager.gameTime;
            if (ramainingTime > 2.0f)
            {
                player1mat.SetColor("_EmissionColor", player1Color * GameManager.instance.playerSkillEmission);
            }
            else
            {
                if (Mathf.FloorToInt(ramainingTime * 4.0f) % 2 == 1)
                {
                    player1mat.SetColor("_EmissionColor", player1Color * 0.5f);
                }
                else
                {
                    player1mat.SetColor("_EmissionColor", player1Color * GameManager.instance.playerSkillEmission);
                }
            }

            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // 冷却中
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 0, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);
            player1mat.SetColor("_EmissionColor", Color.black);
            if (GameManager.gameTime - lastTriggeredTime >= cd)
            {
                state = 0;
            }
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeManager.playerSkillData[0].player1Skill0 = state;
    }

    public int GetState()
    {
        return state;
    }
}

public class Player1Skill1 : Skill
{
    public float cd = 10.0f;
    public float duration = 3.0f;

    public Vector3 aimingPointPosition;
    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    public Player1Skill1()
    {
        GameManager.uiManager.RemoveAimingPoint();
    }

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 1, false);
            if (Input.GetKey(KeyCode.Q))
            {
                GameManager.uiManager.PlaceAimingPointAtMouseLocation();
                state = 1;
            }
        }
        else if (state == 1) // 设置目标
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 1, false);
            GameManager.uiManager.PlaceAimingPointAtMouseLocation();
            if (Input.GetKey(KeyCode.Mouse0))
            {
                Vector3 mousePosition = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(mousePosition);
                if (GameManager.gamePlane.Raycast(ray, out var enter))
                {
                    aimingPointPosition = ray.GetPoint(enter);
                }
                aimingPointPosition = GameManager.basicTransform.InverseTransformPoint(aimingPointPosition);

                lastTriggeredTime = GameManager.gameTime;
                state = 2;
            }
            else if (Input.GetKey(KeyCode.Mouse1))
            {
                GameManager.uiManager.RemoveAimingPoint();
                state = 0;
            }
        }
        if (state == 2) // 执行中
        {
            GameManager.player1.weapon.shootIntervalCoeff = 0.5f;
            GameManager.player2.weapon.shootIntervalCoeff = 0.5f;
            GameManager.player1.weapon.bulletSpeedCoeff = 2.5f;
            GameManager.player2.weapon.bulletSpeedCoeff = 1.25f;
            GameManager.uiManager.UpdatePlayerSkillUI(0, 1, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);
            if (GameManager.gameTime - lastTriggeredTime > duration)
            {
                GameManager.player1.weapon.shootIntervalCoeff = 1.0f;
                GameManager.player2.weapon.shootIntervalCoeff = 1.0f;
                GameManager.player1.weapon.bulletSpeedCoeff = 1.0f;
                GameManager.player2.weapon.bulletSpeedCoeff = 1.0f;
                GameManager.uiManager.RemoveAimingPoint();
                state = 3;
            }
        }
        if (state == 3) // 冷却中
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 1, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);
            if (GameManager.gameTime - lastTriggeredTime > cd)
            {
                state = 0;
            }
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeManager.playerSkillData[0].player1Skill1 = state;
        GameManager.computeManager.playerSkillData[0].player1Skill1AimingPointPosition = aimingPointPosition;
    }

    public int GetState()
    {
        return state;
    }
}

public class Player2Skill0 : Skill
{
    public float cd = 12.0f;
    public float duration = 5.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    Material player2mat;
    Color player2Color;

    public Player2Skill0()
    {
        player2mat = GameManager.player2.obj.GetComponent<MeshRenderer>().material;
        player2Color = player2mat.GetColor("_BaseColor");
    }

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            GameManager.uiManager.UpdatePlayerSkillUI(1, 0, false);
            if (Input.GetKey("joystick button 5"))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // 已触发
        {
            GameManager.uiManager.UpdatePlayerSkillUI(1, 0, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);

            GameManager.player2.maxSpeed = GameManager.player2.initialMaxSpeed * 3.5f;
            GameManager.player2.maxAcceleration = GameManager.player2.initialMaxAcceleration * 10.0f;

            float ramainingTime = lastTriggeredTime + duration - GameManager.gameTime;
            if (ramainingTime > 2.0f)
            {
                player2mat.SetColor("_EmissionColor", player2Color * GameManager.instance.playerSkillEmission);
            }
            else
            {
                if (Mathf.FloorToInt(ramainingTime * 4.0f) % 2 == 1)
                {
                    player2mat.SetColor("_EmissionColor", player2Color * 0.5f);
                }
                else
                {
                    player2mat.SetColor("_EmissionColor", player2Color * GameManager.instance.playerSkillEmission);
                }
            }

            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // 冷却中
        {
            GameManager.uiManager.UpdatePlayerSkillUI(1, 0, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);

            GameManager.player2.maxSpeed = GameManager.player2.initialMaxSpeed;
            GameManager.player2.maxAcceleration = GameManager.player2.initialMaxAcceleration;

            player2mat.SetColor("_EmissionColor", Color.black);

            if (GameManager.gameTime - lastTriggeredTime >= cd)
            {
                state = 0;
            }
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeManager.playerSkillData[0].player2Skill0 = state;
    }

    public int GetState()
    {
        return state;
    }
}

public class Player2Skill1 : Skill
{
    public float cd = 10.0f;

    public static Vector3 availablePosition1 = new Vector3(2.0f, 0.5f, 2.0f);
    public static Vector3 availablePosition2 = new Vector3(-2.0f, 0.5f, -2.0f);
    public static bool canTeleport = false;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            GameManager.uiManager.UpdatePlayerSkillUI(1, 1, false);
            if ((Input.GetKey("joystick button 4") || Input.GetKey(KeyCode.T)) && canTeleport)
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
                GameManager.player1.obj.transform.localPosition = availablePosition1;
                GameManager.player2.obj.transform.localPosition = availablePosition2;
                GameManager.player1.body.velocity = new float3(0.0f, 0.0f, 0.0f);
                GameManager.player2.body.velocity = new float3(0.0f, 0.0f, 0.0f);
                GameManager.cameraMotionManager.ShakeByZDisplacement(2.5f);
            }
        }
        else if (state == 1) // 冷却中
        {
            GameManager.uiManager.UpdatePlayerSkillUI(1, 1, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);
            if (GameManager.gameTime - lastTriggeredTime >= cd)
            {
                state = 0;
            }
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeManager.playerSkillData[0].player2Skill1 = state;
    }

    public int GetState()
    {
        return state;
    }
}

public class SharedSkill0 : Skill
{
    public float cd = 10.0f;
    public float delay = 3.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            /*
            if (Input.GetKey(KeyCode.R))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
            */
            if (Input.GetKey("joystick button 0"))
            {
                state = 2;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // 延迟中，玩家1触发
        {
            if (GameManager.gameTime - lastTriggeredTime >= delay)
            {
                state = 3;
            }
        }
        else if (state == 2) // 延迟中，玩家2触发
        {
            if (GameManager.gameTime - lastTriggeredTime >= delay)
            {
                state = 4;
            }
        }
        else if (state == 3) // 执行，玩家1触发
        {
            GameManager.cameraMotionManager.ShakeByZDisplacement(-1.0f);
            state = 5;
        }
        else if (state == 4) // 执行，玩家2触发
        {
            GameManager.cameraMotionManager.ShakeByZDisplacement(-1.0f);
            state = 5;
        }
        else if (state == 5) // 冷却中
        {

            if (GameManager.gameTime - lastTriggeredTime >= cd)
            {
                state = 0;
            }
        }

        if (state == 0)
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 2, false);
            GameManager.uiManager.UpdatePlayerSkillUI(1, 2, false);
        }
        else
        {
            float remainingTime = cd - (GameManager.gameTime - lastTriggeredTime);
            GameManager.uiManager.UpdatePlayerSkillUI(0, 2, true, remainingTime, cd);
            GameManager.uiManager.UpdatePlayerSkillUI(1, 2, true, remainingTime, cd);
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeManager.playerSkillData[0].sharedSkill0 = state;
        Shader.SetGlobalFloat("sharedSkill0LastTriggeredTime", lastTriggeredTime);
        Shader.SetGlobalFloat("sharedSkill0CdStartTime", lastTriggeredTime + delay);
    }

    public int GetState()
    {
        return state;
    }
}

public class SharedSkill1 : Skill
{
    public float cd = 20.0f;
    public float duration = 4.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    Material player1mat;

    public SharedSkill1()
    {
        player1mat = GameManager.player1.obj.GetComponent<MeshRenderer>().material;
    }

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            if (Input.GetKey(KeyCode.R))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) //已触发
        {
            float ramainingTime = lastTriggeredTime + duration - GameManager.gameTime;
            if (ramainingTime > 2.0f)
            {
                player1mat.SetColor("_BaseColor", GameManager.instance.bossBulletColor);
            }
            else
            {
                if (Mathf.FloorToInt(ramainingTime * 4.0f) % 2 == 1)
                {
                    player1mat.SetColor("_BaseColor", GameManager.player1Color * 0.8f);
                }
                else
                {
                    player1mat.SetColor("_BaseColor", GameManager.instance.bossBulletColor);
                }
            }

            if (GameManager.gameTime - lastTriggeredTime > duration)
            {
                state = 2;
            }
        }
        else if (state == 2)
        {
            if (GameManager.gameTime - lastTriggeredTime > cd)
            {
                state = 0;
            }
        }
        

        if (state == 0)
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 2, false);
        }
        else
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 2, true, cd - (GameManager.gameTime - lastTriggeredTime), cd);
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeManager.playerSkillData[0].sharedSkill1 = state;
    }

    public int GetState()
    {
        return state;
    }
}