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

    public static InstancePool<Bullet> bulletPool = new InstancePool<Bullet>();
    public static HashSet<Bullet> bullets = new HashSet<Bullet>();
    public static Stack<Bullet> bulletRecycleBin = new Stack<Bullet>();
    public static float bulletLifeSpan = 12.0f;

    public static GameObject playerObj;
    public static Player player;

    public static EnemyLegion enemyLegion = new EnemyLegion();

    public static float enemyAndBulletIntersectionBias = 0.05f;
    public static float enemyAndEnemyIntersectionBias = 0.5f;
    public static Vector3 bulletPoolRecyclePosition = new Vector3(-100.0f, 0.0f, 5.0f);

    [SerializeField]
    public ComputeShader testComputeShader;

    void Awake()
    {
        playerObj = GameObject.Find("Player");
        player = new Player();
        gameStartedTime = DateTime.Now;
        basicTransform = GameObject.Find("ball game").transform;
    }

    void Start()
    {
        lastTickTime = DateTime.Now;
        player.Initialize();

        enemyLegion.SpawnCubeEnemy(5.0f, 5.0f);
        enemyLegion.SpawnSphereEnemy(3.0f, 5.0f);
        enemyLegion.SpawnCubeEnemy(1.0f, 5.0f);
        enemyLegion.SpawnCubeEnemy(2.0f, 3.0f);
        enemyLegion.SpawnSphereEnemy(4.0f, 3.0f);
        enemyLegion.SpawnCubeEnemy(6.0f, 3.0f);
        enemyLegion.SpawnCubeEnemy(5.0f, 1.0f);
        enemyLegion.SpawnSphereEnemy(3.0f, 1.0f);
        enemyLegion.SpawnCubeEnemy(1.0f, 1.0f);
        enemyLegion.SpawnSphereEnemy(2.0f, 7.0f);
        enemyLegion.SpawnCubeEnemy(4.0f, 7.0f);
        enemyLegion.SpawnCubeEnemy(6.0f, 7.0f);
        enemyLegion.SpawnCubeEnemy(-5.0f, 5.0f);
        enemyLegion.SpawnSphereEnemy(-3.0f, 5.0f);
        enemyLegion.SpawnCubeEnemy(-1.0f, 5.0f);
        enemyLegion.SpawnCubeEnemy(-2.0f, 3.0f);
        enemyLegion.SpawnSphereEnemy(-4.0f, 3.0f);
        enemyLegion.SpawnCubeEnemy(-6.0f, 3.0f);
        enemyLegion.SpawnCubeEnemy(-5.0f, 1.0f);
        enemyLegion.SpawnSphereEnemy(-3.0f, 1.0f);
        enemyLegion.SpawnCubeEnemy(-1.0f, 1.0f);
        enemyLegion.SpawnSphereEnemy(-2.0f, 7.0f);
        enemyLegion.SpawnCubeEnemy(-4.0f, 7.0f);
        enemyLegion.SpawnCubeEnemy(-6.0f, 7.0f);

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
        player.FixedUpdate();
    }

    public void Tick()
    {
        UpdateTime();
        using (new BallGameUtils.Profiler("Player.Update")) { player.Update(); }
        using (new BallGameUtils.Profiler("TickAllEnemies")) { enemyLegion.TickAllEnemies(); }
        using (new BallGameUtils.Profiler("RecycleDyingBullets")) { RecycleDyingBullets(); }
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
