using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillUI
{
    public TextMeshProUGUI text;
    public TextMeshProUGUI textKey;
    public Image image;
    public Image imageMask;

    public PlayerSkillUI(int playerIndex, int skillIndex)
    {
        text = GameObject.Find(string.Format("text_player{0}Skill{1}", playerIndex + 1, skillIndex)).GetComponent<TextMeshProUGUI>();
        textKey = GameObject.Find(string.Format("text_player{0}Skill{1}Key", playerIndex + 1, skillIndex)).GetComponent<TextMeshProUGUI>();
        image = GameObject.Find(string.Format("image_player{0}Skill{1}", playerIndex + 1, skillIndex)).GetComponent<Image>();
        imageMask = GameObject.Find(string.Format("image_player{0}Skill{1}Mask", playerIndex + 1, skillIndex)).GetComponent<Image>();

        Debug.Assert(text != null);
        Debug.Assert(textKey != null);
        Debug.Assert(image != null);
        Debug.Assert(imageMask != null);
    }
}

public class UIManager
{
    public TextMeshProUGUI text_player1HP;
    public TextMeshProUGUI text_player1Mass;
    public Image image_player1HP;

    public TextMeshProUGUI text_player2HP;
    public TextMeshProUGUI text_player2Mass;
    public Image image_player2HP;

    public PlayerSkillUI[,] playerSkillUI;

    public TextMeshProUGUI fps;
    public TextMeshProUGUI enemyNum;
    public TextMeshProUGUI enemyBulletNum;
    public TextMeshProUGUI playerBulletNum;

    

    public UIManager()
    {
        text_player1HP = GameObject.Find("text_player1HP").GetComponent<TextMeshProUGUI>();
        text_player1Mass = GameObject.Find("text_player1Mass").GetComponent<TextMeshProUGUI>();
        image_player1HP = GameObject.Find("image_player1HP").GetComponent<Image>();

        text_player2HP = GameObject.Find("text_player2HP").GetComponent<TextMeshProUGUI>();
        text_player2Mass = GameObject.Find("text_player2Mass").GetComponent<TextMeshProUGUI>();
        image_player2HP = GameObject.Find("image_player2HP").GetComponent<Image>();

        playerSkillUI = new PlayerSkillUI[2, 4];
        for (int p = 0; p < 2; p++)
        {
            for (int i = 0; i < 4; i++)
            {
                playerSkillUI[p, i] = new PlayerSkillUI(p, i);
            }
        }

        fps = GameObject.Find("text_fps").GetComponent<TextMeshProUGUI>();
        enemyNum = GameObject.Find("text_enemyNum").GetComponent<TextMeshProUGUI>();
        enemyBulletNum = GameObject.Find("text_enemyBulletNum").GetComponent<TextMeshProUGUI>();
        playerBulletNum = GameObject.Find("text_playerBulletNum").GetComponent<TextMeshProUGUI>();
    }

    public void UpdatePlayerHP(int index, int value)
    {
        TextMeshProUGUI hpText = index == 0 ? text_player1HP : text_player2HP;
        Player player = index == 0 ? GameManager.player1 : GameManager.player2;
        Image image = index == 0 ? image_player1HP : image_player2HP;

        hpText.text = string.Format("HP: {0} / {1}", Mathf.Max(value, 0), player.maxHP);

        Color color;
        if (value > 200) color = new Color(0.1f, 0.8f, 0.1f);
        else if (value > 100) color = Color.yellow;
        else if (value > 0) color = Color.red;
        else color = Color.black;

        if (image != null)
        {
            image.fillAmount = value / 300.0f;
            image.color = color;
        }
    }

    public void UpdatePlayerMass(int index, float value)
    {
        TextMeshProUGUI massText = index == 0 ?
            GameManager.uiManager.text_player1Mass : GameManager.uiManager.text_player2Mass;
        Player player = index == 0 ? GameManager.player1 : GameManager.player2;

        massText.text = string.Format("Mass: {0:F2} kg", player.m);
    }

    public void UpdatePlayerSkillUI(int playerIndex, int skillIndex, bool isInCd, float remainingTime = -1.0f, float totalTime = 1000.0f)
    {
        PlayerSkillUI ui = playerSkillUI[playerIndex, skillIndex];
        if (isInCd)
        {
            ui.imageMask.fillAmount = remainingTime / totalTime;
            ui.text.color = Color.red;
        }
        else
        {
            ui.imageMask.fillAmount = 0.0f;
            ui.text.color = Color.green;
        }

        if (remainingTime > 0)
        {
            ui.text.text = string.Format("{0:F1}s", remainingTime);
        }
        else
        {
            ui.text.text = " ";
        }
    }

    public void UpdateFPS(float value)
    {
        fps.text = string.Format("FPS =  {0}", (int)value);
    }

}
