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
    public static int frameCount;

    public static ComputeCenter computeCenter;
    public static float bulletLifeSpan = 12.0f;

    public static BulletManager bulletManager;

    public static Player player1;
    public static Player player2;

    public static EnemyLegion enemyLegion;

    public static UIManager uiManager;

    public static float enemyAndBulletIntersectionBias = 0.05f;
    public static float enemyAndEnemyIntersectionBias = 0.5f;
    public static Vector3 bulletPoolRecyclePosition = new Vector3(-15.0f, 10.0f, 5.0f);
    public static Vector3 enemyPoolRecyclePosition = new Vector3(15.0f, 10.0f, 5.0f);

    public static Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

    public ComputeShader computeCenterCS;

    void Awake()
    {
        frameCount = 0;
        computeCenter = new ComputeCenter(this);
        bulletManager = new BulletManager();
        player1 = new Player(0, GameObject.Find("Player1"), new KeyboardInputManager());
        player2 = new Player(1, GameObject.Find("Player2"), new KeyboardInputManager());
        enemyLegion = new EnemyLegion();
        uiManager = new UIManager();
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

        /*
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
        */
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
        using (new GameUtils.Profiler("player1.Update")) { player1.Update(); }
        using (new GameUtils.Profiler("player2.Update")) { player2.Update(); }
        using (new GameUtils.Profiler("TickGPU")) { computeCenter.TickGPU(); }
    }

    public void UpdateTime()
    {
        frameCount++;
        currentTime = DateTime.Now;
        gameTime = (float)(currentTime - gameStartedTime).TotalSeconds;
        deltaTime = Mathf.Min((float)(currentTime - lastTickTime).TotalSeconds, 0.03f);
        lastTickTime = currentTime;
    }
}
