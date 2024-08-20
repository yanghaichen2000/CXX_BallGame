using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class GameLevel
{
    public float levelStartTime;
    public int currentWave;
    public float nextWaveTime;
    public Func<int> nextWave;

    public int currentEnemyNum;
    public int currentDeployingEnemyNum;
    public float currentWaveStartTime;
    public bool canAutoSkipThisWave;

    public void StartLevel1()
    {
        GameManager.instance.UpdateTime();
        levelStartTime = GameManager.gameTime;
        currentWave = 0;
        canAutoSkipThisWave = false;

        nextWaveTime = GameManager.gameTime + 3.0f;
        nextWave = Wave1;

        GameManager.player1.Load();
        GameManager.player2.Load();
        GameManager.player1.exp = 0;
        GameManager.player2.exp = 0;
        GameManager.player1.level = 0;
        GameManager.player2.level = 0;
        GameManager.player1.weapon = GameManager.allLevelPlayerData.GetWeapon(0, 0);
        GameManager.player2.weapon = GameManager.allLevelPlayerData.GetWeapon(1, 0);
    }

    public void StartLevel17()
    {
        GameManager.instance.UpdateTime();
        levelStartTime = GameManager.gameTime;
        currentWave = 16;
        canAutoSkipThisWave = false;

        nextWaveTime = GameManager.gameTime + 3.0f;
        nextWave = Wave17;

        GameManager.player1.Load();
        GameManager.player2.Load();
        GameManager.player1.exp = 2400;
        GameManager.player2.exp = 2400;
        GameManager.player1.level = 18;
        GameManager.player2.level = 18;
        GameManager.player1.weapon = GameManager.allLevelPlayerData.GetWeapon(0, 18);
        GameManager.player2.weapon = GameManager.allLevelPlayerData.GetWeapon(1, 18);
    }

    public void StartLevel20()
    {
        GameManager.instance.UpdateTime();
        levelStartTime = GameManager.gameTime;
        currentWave = 19;
        canAutoSkipThisWave = false;

        nextWaveTime = GameManager.gameTime + 3.0f;
        nextWave = Wave20;

        GameManager.player1.Load();
        GameManager.player2.Load();
        GameManager.player1.exp = 5000;
        GameManager.player2.exp = 5000;
        GameManager.player1.level = 20;
        GameManager.player2.level = 20;
        GameManager.player1.weapon = GameManager.allLevelPlayerData.GetWeapon(0, 20);
        GameManager.player2.weapon = GameManager.allLevelPlayerData.GetWeapon(1, 20);
    }

    public void Update()
    {
        if (GameManager.gameTime >= nextWaveTime && nextWave != null)
        {
            nextWave();
            canAutoSkipThisWave = true;
            currentWaveStartTime = GameManager.gameTime;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            nextWaveTime = GameManager.gameTime + 0.25f;
        }
        else if (currentEnemyNum == 0 && currentDeployingEnemyNum == 0 && currentWave != 20 &&
            canAutoSkipThisWave && GameManager.gameTime - currentWaveStartTime > 5.0f)
        {
            nextWaveTime = GameManager.gameTime + 2.0f;
            canAutoSkipThisWave = false;
        }

        GameManager.uiManager.UpdateCurrentWave(currentWave);
        GameManager.uiManager.UpdateNextWaveTime(nextWaveTime - GameManager.gameTime);
    }

    public int Wave1()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(10.0f, 10.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(12.0f, 10.0f, 0);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave2;
        return 0;
    }

    public int Wave2()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(10.0f, 10.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(12.0f, 10.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(10.0f, 12.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(12.0f, 12.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(14.0f, 10.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(16.0f, 10.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(14.0f, 12.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(16.0f, 12.0f, 0);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave3;
        return 0;
    }


    public int Wave3()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(13.0f + i * 2.0f, -10.0f + j * 2.0f, 1);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave4;
        return 0;
    }

    public int Wave4()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(10.0f + i * 2.0f, -10.0f + j * 2.0f, 1);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave5;
        return 0;
    }

    public int Wave5()
    {
        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 6.0f + j * 1.6f, 2);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave6;
        return 0;
    }

    public int Wave6()
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 6.0f + j * 1.6f, 2);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave7;
        return 0;
    }

    public int Wave7()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 6.0f + j * 1.6f, 3);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave8;
        return 0;
    }

    public int Wave8()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-13.0f, -9.0f, 14, 10, 3, 2.0f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave9;
        return 0;
    }

    public int Wave9()
    {
        for (int i = 0; i < 18; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 9.0f + j * 1.6f, 4);
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, -9.0f - j * 1.6f, 4);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave10;
        return 0;
    }

    public int Wave10()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-19.2f + i * 1.6f, 8.0f + j * 1.4f, 4);
                GameManager.enemyLegion.SpawnSphereEnemy(-19.2f + i * 1.6f, -8.0f - j * 1.4f, 4);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave11;
        return 0;
    }

    public int Wave11()
    {
        for (int i = 0; i < 25; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-19.2f + i * 1.6f, 8.0f + j * 1.4f, 5);
                GameManager.enemyLegion.SpawnSphereEnemy(-19.2f + i * 1.6f, -8.0f - j * 1.4f, 5);
            }
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave12;
        return 0;
    }

    public int Wave12()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -5.0f, 19, 10, 5, 2.0f, 2.0f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 40.0f;
        nextWave = Wave13;
        return 0;
    }

    public int Wave13()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, 10.0f, 20, 4, 6, 2.0f, 1.4f);
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, -10.0f, 20, 4, 6, 2.0f, 1.4f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 40.0f;
        nextWave = Wave14;
        return 0;
    }

    public int Wave14()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, 9.0f, 21, 3, 7, 1.6f, 1.6f);
        GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, -13.8f, 21, 3, 7, 1.6f, 1.6f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 40.0f;
        nextWave = Wave15;
        return 0;
    }

    public int Wave15()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, -14.0f, 10, 10, 1, 1.4f, 1.4f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, 14.0f, 10, 10, 3, 1.4f, -1.4f, 5.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(19.0f, 14.0f, 10, 10, 5, -1.4f, -1.4f, 10.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(19.0f, -14.0f, 10, 10, 7, -1.4f, 1.4f, 15.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, -14.0f, 10, 10, 7, 1.4f, 1.4f, 20.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, 14.0f, 10, 10, 7, 1.4f, -1.4f, 25.0f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 45.0f;
        nextWave = Wave16;
        return 0;
    }

    public int Wave16()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-1.4f, -14.0f, 2, 15, 8, 1.4f, 2.0f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-1.4f, -14.0f, 2, 15, 8, 1.4f, 2.0f, 5.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-1.4f, -14.0f, 2, 15, 8, 1.4f, 2.0f, 10.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-1.4f, -14.0f, 2, 15, 8, 1.4f, 2.0f, 15.0f);

        for (int i = 0; i < 3; i++)
        {
            GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, 13.0f, 19, 1, 7, 2.0f, 2.0f, 20.0f + i * 5.0f);
            GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -13.0f, 19, 1, 7, 2.0f, 2.0f, 20.0f + i * 5.0f);
            GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -11.0f, 1, 12, 7, 2.0f, 2.0f, 20.0f + i * 5.0f);
            GameManager.enemyLegion.SpawnSphereEnemy(18.0f, -11.0f, 1, 12, 7, 2.0f, 2.0f, 20.0f + i * 5.0f);
        }

        currentWave++;
        nextWaveTime = GameManager.gameTime + 45.0f;
        nextWave = Wave17;
        return 0;
    }

    public int Wave17()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, 13.0f, 19, 1, 8, 2.0f, 2.0f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -13.0f, 19, 1, 8, 2.0f, 2.0f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -11.0f, 1, 12, 8, 2.0f, 2.0f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(18.0f, -11.0f, 1, 12, 8, 2.0f, 2.0f, 0.0f);

        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 7, 2.0f, 2.0f, 0.0f);

        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, 13.0f, 19, 1, 7, 2.0f, 2.0f, 3.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -13.0f, 19, 1, 7, 2.0f, 2.0f, 3.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -11.0f, 1, 12, 7, 2.0f, 2.0f, 3.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(18.0f, -11.0f, 1, 12, 7, 2.0f, 2.0f, 3.0f);

        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 8, 2.0f, 2.0f, 3.0f);

        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, -14.0f, 21, 2, 5, 1.4f, 1.4f, 8.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, 14.0f, 21, 2, 5, 1.4f, -1.4f, 8.0f);

        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 8, 2.0f, 2.0f, 10.5f);

        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, -14.0f, 21, 2, 6, 1.4f, 1.4f, 13.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, 14.0f, 21, 2, 6, 1.4f, -1.4f, 13.0f);

        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 7, 2.0f, 2.0f, 15.5f);

        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, -14.0f, 21, 2, 8, 1.4f, 1.4f, 18.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, 14.0f, 21, 2, 8, 1.4f, -1.4f, 18.0f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 45.0f;
        nextWave = Wave18;
        return 0;
    }

    public int Wave18()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, -14.0f, 8, 8, 8, 1.2f, 1.2f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-19.0f, 14.0f, 8, 8, 8, 1.2f, -1.2f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(19.0f, 14.0f, 8, 8, 8, -1.2f, -1.2f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(19.0f, -14.0f, 8, 8, 8, -1.2f, 1.2f, 0.0f);

        for (int i = 0; i < 5; i++)
        {
            GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, 13.0f, 19, 1, 6, 2.0f, 2.0f, 10.0f + i * 6.0f);
            GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -13.0f, 19, 1, 5, 2.0f, 2.0f, 10.0f + i * 6.0f);
            GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -11.0f, 1, 12, 5, 2.0f, 2.0f, 10.0f + i * 6.0f);
            GameManager.enemyLegion.SpawnSphereEnemy(18.0f, -11.0f, 1, 12, 6, 2.0f, 2.0f, 10.0f + i * 6.0f);
        }

        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 8, 2.0f, 2.0f, 13.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 5, 2.0f, 2.0f, 19.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 7, 2.0f, 2.0f, 25.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 4, 2.0f, 2.0f, 31.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-8.0f, -8.0f, 9, 9, 8, 2.0f, 2.0f, 37.0f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 50.0f;
        nextWave = Wave19;
        return 0;
    }

    public int Wave19()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 1, 3.0f, 3.0f, 0.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 2, 3.0f, 3.0f, 3.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 3, 3.0f, 3.0f, 6.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 4, 3.0f, 3.0f, 9.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 5, 3.0f, 3.0f, 12.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 6, 3.0f, 3.0f, 15.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 7, 3.0f, 3.0f, 18.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 8, 3.0f, 3.0f, 21.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 8, 3.0f, 3.0f, 24.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 8, 3.0f, 3.0f, 27.0f);
        GameManager.enemyLegion.SpawnSphereEnemy(-18.0f, -12.0f, 13, 9, 7, 3.0f, 3.0f, 30.0f);

        currentWave++;
        nextWaveTime = GameManager.gameTime + 60.0f;
        nextWave = Wave20;
        return 0;
    }

    public int Wave20()
    {
        GameManager.boss.Load();

        currentWave++;
        nextWaveTime = GameManager.gameTime - 1.0f;
        nextWave = null;
        return 0;
    }
}
