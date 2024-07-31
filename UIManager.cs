using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    public TextMeshProUGUI player1HP;

    public UIManager()
    {
        player1HP = GameObject.Find("text_player1_hp").GetComponent<TextMeshProUGUI>();
    }

    public void UpdatePlayerHP(int index, int value)
    {
        if (index == 0)
        {
            player1HP.text = string.Format("HP: {0}", Mathf.Max(value, 0));
            if (value > 200) player1HP.color = Color.green;
            else if (value > 100) player1HP.color = Color.yellow;
            else if (value > 0) player1HP.color = Color.red;
            else player1HP.color = Color.black;
        }
    }
}
