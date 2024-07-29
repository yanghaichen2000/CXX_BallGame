using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;


public class GameManagerGPU
{
    const int maxBulletNum = 16384;
    GameManager gameManager;

    public struct MoveBulletsDatum
    {
        public Vector4 posDir;
        public float speed;
    }
    int moveBulletsDatumSize = sizeof(float) * 5;
    ComputeShader moveBulletsCS;
    int moveBulletsKernel;
    MoveBulletsDatum[] moveBulletsData;
    ComputeBuffer moveBulletsCB;
    Bullet[] moveBulletsWriteBackReferences;

    public GameManagerGPU(GameManager _gameManager)
    {
        gameManager = _gameManager;

        moveBulletsData = new MoveBulletsDatum[maxBulletNum];
        moveBulletsCS = gameManager.moveBulletsCS;
        moveBulletsKernel = moveBulletsCS.FindKernel("MoveBullets");
        moveBulletsCB = new ComputeBuffer(maxBulletNum, moveBulletsDatumSize);
        moveBulletsWriteBackReferences = new Bullet[maxBulletNum];
    }

    public void MovePlayerBullets()
    {
        if (maxBulletNum < GameManager.bulletManager.bullets.Count) Debug.Assert(false);

        int bulletNum = 0;
        using (new GameUtils.Profiler("PrepareData"))
        {
            foreach (Bullet bullet in GameManager.bulletManager.bullets)
            {
                moveBulletsData[bulletNum] = new MoveBulletsDatum()
                {
                    posDir = new Vector4(bullet.pos.x, bullet.pos.z, bullet.dir.x, bullet.dir.z),
                    speed = bullet.speed
                };
                moveBulletsWriteBackReferences[bulletNum] = bullet;
                bulletNum++;
            }

            moveBulletsCB.SetData(moveBulletsData);
            moveBulletsCS.SetFloat("_DeltaTime", GameManager.deltaTime);
            moveBulletsCS.SetBuffer(moveBulletsKernel, "moveBulletsData", moveBulletsCB);
        }
            
        moveBulletsCS.Dispatch(moveBulletsKernel, maxBulletNum / 64, 1, 1);
        moveBulletsCB.GetData(moveBulletsData);

        using (new GameUtils.Profiler("WriteBackData"))
        {
            int i = 0;
            foreach (Bullet bullet in GameManager.bulletManager.bullets)
            {
                bullet.SetPos(new Vector3(moveBulletsData[i].posDir.x, 0.5f, moveBulletsData[i].posDir.y));
                i++;
                if (i == bulletNum) break;
            }
        }
    }
}
