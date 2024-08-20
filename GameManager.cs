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

    public static ComputeManager computeManager;

    public static Player player1;
    public static Player player2;
    public static PlayerSkillManager playerSkillManager;

    public static AllLevelPlayerData allLevelPlayerData;

    public static AllEnemyProperty allEnemyProperty;
    public static EnemyLegion enemyLegion;

    public static UIManager uiManager;

    public static GameLevel level;
    public static Boss boss;

    public static CameraMotionManager cameraMotionManager;

    public ComputeShader computeManagerCS;

    public SceneRenderingManager sceneRenderingManager;

    // const
    public static Plane gamePlane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));
    public static Color player1Color;


    // inspector
    public Color player1BulletColor;
    public Color player2BulletColor;
    public Color bossBulletColor;
    public Color enemyColorWeak;
    public Color enemyColorMedium;
    public Color enemyColorStrong;
    public Color enemyColorSuper;
    public Color enemyColorUltra;
    public Color enemyColorLittleBoss;
    [Range(0.0f, 3.0f)] public float bulletDirectionalLightIntensity;
    [Range(0.0f, 3.0f)] public float bulletEmissionIntensity;
    [Range(0.0f, 3.0f)] public float planeLightingGaussianBlurCoeff;
    [Range(0.0f, 3.0f)] public float planeLightingTextureIntensity;
    [Range(0.0f, 3.0f)] public float bulletLightingOnEnemyIntensity;
    [Range(0.0f, 10.0f)] public float playerSkillEmission;

    // game
    bool gamePaused = false;
    int state = 0;

    void Awake()
    {
        instance = this;
        deltaTimeQueue = new Queue<float>();
        timeSum = 0.0f;
        frameCount = -1;
        computeManager = new ComputeManager(this);
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
        boss = new Boss();
        sceneRenderingManager = new SceneRenderingManager();
        state = 0;

        player1Color = player1.obj.GetComponent<Renderer>().material.color;
    }

    void Start()
    {
        lastTickTime = DateTime.Now;
        boss.Remove();
        player1.Remove();
        player2.Remove();
        uiManager.UpdateUIState(state);
    }

    void Update()
    {
        if (state == 0)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                state = 1;
                level.StartLevel1();
                uiManager.UpdateUIState(state);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                state = 1;
                level.StartLevel17();
                uiManager.UpdateUIState(state);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                state = 1;
                level.StartLevel20();
                uiManager.UpdateUIState(state);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
        if (state == 1)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                gamePaused = !gamePaused;

                if (gamePaused)
                {
                    player1.body.isKinematic = true;
                    player2.body.isKinematic = true;
                    boss.body.isKinematic = true;
                }
                else
                {
                    player1.body.isKinematic = false;
                    player2.body.isKinematic = false;
                    boss.body.isKinematic = false;
                }
            }

            if (!gamePaused)
            {
                UpdateTime();
                using (new GUtils.PFL("GameLevel.Update")) { level.Update(); }
                using (new GUtils.PFL("CameraMotionManager.Update")) { cameraMotionManager.Update(); }
                using (new GUtils.PFL("EnemyLegion.Update")) { enemyLegion.Update(); }
                using (new GUtils.PFL("player1.Update")) { player1.Update(); }
                using (new GUtils.PFL("player2.Update")) { player2.Update(); }
                using (new GUtils.PFL("PlayerSkillManager.Update")) { playerSkillManager.Update(); }
                using (new GUtils.PFL("Boss.Update")) { boss.Update(); }
                using (new GUtils.PFL("ComputeCenter.UpdateGPU")) { computeManager.UpdateGPU(); }
                using (new GUtils.PFL("SceneRenderingManager.Update")) { sceneRenderingManager.Update(); }

                if (Input.GetKeyDown(KeyCode.U))
                {
                    level.nextWave = level.Wave17;
                    level.currentWave = 16;
                    player1.exp = 88888;
                    player2.exp = 88888;
                    player1.weapon = allLevelPlayerData.GetWeapon(0, 20);
                    player2.weapon = allLevelPlayerData.GetWeapon(1, 20);
                }

                if (Input.GetKeyDown(KeyCode.I))
                {
                    level.nextWave = level.Wave20;
                    level.currentWave = 19;
                    player1.exp = 88888;
                    player2.exp = 88888;
                    player1.weapon = allLevelPlayerData.GetWeapon(0, 20);
                    player2.weapon = allLevelPlayerData.GetWeapon(1, 20);
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    state = 0;
                    ResetContext();
                    uiManager.UpdateUIState(state);
                }

                if (player1.obj.transform.localPosition.y < -20.0f && player2.obj.transform.localPosition.y < -20.0f)
                {
                    state = 2;
                    uiManager.UpdateUIState(state, true);
                    boss.body.isKinematic = true;
                }
                else if (level.currentWave == 20 && level.currentEnemyNum == 0 && boss.obj.transform.localPosition.y < -5.0f)
                {
                    state = 2;
                    uiManager.UpdateUIState(state, false);
                    boss.body.isKinematic = true;
                    player1.body.isKinematic = true;
                    player2.body.isKinematic = true;
                }
            }
            if (gamePaused)
            {
                computeManager.UpdateComputeGlobalConstant();
                computeManager.UpdateGlobalBufferForRendering();
                computeManager.DrawEnemyBullet();
                computeManager.DrawPlayerBullet();
                computeManager.DrawEnemy();
            }
        }
        else if (state == 2)
        {
            UpdateTime();
            using (new GUtils.PFL("ComputeCenter.UpdateGPU")) { computeManager.UpdateGPU(); }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                state = 0;
                ResetContext();
                uiManager.UpdateUIState(state);
            }
        }

    }

    void FixedUpdate()
    {
        player1.FixedUpdate();
        player2.FixedUpdate();
        boss.FixedUpdate();
    }

    void OnDestroy()
    {
        computeManager.Release();
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
        uiManager.UpdateFPSAndOtherDebugData(averageFPS);
    }

    public void ResetContext()
    {
        player1.obj.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        player2.obj.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", Color.black);
        player1.obj.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", player1Color);
        cameraMotionManager.ResetCameraState();
        computeManager.Release();
        uiManager.obj_gameOver.SetActive(true);
        uiManager.obj_mainMenu.SetActive(true);
        uiManager.obj_gameUI.SetActive(true);
        uiManager.image_aimingPoint.SetActive(true);
        Awake();
        boss.Remove();
        player1.Remove();
        player2.Remove();
    }
}
