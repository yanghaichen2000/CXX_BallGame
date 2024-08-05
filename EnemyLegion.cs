using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLegion
{
    public void SpawnSphereEnemy(float x, float z)
    {
        GameManager.computeCenter.AppendCreateSphereEnemyRequest(
            new Vector3(x, 0.5f, z),
            new Vector3(0.0f, 0.0f, 0.0f),
            1000,
            1.0f,
            0.9f,
            new Unity.Mathematics.int3(0, 0, 0),
            3,
            -99999.0f,
            10.0f,
            10.0f
            );
    }

    public void SpawnSphereEnemy()
    {

    }
}
