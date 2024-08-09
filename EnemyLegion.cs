using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct EnemyProperty
{
    public int hp;
    public int weapon;
    public float mass;
    public float acceleration;
    public float frictionalDeceleration;
    public float maxSpeed;
}

public class AllEnemyProperty
{
    public EnemyProperty[] enemyPropertyData;

    public AllEnemyProperty()
    {
        enemyPropertyData = new EnemyProperty[2];

        enemyPropertyData[0] = new EnemyProperty()
        {
            hp = 50,
            weapon = 0,
            mass = 1.0f,
            acceleration = 1.0f,
            frictionalDeceleration = 0.5f,
            maxSpeed = 1.0f,
        };

        enemyPropertyData[1] = new EnemyProperty()
        {
            hp = 50,
            weapon = 1,
            mass = 1.0f,
            acceleration = 1.0f,
            frictionalDeceleration = 0.5f,
            maxSpeed = 1.0f,
        };
    }
}


public class EnemyLegion
{
    public float x;
    public float z;
    public int remainingNum;
    public int dir;
    public float lastSpawnEnemyTime;
    public float spawnEnemyInterval;

    public EnemyLegion()
    {
        remainingNum = 0;
        lastSpawnEnemyTime = -9999.9f;
        spawnEnemyInterval = 0.06f;
    }

    public void SpawnSphereEnemy(float x, float z)
    {
        GameManager.computeCenter.AppendCreateSphereEnemyRequest(
            new Vector3(x, 0.5f, z),
            new Vector3(0.0f, 0.0f, 0.0f),
            600,
            600,
            1.0f,
            0.9f,
            new Unity.Mathematics.int3(0, 0, 0),
            3,
            -99999.0f,
            10.0f,
            10.0f,
            2.0f,
            2.0f,
            1.0f
            );
    }

    public void SpawnSphereEnemy(float x, float z, EnemyProperty prop)
    {
        GameManager.computeCenter.AppendCreateSphereEnemyRequest(
            new Vector3(x, 0.5f, z),
            new Vector3(0.0f, 0.0f, 0.0f),
            prop.hp,
            prop.hp,
            1.0f,
            0.9f,
            new Unity.Mathematics.int3(0, 0, 0),
            prop.weapon,
            -99999.0f,
            prop.mass,
            prop.mass,
            prop.acceleration,
            prop.frictionalDeceleration,
            prop.maxSpeed
            );
    }

    public void SpawnSphereEnemy(float x, float z, int propIndex)
    {
        SpawnSphereEnemy(x, z, GameManager.allEnemyProperty.enemyPropertyData[propIndex]);
    }

    public void CreateSpawnEnemyRequest(int num)
    {
        x = -19.0f;
        z = 14.0f;
        remainingNum = num;
        dir = 0;
    }

    public void Update()
    {
        if (GameManager.gameTime - lastSpawnEnemyTime > spawnEnemyInterval
            && remainingNum > 0)
        {
            lastSpawnEnemyTime = GameManager.gameTime;
            remainingNum--;
            SpawnSphereEnemy(x, z);
            if (dir == 0)
            {
                if (x > 18.9f)
                {
                    x = 19.0f;
                    z = 12.0f;
                    dir = 1;
                }
                else x += 2.0f;
            }
            else if (dir == 1)
            {
                if (z < -13.9f)
                {
                    x = 17.0f;
                    z = -14.0f;
                    dir = 2;
                }
                else z -= 2.0f;
            }
            else if (dir == 2)
            {
                if (x < -18.9f)
                {
                    x = -19.0f;
                    z = -12.0f;
                    dir = 3;
                }
                else x -= 2.0f;
            }
            else if (dir == 3)
            {
                if (z > 13.9f)
                {
                    x = -17.0f;
                    z = 14.0f;
                    dir = 0;
                }
                else z += 2.0f;
            }
        }
    }
}
