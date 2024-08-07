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
    public static PlayerSkillManager playerSkillManager;

    public static EnemyLegion enemyLegion;

    public static UIManager uiManager;

    public ComputeShader computeCenterCS;

    public static float enemyAndBulletIntersectionBias = 0.05f;
    public static float enemyAndEnemyIntersectionBias = 0.5f;
    public static Vector3 bulletPoolRecyclePosition = new Vector3(-15.0f, 10.0f, 5.0f);
    public static Vector3 enemyPoolRecyclePosition = new Vector3(15.0f, 10.0f, 5.0f);

    public static Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

    public Color player1BulletColor;
    public Color player2BulletColor;
    public Color enemyBulletColor;
    [Range(0.0f, 3.0f)] public float bulletDirectionalLightIntensity;

    void Awake()
    {
        //Screen.SetResolution(2560, 1440, true);
        Application.targetFrameRate = 240;

        deltaTimeQueue = new Queue<float>();
        timeSum = 0.0f;
        frameCount = -1;
        computeCenter = new ComputeCenter(this);
        player1 = new Player(0, GameObject.Find("Player1"), new KeyboardInputManager());
        player2 = new Player(1, GameObject.Find("Player2"), new ControllerInputManager());
        playerSkillManager = new PlayerSkillManager();
        enemyLegion = new EnemyLegion();
        uiManager = new UIManager();
        gameStartedTime = DateTime.Now;
        basicTransform = GameObject.Find("ball game").transform;
    }

    void Start()
    {
        lastTickTime = DateTime.Now;

        enemyLegion.CreateSpawnEnemyRequest(800);
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

        using (new GUtils.PFL("EnemyLegion.Tick")) { enemyLegion.Tick(); }
        using (new GUtils.PFL("player1.Update")) { player1.Update(); }
        using (new GUtils.PFL("player2.Update")) { player2.Update(); }
        using (new GUtils.PFL("playerSkillManager.Tick")) { playerSkillManager.Tick(); }
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
