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
        SphereEnemy enemy = sphereEnemyInstancePool.Get();
        enemy.Initialize(new Vector3(x, 0.5f, z));
        enemies.Add(enemy);
    }

    public void SpawnStaticCube(float x, float z)
    {
        StaticCube enemy = new StaticCube();
        enemy.Initialize(new Vector3(x, 0.5f, z));
        enemies.Add(enemy);
    }

    public void TickAllEnemies()
    {
        foreach (Enemy enemy in enemies)
        {
            enemy.ProcessBullets();
            if (!enemy.IsDead()) enemy.Move();
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
