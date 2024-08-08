using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillManager
{
    public Dictionary<string, Skill> skills;

    public PlayerSkillManager()
    {
        skills = new Dictionary<string, Skill>();
        skills["Player1Skill0"] = new Player1Skill0();
        skills["Player2Skill0"] = new Player2Skill0();
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
        if (state == 0) // ��ʹ��
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 0, false);
            if (Input.GetKey(KeyCode.E))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // �Ѵ���
        {
            GameManager.uiManager.UpdatePlayerSkillUI(0, 0, false, duration - (GameManager.gameTime - lastTriggeredTime));
            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // ��ȴ��
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
        if (state == 0) // ��ʹ��
        {
            if (Input.GetKey("joystick button 5"))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // �Ѵ���
        {
            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // ��ȴ��
        {
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

public class SharedSkill0 : Skill
{
    public float cd = 3.5f;
    public float delay = 3.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    public void UpdateState()
    {
        if (state == 0) // ��ʹ��
        {
            if (Input.GetKey("joystick button 0"))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
            else if (Input.GetKey(KeyCode.R))
            {
                state = 2;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // �ӳ��У����1����
        {
            if (GameManager.gameTime - lastTriggeredTime >= delay)
            {
                state = 3;
            }
        }
        else if (state == 2) // �ӳ��У����2����
        {
            if (GameManager.gameTime - lastTriggeredTime >= delay)
            {
                state = 4;
            }
        }
        else if (state == 3) // ִ�У����1����
        {
            GameManager.cameraMotionManager.ShakeByNegativeZDisplacement();
            state = 5;
        }
        else if (state == 4) // ִ�У����2����
        {
            GameManager.cameraMotionManager.ShakeByNegativeZDisplacement();
            state = 5;
        }
        else if (state == 5) // ��ȴ��
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

    public void UpdateUI()
    {

    }

    public void UpdateGPUDataAndBuffer()
    {
        GameManager.computeCenter.playerSkillData[0].sharedSkill0 = state;
        Shader.SetGlobalFloat("sharedSkill0LastTriggeredTime", lastTriggeredTime);
    }

    public int GetState()
    {
        return state;
    }
}