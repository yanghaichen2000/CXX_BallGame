using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public struct EnemyProperty
{
    public int hp;
    public int weapon;
    public float mass;
    public float acceleration;
    public float frictionalDeceleration;
    public float maxSpeed;
    public uint color;
}

public class AllEnemyProperty
{
    public EnemyProperty[] enemyPropertyData;

    public AllEnemyProperty()
    {
        enemyPropertyData = new EnemyProperty[20];

        enemyPropertyData[0] = new EnemyProperty()
        {
            hp = 50,
            weapon = 0,
            mass = 1.0f,
            acceleration = 0.7f,
            frictionalDeceleration = 0.5f,
            maxSpeed = 0.5f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorWeak),
        };

        enemyPropertyData[1] = new EnemyProperty()
        {
            hp = 50,
            weapon = 1,
            mass = 1.0f,
            acceleration = 0.7f,
            frictionalDeceleration = 0.5f,
            maxSpeed = 0.5f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorWeak),
        };

        enemyPropertyData[2] = new EnemyProperty()
        {
            hp = 150,
            weapon = 1,
            mass = 2.5f,
            acceleration = 1.0f,
            frictionalDeceleration = 1.0f,
            maxSpeed = 0.8f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorMedium),
        };

        enemyPropertyData[3] = new EnemyProperty()
        {
            hp = 150,
            weapon = 2,
            mass = 2.5f,
            acceleration = 1.0f,
            frictionalDeceleration = 1.0f,
            maxSpeed = 0.8f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorMedium),
        };

        enemyPropertyData[4] = new EnemyProperty()
        {
            hp = 300,
            weapon = 3,
            mass = 3.0f,
            acceleration = 1.0f,
            frictionalDeceleration = 1.5f,
            maxSpeed = 1.0f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorStrong),
        };

        enemyPropertyData[5] = new EnemyProperty()
        {
            hp = 300,
            weapon = 4,
            mass = 3.0f,
            acceleration = 1.0f,
            frictionalDeceleration = 1.5f,
            maxSpeed = 1.0f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorStrong),
        };

        enemyPropertyData[6] = new EnemyProperty()
        {
            hp = 600,
            weapon = 4,
            mass = 10.0f,
            acceleration = 1.0f,
            frictionalDeceleration = 1.5f,
            maxSpeed = 1.2f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorSuper),
        };

        enemyPropertyData[7] = new EnemyProperty()
        {
            hp = 600,
            weapon = 5,
            mass = 10.0f,
            acceleration = 1.0f,
            frictionalDeceleration = 1.5f,
            maxSpeed = 1.2f,
            color = GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorSuper),
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
    public EnemyProperty currentprop;

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
            1.0f,
            GUtils.SRGBColorToLinearUInt(GameManager.instance.enemyColorWeak),
            0.0f
            );
    }

    public void SpawnSphereEnemy(float x, float z, EnemyProperty prop, float extraDelay = 0.0f)
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
            prop.maxSpeed,
            prop.color,
            extraDelay
            );
    }

    public void SpawnSphereEnemy(float x, float z, int propIndex, float extraDelay = 0.0f)
    {
        SpawnSphereEnemy(x, z, GameManager.allEnemyProperty.enemyPropertyData[propIndex], extraDelay);
    }

    public void SpawnSphereEnemy(float xStart, float zStart, int xLength, int zLength, int propIndex, float stepSizeX = 1.6f, float stepSizeZ = 1.6f, float extraDelay = 0.0f)
    {
        Debug.Assert(xLength * zLength <= ComputeCenter.maxDeployingEnemyNum);
        for (int i = 0; i < xLength; i++)
        {
            for (int j = 0; j < zLength; j++)
            {
                float x = xStart + i * stepSizeX;
                float z = zStart + j * stepSizeZ;
                SpawnSphereEnemy(x, z, GameManager.allEnemyProperty.enemyPropertyData[propIndex], extraDelay);
            }
        }
        
    }

    public void CreateSpawnEnemyRequest(int num, EnemyProperty prop)
    {
        x = -19.0f;
        z = 14.0f;
        remainingNum = num;
        dir = 0;
        currentprop = prop;
        lastSpawnEnemyTime = GameManager.gameTime;
    }

    public void Update()
    {
        if (GameManager.gameTime - lastSpawnEnemyTime > spawnEnemyInterval
            && remainingNum > 0)
        {
            lastSpawnEnemyTime = GameManager.gameTime;
            remainingNum--;
            SpawnSphereEnemy(x, z, currentprop);
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
