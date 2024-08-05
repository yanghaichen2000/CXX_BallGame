using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
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

    public Queue<float> deltaTimeQueue;
    float timeSum;
    float averageFPS;

    public static ComputeCenter computeCenter;
    public static float bulletLifeSpan = 12.0f;

    public static Player player1;
    public static Player player2;

    public static EnemyLegion enemyLegion;

    public static UIManager uiManager;

    public ComputeShader computeCenterCS;

    public static float enemyAndBulletIntersectionBias = 0.05f;
    public static float enemyAndEnemyIntersectionBias = 0.5f;
    public static Vector3 bulletPoolRecyclePosition = new Vector3(-15.0f, 10.0f, 5.0f);
    public static Vector3 enemyPoolRecyclePosition = new Vector3(15.0f, 10.0f, 5.0f);

    public static Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

    [SerializeField]
    public Color player1BulletColor;
    [SerializeField]
    public Color player2BulletColor;

    void Awake()
    {
        Screen.SetResolution(3840, 2160, true);
        Application.targetFrameRate = 240;

        deltaTimeQueue = new Queue<float>();
        timeSum = 0.0f;
        frameCount = 0;
        computeCenter = new ComputeCenter(this);
        player1 = new Player(0, GameObject.Find("Player1"), new KeyboardInputManager());
        player2 = new Player(1, GameObject.Find("Player2"), new ControllerInputManager());
        enemyLegion = new EnemyLegion();
        uiManager = new UIManager();
        gameStartedTime = DateTime.Now;
        basicTransform = GameObject.Find("ball game").transform;
    }

    void Start()
    {
        lastTickTime = DateTime.Now;

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
        player2.FixedUpdate();
    }

    public void Tick()
    {
        UpdateTime();

        bool spawn = frameCount % 10 == 0;
        int spawnIndex = frameCount / 10;
        float x = -18.0f + (spawnIndex % 30) * 1.2f;
        float z = 0.0f + (spawnIndex / 30) * 1.2f;
        if (spawn && spawnIndex < 128) enemyLegion.SpawnSphereEnemy(x, z);

        using (new GUtils.PFL("player1.Update")) { player1.Update(); }
        using (new GUtils.PFL("player2.Update")) { player2.Update(); }
        using (new GUtils.PFL("TickGPU")) { computeCenter.TickGPU(); }
    }

    public void UpdateTime()
    {
        frameCount++;
        currentTime = DateTime.Now;
        gameTime = (float)(currentTime - gameStartedTime).TotalSeconds;
        deltaTime = Mathf.Min((float)(currentTime - lastTickTime).TotalSeconds, 0.03f);
        lastTickTime = currentTime;

        if (deltaTimeQueue.Count >= 20) timeSum -= deltaTimeQueue.Dequeue();
        deltaTimeQueue.Enqueue(deltaTime);
        timeSum += deltaTime;
        averageFPS = deltaTimeQueue.Count > 0 ? (1.0f / (timeSum / deltaTimeQueue.Count)) : 1;
        uiManager.UpdateFPS(averageFPS);
    }
}
