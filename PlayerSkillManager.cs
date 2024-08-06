using UnityEngine;

public class PlayerSkillManager
{
    Skill[] skills;

    public PlayerSkillManager()
    {
        skills = new Skill[2];
        skills[0] = new Player1Skill0();
        skills[1] = new Player2Skill0();
    }

    public void Tick()
    {
        foreach (Skill skill in skills)
        {
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
}


public class Player1Skill0 : Skill
{
    public float cd = 5.0f;
    public float duration = 2.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            if (Input.GetKey(KeyCode.E))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // 已触发
        {
            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // 冷却中
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
            GameManager.uiManager.player1Skill0.text = "E: Available";
        }
        else if (state == 1 || state == 2)
        {
            GameManager.uiManager.player1Skill0.text = string.Format("E: {0:F2}s",
                cd - (GameManager.gameTime - lastTriggeredTime));
        }
    }

    public void UpdateComputeBufferData()
    {

    }
}

public class Player2Skill0 : Skill
{
    public float cd = 10.0f;
    public float duration = 2.0f;

    public float lastTriggeredTime = -99999.9f;
    public int state = 0;

    public void UpdateState()
    {
        if (state == 0) // 可使用
        {
            if (Input.GetKey("joystick button 5"))
            {
                state = 1;
                lastTriggeredTime = GameManager.gameTime;
            }
        }
        else if (state == 1) // 已触发
        {
            if (GameManager.gameTime - lastTriggeredTime >= duration)
            {
                state = 2;
            }
        }
        else if (state == 2) // 冷却中
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
            GameManager.uiManager.player2Skill0.text = "RB: Available";
        }
        else if (state == 1 || state == 2)
        {
            GameManager.uiManager.player2Skill0.text = string.Format("RB: {0:F2}s",
                cd - (GameManager.gameTime - lastTriggeredTime));
        }
    }

    public void UpdateComputeBufferData()
    {

    }
}