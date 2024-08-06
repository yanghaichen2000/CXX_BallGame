using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    public Dictionary<string, TextMeshProUGUI> textData;

    public TextMeshProUGUI player1HP;
    public TextMeshProUGUI player2HP;

    public TextMeshProUGUI player1Skill0;
    public TextMeshProUGUI player2Skill0;

    public TextMeshProUGUI fps;
    public TextMeshProUGUI enemyNum;
    public TextMeshProUGUI enemyBulletNum;
    public TextMeshProUGUI playerBulletNum;

    public UIManager()
    {
        player1HP = GameObject.Find("text_player1HP").GetComponent<TextMeshProUGUI>();
        player2HP = GameObject.Find("text_player2HP").GetComponent<TextMeshProUGUI>();

        player1Skill0 = GameObject.Find("text_player1Skill0").GetComponent<TextMeshProUGUI>();
        player2Skill0 = GameObject.Find("text_player2Skill0").GetComponent<TextMeshProUGUI>();

        fps = GameObject.Find("text_fps").GetComponent<TextMeshProUGUI>();
        enemyNum = GameObject.Find("text_enemyNum").GetComponent<TextMeshProUGUI>();
        enemyBulletNum = GameObject.Find("text_enemyBulletNum").GetComponent<TextMeshProUGUI>();
        playerBulletNum = GameObject.Find("text_playerBulletNum").GetComponent<TextMeshProUGUI>();
    }

    public void UpdatePlayerHP(int index, int value)
    {
        TextMeshProUGUI hpText = index == 0 ? player1HP : player2HP;
        Player player = index == 0 ? GameManager.player1 : GameManager.player2;
        hpText.text = string.Format("HP: {0} / {1}", Mathf.Max(value, 0), player.maxHP);
        if (value > 200) hpText.color = Color.green;
        else if (value > 100) hpText.color = Color.yellow;
        else if (value > 0) hpText.color = Color.red;
        else hpText.color = Color.black;
    }

    public void UpdateFPS(float value)
    {
        fps.text = string.Format("FPS =  {0}", (int)value);
    }

}
