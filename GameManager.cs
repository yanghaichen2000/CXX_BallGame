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
    public static float deltaTime;

    public static BulletManager playerBulletManager;
    public static BulletManager enemyBulletManager;
    public static float bulletLifeSpan = 12.0f;

    public static Player player1;
    public static Player player2;

    public static EnemyLegion enemyLegion;

    public static float enemyAndBulletIntersectionBias = 0.05f;
    public static float enemyAndEnemyIntersectionBias = 0.5f;
    public static Vector3 bulletPoolRecyclePosition = new Vector3(-100.0f, 0.0f, 5.0f);

    public static Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

    public ComputeShader testComputeShader;

    void Awake()
    {
        playerBulletManager = new BulletManager();
        enemyBulletManager = new BulletManager();
        player1 = new Player(GameObject.Find("Player1"), new KeyboardInputManager());
        player2 = new Player(GameObject.Find("Player2"), new KeyboardInputManager());
        enemyLegion = new EnemyLegion();
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
                enemyLegion.SpawnCubeEnemy(a * 5.0f, b * 5.0f);
                enemyLegion.SpawnSphereEnemy(a * 3.0f, b * 5.0f);
                enemyLegion.SpawnCubeEnemy(a * 1.0f, b * 5.0f);
                enemyLegion.SpawnCubeEnemy(a * 2.0f, b * 3.0f);
                enemyLegion.SpawnSphereEnemy(a * 4.0f, b * 3.0f);
                enemyLegion.SpawnCubeEnemy(a * 6.0f, b * 3.0f);
                enemyLegion.SpawnCubeEnemy(a * 5.0f, b * 1.0f);
                enemyLegion.SpawnSphereEnemy(a * 3.0f, b * 1.0f);
                enemyLegion.SpawnCubeEnemy(a * 1.0f, b * 1.0f);
                enemyLegion.SpawnSphereEnemy(a * 2.0f, b * 7.0f);
                enemyLegion.SpawnCubeEnemy(a * 4.0f, b * 7.0f);
                enemyLegion.SpawnCubeEnemy(a * 6.0f, b * 7.0f);
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
        //TestComputeShader();
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
        using (new BallGameUtils.Profiler("TickAllEnemies")) { enemyLegion.TickAllEnemies(); }
        using (new BallGameUtils.Profiler("TickAllPlayerBullets")) { playerBulletManager.TickAllBullets(); }
        BallGameUtils.LogWithCD(playerBulletManager.bullets.Count);
    }

    public void UpdateTime()
    {
        currentTime = DateTime.Now;
        deltaTime = (float)(currentTime - lastTickTime).TotalSeconds;
        lastTickTime = currentTime;
    }

    public void TestComputeShader()
    {
        DateTime t1 = DateTime.Now;

        const int length = 128;
        float[] data = new float[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = i + 1;
        }

        ComputeBuffer buffer = new ComputeBuffer(data.Length, sizeof(float));
        buffer.SetData(data);

        int kernel = testComputeShader.FindKernel("AddOne");
        testComputeShader.SetBuffer(kernel, "data", buffer);
        testComputeShader.Dispatch(kernel, data.Length / 64, 1, 1);

        buffer.GetData(data);
        buffer.Release();

        DateTime t2 = DateTime.Now;

        Debug.Log(data[0]);
        Debug.Log((t1 - t2).TotalSeconds);
    }
}
