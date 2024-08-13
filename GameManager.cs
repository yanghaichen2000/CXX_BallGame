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
    public static GameManager instance;

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

    public static Player player1;
    public static Player player2;
    public static PlayerSkillManager playerSkillManager;

    public static AllLevelPlayerData allLevelPlayerData;

    public static AllEnemyProperty allEnemyProperty;
    public static EnemyLegion enemyLegion;

    public static UIManager uiManager;

    public static GameLevel level;

    public static CameraMotionManager cameraMotionManager;

    public ComputeShader computeCenterCS;


    // const
    public static Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

    // inspector
    public Color player1BulletColor;
    public Color player2BulletColor;
    public Color enemyColorWeak;
    public Color enemyColorMedium;
    public Color enemyColorStrong;
    public Color enemyColorSuper;
    public Color enemyColorUltra;
    [Range(0.0f, 3.0f)] public float bulletDirectionalLightIntensity;

    // game
    bool gameOver = false;

    void Awake()
    {
        instance = this;

        //Screen.SetResolution(2560, 1440, true);
        Application.targetFrameRate = 240;

        deltaTimeQueue = new Queue<float>();
        timeSum = 0.0f;
        frameCount = -1;
        computeCenter = new ComputeCenter(this);
        player1 = new Player(0, GameObject.Find("Player1"), new KeyboardInputManager());
        player2 = new Player(1, GameObject.Find("Player2"), new ControllerInputManager());
        allLevelPlayerData = new AllLevelPlayerData();
        allEnemyProperty = new AllEnemyProperty();
        enemyLegion = new EnemyLegion();
        uiManager = new UIManager();
        playerSkillManager = new PlayerSkillManager();
        gameStartedTime = DateTime.Now;
        basicTransform = GameObject.Find("ball game").transform;
        cameraMotionManager = new CameraMotionManager();
        level = new GameLevel();
    }

    void Start()
    {
        lastTickTime = DateTime.Now;

        level.StartLevel();
    }

    void Update()
    {
        if (!gameOver)
        {
            UpdateTime();
            using (new GUtils.PFL("GameLevel.Update")) { level.Update(); }
            using (new GUtils.PFL("CameraMotionManager.Update")) { cameraMotionManager.Update(); }
            using (new GUtils.PFL("EnemyLegion.Update")) { enemyLegion.Update(); }
            using (new GUtils.PFL("player1.Update")) { player1.Update(); }
            using (new GUtils.PFL("player2.Update")) { player2.Update(); }
            using (new GUtils.PFL("PlayerSkillManager.Update")) { playerSkillManager.Update(); }
            using (new GUtils.PFL("ComputeCenter.UpdateGPU")) { computeCenter.UpdateGPU(); }

            if (Input.GetKeyDown(KeyCode.U))
            {
                level.nextWave = level.Wave18;
                level.currentWave = 17;
                player1.exp = 88888;
                player2.exp = 88888;
                player1.weapon = allLevelPlayerData.GetWeapon(0, 20);
                player2.weapon = allLevelPlayerData.GetWeapon(1, 20);
            }

            if (player1.obj.transform.localPosition.y < -20.0f && player2.obj.transform.localPosition.y < -20.0f)
            {
                gameOver = true;
                uiManager.text_gameOver.text = "YOU LOSE";
                uiManager.text_gameOver.color = Color.red;
            }
            else if (level.currentWave == 19 && level.currentEnemyNum == 0 && gameTime > level.currentWaveStartTime + 45.0f)
            {
                gameOver = true;
                uiManager.text_gameOver.text = "YOU WIN";
                uiManager.text_gameOver.color = Color.green;
            }
        }
        else
        {
            UpdateTime();
            using (new GUtils.PFL("ComputeCenter.UpdateGPU")) { computeCenter.UpdateGPU(); }
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void FixedUpdate()
    {
        player1.FixedUpdate();
        player2.FixedUpdate();
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
