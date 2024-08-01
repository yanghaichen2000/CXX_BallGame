using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLegion
{
    public HashSet<Enemy> enemies = new HashSet<Enemy>();
    public Stack<Enemy> enemyRecycleBin = new Stack<Enemy>();
    public InstancePool<SphereEnemy> sphereEnemyInstancePool = new InstancePool<SphereEnemy>();
    public InstancePool<CubeEnemy> cubeEnemyInstancePool = new InstancePool<CubeEnemy>();
    public Vector3 instancePoolDefaultPos = new Vector3(15.0f, 0.0f, 10.0f);

    public void SpawnCubeEnemy(float x, float z)
    {
        CubeEnemy enemy = cubeEnemyInstancePool.Get();
        enemy.Initialize(new Vector3(x, 0.5f, z));
        enemies.Add(enemy);
    }

    public void SpawnSphereEnemy(float x, float z)
    {
        GameManager.computeCenter.AppendCreateSphereEnemyRequest(
            new Vector3(x, 0.5f, z),
            new Vector3(0.0f, 0.0f, 0.0f),
            1000,
            1.0f,
            0.9f,
            new Unity.Mathematics.int3(0, 0, 0),
            1,
            -99999.0f,
            10.0f,
            10.0f
            );
    }

    public void SpawnStaticCube(float x, float z)
    {
        StaticCube enemy = new StaticCube();
        enemy.Initialize(new Vector3(x, 0.5f, z));
        enemies.Add(enemy);
    }

    public void TickAllEnemies()
    {
        using (new GameUtils.Profiler("enemy.ProcessBullets"))
        {
            foreach (Enemy enemy in enemies)
            {
                //enemy.ProcessBullets();
            }
        }

        using (new GameUtils.Profiler("enemy.Move"))
        {
            foreach (Enemy enemy in enemies)
            {
                if (!enemy.IsDead()) enemy.Move();
            }
        }

        foreach (Enemy enemy in enemies)
        {
            if (enemy.IsDead()) enemyRecycleBin.Push(enemy);
        }
        
        RecycleAllDeadEnemies();
    }

    public void RecycleAllDeadEnemies()
    {
        while (enemyRecycleBin.TryPop(out var enemy))
        {
            enemies.Remove(enemy);
            enemy.SetPos(instancePoolDefaultPos);
            if (enemy.GetType().Name == "SphereEnemy")
            {
                sphereEnemyInstancePool.Return((SphereEnemy)enemy);
            }
            else if (enemy.GetType().Name == "CubeEnemy")
            {
                cubeEnemyInstancePool.Return((CubeEnemy)enemy);
            }
            else
            {
                Debug.Assert(false);
            }
        }
    }
}
