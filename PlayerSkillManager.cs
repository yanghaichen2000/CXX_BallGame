using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerSkillManager
{
    public Dictionary<string, Skill> skills;

    public PlayerSkillManager()
    {
        skills = new Dictionary<string, Skill>();
        skills["Player1Skill0"] = new Player1Skill0();
        skills["Player2Skill0"] = new Player2Skill0();
        skills["Player2Skill1"] = new Player2Skill1();
        skills["SharedSkill0"] = new SharedSkill0();

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
            GameManager.uiManager.UpdatePlayerSkillUI(0, 0, false, duration - (GameManager.gameTime - lastTriggeredTime));
            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // 冷却中
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 0, true, cd - (GameManager.gameTime - lastTriggeredTime), cd - duration);
            if (GameManager.gameTime - lastTriggeredTime >= cd)
            {
                state = 0;
            }
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeCenter.playerSkillData[0].player1Skill0 = state;
    }

    public int GetState()
    {
        return state;
    }
}

public class Player2Skill0 : Skill
{
    public float cd = 10.0f;
    public float duration = 5.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

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
            GameManager.uiManager.UpdatePlayerSkillUI(1, 0, false, duration - (GameManager.gameTime - lastTriggeredTime));
            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // 冷却中
        {
            GameManager.uiManager.UpdatePlayerSkillUI(1, 0, true, cd - duration - (GameManager.gameTime - lastTriggeredTime));
            if (GameManager.gameTime - lastTriggeredTime >= cd)
            {
                state = 0;
            }
        }
    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeCenter.playerSkillData[0].player2Skill0 = state;
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
            if (Input.GetKey("joystick button 4") && canTeleport)
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
        GameManager.computeCenter.playerSkillData[0].player2Skill1 = state;
    }

    public int GetState()
    {
        return state;
    }
}

public class SharedSkill0 : Skill
{
    public float cd = 3.5f;
    public float delay = 3.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            if (Input.GetKey(KeyCode.R))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
            else if (Input.GetKey("joystick button 0"))
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
        GameManager.computeCenter.playerSkillData[0].sharedSkill0 = state;
        Shader.SetGlobalFloat("sharedSkill0LastTriggeredTime", lastTriggeredTime);
        Shader.SetGlobalFloat("sharedSkill0CdStartTime", lastTriggeredTime + delay);
    }

    public int GetState()
    {
        return state;
    }
}