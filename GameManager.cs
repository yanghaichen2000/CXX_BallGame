using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static Transform basicTransform;
    public static DateTime gameStartedTime;
    public static DateTime lastTickTime;
    public static DateTime currentTime;
    public static float deltaTime;

    public static InstancePool<Bullet> bulletPool = new InstancePool<Bullet>();
    public static HashSet<Bullet> bullets = new HashSet<Bullet>();
    public static Stack<Bullet> bulletRecycleBin = new Stack<Bullet>();
    public static float bulletLifeSpan = 5.0f;

    public static GameObject player;
    public static PlayerManager playerManager;

    public static EnemyLegion enemyLegion = new EnemyLegion();

    public static float enemyAndBulletIntersectionBias = 0.05f;
    public static float enemyAndEnemyIntersectionBias = 0.5f;
    public static Vector3 bulletPoolRecyclePosition = new Vector3(-100.0f, 0.0f, 5.0f);

    void Awake()
    {
        player = GameObject.Find("Player");
        playerManager = player.GetComponent<PlayerManager>();
        gameStartedTime = DateTime.Now;
        basicTransform = GameObject.Find("ball game").transform;
    }

    void Start()
    {
        lastTickTime = DateTime.Now;
        enemyLegion.SpawnCubeEnemy(5.0f, 5.0f);
        enemyLegion.SpawnCubeEnemy(3.0f, 5.0f);
        enemyLegion.SpawnCubeEnemy(1.0f, 5.0f);
        enemyLegion.SpawnCubeEnemy(2.0f, 3.0f);
        enemyLegion.SpawnCubeEnemy(4.0f, 3.0f);
        enemyLegion.SpawnCubeEnemy(6.0f, 3.0f);
        enemyLegion.SpawnCubeEnemy(5.0f, 1.0f);
        enemyLegion.SpawnCubeEnemy(3.0f, 1.0f);
        enemyLegion.SpawnCubeEnemy(1.0f, 1.0f);
        enemyLegion.SpawnCubeEnemy(2.0f, 7.0f);
        enemyLegion.SpawnCubeEnemy(4.0f, 7.0f);
        enemyLegion.SpawnCubeEnemy(6.0f, 7.0f);
    }

    void Update()
    {
        Tick();
    }

    public void Tick()
    {
        UpdateTime();
        enemyLegion.TickAllEnemies();
        RecycleDyingBullets();
    }

    public void UpdateTime()
    {
        currentTime = DateTime.Now;
        deltaTime = (float)(currentTime - lastTickTime).TotalSeconds;
        lastTickTime = currentTime;
    }

    public void RecycleDyingBullets()
    {
        foreach (Bullet bullet in bullets)
        {
            if ((currentTime - bullet.createdTime).TotalSeconds > bulletLifeSpan)
            {
                bulletRecycleBin.Push(bullet);
                continue;
            }
            bullet.pos += bullet.speed * bullet.dir * deltaTime;
            bullet.obj.transform.localPosition = bullet.pos;
        }

        while (bulletRecycleBin.TryPop(out var bullet))
        {
            bullet.MoveToSomeplace();
            bullets.Remove(bullet);
            bulletPool.Return(bullet);
        }
    }
}
