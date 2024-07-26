using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;


public class GameManager : MonoBehaviour
{
    public static Transform basicTransform;
    public static DateTime gameStartedTime;
    public static DateTime lastTickTime;
    public static DateTime currentTime;
    public static float gameTime;
    public static float deltaTime;

    public static BulletManager bulletManager;
    public static float bulletLifeSpan = 12.0f;

    public static Player player1;
    public static Player player2;

    public static EnemyLegion enemyLegion;

    public static GameManagerGPU gameManagerGPU;

    public static float enemyAndBulletIntersectionBias = 0.05f;
    public static float enemyAndEnemyIntersectionBias = 0.5f;
    public static Vector3 bulletPoolRecyclePosition = new Vector3(-15.0f, 10.0f, 5.0f);
    public static Vector3 enemyPoolRecyclePosition = new Vector3(15.0f, 10.0f, 5.0f);

    public static Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

    public ComputeShader moveBulletsCS;
    public ComputeShader playerBulletCS;

    void Awake()
    {
        bulletManager = new BulletManager(this);
        player1 = new Player(GameObject.Find("Player1"), new KeyboardInputManager());
        player2 = new Player(GameObject.Find("Player2"), new KeyboardInputManager());
        enemyLegion = new EnemyLegion();
        gameManagerGPU = new GameManagerGPU(this);
        gameStartedTime = DateTime.Now;
        basicTransform = GameObject.Find("ball game").transform;
    }

    void Start()
    {
        lastTickTime = DateTime.Now;

        for (int a = -1; a <= 1; a += 2)
        {
            for (int b = -1; b <= 1; b += 2)
            {
                enemyLegion.SpawnSphereEnemy(a * 5.0f, b * 5.0f);
                enemyLegion.SpawnSphereEnemy(a * 3.0f, b * 5.0f);
                enemyLegion.SpawnSphereEnemy(a * 1.0f, b * 5.0f);
                enemyLegion.SpawnSphereEnemy(a * 2.0f, b * 3.0f);
                enemyLegion.SpawnSphereEnemy(a * 4.0f, b * 3.0f);
                enemyLegion.SpawnSphereEnemy(a * 6.0f, b * 3.0f);
                enemyLegion.SpawnSphereEnemy(a * 5.0f, b * 1.0f);
                enemyLegion.SpawnSphereEnemy(a * 3.0f, b * 1.0f);
                enemyLegion.SpawnSphereEnemy(a * 1.0f, b * 1.0f);
                enemyLegion.SpawnSphereEnemy(a * 2.0f, b * 7.0f);
                enemyLegion.SpawnSphereEnemy(a * 4.0f, b * 7.0f);
                enemyLegion.SpawnSphereEnemy(a * 6.0f, b * 7.0f);
            }
        }

        enemyLegion.SpawnStaticCube(-10.0f, -15.0f);
        enemyLegion.SpawnStaticCube(0.0f, -15.0f);
        enemyLegion.SpawnStaticCube(10.0f, -15.0f);
        enemyLegion.SpawnStaticCube(-10.0f, 15.0f);
        enemyLegion.SpawnStaticCube(0.0f, 15.0f);
        enemyLegion.SpawnStaticCube(10.0f, 15.0f);

        enemyLegion.SpawnStaticCube(-20.0f, 5.0f);
        enemyLegion.SpawnStaticCube(-20.0f, -5.0f);
        enemyLegion.SpawnStaticCube(20.0f, 5.0f);
        enemyLegion.SpawnStaticCube(20.0f, -5.0f);
    }

    void Update()
    {
        Tick();
    }

    void FixedUpdate()
    {
        player1.FixedUpdate();
    }

    public void Tick()
    {
        UpdateTime();
        using (new BallGameUtils.Profiler("player1.Update")) { player1.Update(); }
        using (new BallGameUtils.Profiler("player2.Update")) { player2.Update(); }
        using (new BallGameUtils.Profiler("TickAllBulletsGPU")) { bulletManager.TickAllBulletsGPU(); }
        using (new BallGameUtils.Profiler("TickAllEnemies")) { enemyLegion.TickAllEnemies(); }
        //using (new BallGameUtils.Profiler("TickAllPlayerBullets")) { bulletManager.TickAllBullets(); }
        //BallGameUtils.LogWithCD(bulletManager.bullets.Count);
    }

    public void UpdateTime()
    {
        currentTime = DateTime.Now;
        gameTime = (float)(currentTime - gameStartedTime).TotalSeconds;
        deltaTime = (float)(currentTime - lastTickTime).TotalSeconds;
        lastTickTime = currentTime;
    }
}
