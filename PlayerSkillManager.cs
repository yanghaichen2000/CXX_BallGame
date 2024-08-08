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

    public void Tick()
    {
        foreach (var pair in skills)
        {
            Skill skill = pair.Value;
            skill.UpdateState();
            skill.UpdateUI();
            skill.UpdateComputeBufferData();
        }
    }
}


public interface Skill
{
    public void UpdateState();
    public void UpdateUI();
    public void UpdateComputeBufferData();

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
            if (Input.GetKey(KeyCode.E))
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

    public void UpdateUI()
    {
        if (state == 0)
        {
            GameManager.uiManager.player1Skill0.text = "E: Ready";
            GameManager.uiManager.player1Skill0.color = Color.white;
        }
        else if (state == 1)
        {
            GameManager.uiManager.player1Skill0.text = string.Format("E: {0:F2}s",
                duration - (GameManager.gameTime - lastTriggeredTime));
            GameManager.uiManager.player1Skill0.color = Color.green;
        }
        else if (state == 2)
        {
            GameManager.uiManager.player1Skill0.text = string.Format("E: {0:F2}s",
                cd - (GameManager.gameTime - lastTriggeredTime));
            GameManager.uiManager.player1Skill0.color = Color.red;
        }
    }

    public void UpdateComputeBufferData()
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

    public void UpdateUI()
    {
        if (state == 0)
        {
            GameManager.uiManager.player2Skill0.text = "RB: Ready";
            GameManager.uiManager.player2Skill0.color = Color.white;
        }
        else if (state == 1)
        {
            GameManager.uiManager.player2Skill0.text = string.Format("RB: {0:F2}s",
                duration - (GameManager.gameTime - lastTriggeredTime));
            GameManager.uiManager.player2Skill0.color = Color.green;
        }
        else if (state == 2)
        {
            GameManager.uiManager.player2Skill0.text = string.Format("RB: {0:F2}s",
                cd - (GameManager.gameTime - lastTriggeredTime));
            GameManager.uiManager.player2Skill0.color = Color.red;
        }
    }

    public void UpdateComputeBufferData()
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
    public float cd = 2.5f;
    public float delay = 2.0f;

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
            state = 5;
        }
        else if (state == 4) // ִ�У����2����
        {
            state = 5;
        }
        else if (state == 5) // ��ȴ��
        {
            if (GameManager.gameTime - lastTriggeredTime >= cd)
            {
                state = 0;
            }
        }
    }

    public void UpdateUI()
    {

    }

    public void UpdateComputeBufferData()
    {
        GameManager.computeCenter.playerSkillData[0].sharedSkill0 = state;
    }

    public int GetState()
    {
        return state;
    }
}