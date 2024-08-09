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

    public void StartLevel()
    {
        levelStartTime = GameManager.gameTime;
        currentWave = 0;

        nextWaveTime = GameManager.gameTime + 3.0f;
        nextWave = Wave1;

        GameManager.player1.weapon = new Weapon(0, PlayerWeaponDatumSample.player1Initial);
        GameManager.player2.weapon = new Weapon(1, PlayerWeaponDatumSample.player2Initial);
    }

    public void Update()
    {
        if (GameManager.gameTime >= nextWaveTime && nextWave != null)
        {
            nextWave();
        }

        GameManager.uiManager.UpdateNextWave(nextWaveTime - GameManager.gameTime);
    }

    public int Wave1()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(10.0f, 10.0f, 0);
        GameManager.enemyLegion.SpawnSphereEnemy(12.0f, 10.0f, 0);

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

        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = Wave3;
        return 0;
    }

    public int Wave3()
    {
        GameManager.enemyLegion.SpawnSphereEnemy(-10.0f, 10.0f, 1);
        GameManager.enemyLegion.SpawnSphereEnemy(-12.0f, 10.0f, 1);
        GameManager.enemyLegion.SpawnSphereEnemy(-10.0f, 12.0f, 1);
        GameManager.enemyLegion.SpawnSphereEnemy(-12.0f, 12.0f, 1);
        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, 10.0f, 1);
        GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, 10.0f, 1);
        GameManager.enemyLegion.SpawnSphereEnemy(-14.0f, 12.0f, 1);
        GameManager.enemyLegion.SpawnSphereEnemy(-16.0f, 12.0f, 1);

        nextWaveTime = GameManager.gameTime + 30.0f;
        nextWave = null;
        return 0;
    }
}
