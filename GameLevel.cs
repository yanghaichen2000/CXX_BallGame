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

    public void StartLevel()
    {
        levelStartTime = GameManager.gameTime;
        currentWave = 0;

        nextWaveTime = GameManager.gameTime + 3.0f;
        nextWave = Wave1;

        GameManager.player1.weapon = GameManager.allLevelPlayerData.GetWeapon(0, 0);
        GameManager.player2.weapon = GameManager.allLevelPlayerData.GetWeapon(1, 0);
    }

    public void Update()
    {
        if (GameManager.gameTime >= nextWaveTime && nextWave != null)
        {
            nextWave();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            nextWaveTime = GameManager.gameTime + 0.25f;
        }

        GameManager.uiManager.UpdateCurrentWave(currentWave);
        GameManager.uiManager.UpdateNextWaveTime(nextWaveTime - GameManager.gameTime);
    }

    public int Wave1()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(10.0f, 10.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(12.0f, 10.0f, 0);

        currentWave = 1;
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

        currentWave = 2;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave3;
        return 0;
    }

    public int Wave3()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(10.0f + i * 2.0f, 5.0f + j * 2.0f, 0);
            }
        }

        currentWave = 3;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave4;
        return 0;
    }

    public int Wave4()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(13.0f + i * 2.0f, -10.0f + j * 2.0f, 1);
            }
        }

        currentWave = 4;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave5;
        return 0;
    }

    public int Wave5()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(10.0f + i * 2.0f, -10.0f + j * 2.0f, 1);
            }
        }

        currentWave = 5;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave6;
        return 0;
    }

    public int Wave6()
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 6.0f + j * 1.6f, 2);
            }
        }

        currentWave = 6;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave7;
        return 0;
    }

    public int Wave7()
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 6.0f + j * 1.6f, 2);
            }
        }

        currentWave = 7;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave8;
        return 0;
    }

    public int Wave8()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 6.0f + j * 1.6f, 3);
            }
        }

        currentWave = 8;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave9;
        return 0;
    }

    public int Wave9()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 6.0f + j * 1.6f, 3);
            }
        }

        currentWave = 9;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave10;
        return 0;
    }

    public int Wave10()
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 10.0f + j * 1.6f, 4);
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, -10.0f - j * 1.6f, 4);
            }
        }

        currentWave = 10;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave11;
        return 0;
    }

    public int Wave11()
    {
        for (int i = 0; i < 22; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 8.0f + j * 1.4f, 4);
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, -8.0f - j * 1.4f, 4);
            }
        }

        currentWave = 11;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave12;
        return 0;
    }

    public int Wave12()
    {
        for (int i = 0; i < 22; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, 8.0f + j * 1.4f, 5);
                GameManager.enemyLegion.SpawnSphereEnemy(-18.0f + i * 1.6f, -8.0f - j * 1.4f, 5);
            }
        }

        currentWave = 12;
        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave13;
        return 0;
    }

    public int Wave13()
    {
        GameManager.enemyLegion.CreateSpawnEnemyRequest(300, GameManager.allEnemyProperty.enemyPropertyData[5]);

        currentWave = 13;
        nextWaveTime = GameManager.gameTime + 40.0f;
        nextWave = Wave14;
        return 0;
    }

    public int Wave14()
    {
        GameManager.enemyLegion.CreateSpawnEnemyRequest(420, GameManager.allEnemyProperty.enemyPropertyData[6]);

        currentWave = 14;
        nextWaveTime = GameManager.gameTime + 40.0f;
        nextWave = Wave15;
        return 0;
    }

    public int Wave15()
    {
        GameManager.enemyLegion.CreateSpawnEnemyRequest(600, GameManager.allEnemyProperty.enemyPropertyData[7]);

        currentWave = 15;
        nextWaveTime = GameManager.gameTime + 40.0f;
        nextWave = null;
        return 0;
    }
}
