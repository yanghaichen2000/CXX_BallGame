using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;


public class GameManagerGPU
{
    const int maxDataLength = 16384;
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

        moveBulletsData = new MoveBulletsDatum[maxDataLength];
        moveBulletsCS = gameManager.moveBulletsCS;
        moveBulletsKernel = moveBulletsCS.FindKernel("MoveBullets");
        moveBulletsCB = new ComputeBuffer(maxDataLength, moveBulletsDatumSize);
        moveBulletsWriteBackReferences = new Bullet[maxDataLength];
    }

    public void MovePlayerBullets()
    {
        int bulletNum = 0;
        using (new BallGameUtils.Profiler("PrepareData"))
        {
            foreach (Bullet bullet in GameManager.playerBulletManager.bullets)
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
            
        moveBulletsCS.Dispatch(moveBulletsKernel, maxDataLength / 64, 1, 1);
        moveBulletsCB.GetData(moveBulletsData);

        using (new BallGameUtils.Profiler("WriteBackData"))
        {
            for (int i = 0; i < bulletNum; i++)
            {
                Bullet bullet = moveBulletsWriteBackReferences[i];
                bullet.SetPos(new Vector3(moveBulletsData[i].posDir.x, 0.5f, moveBulletsData[i].posDir.y));
            }
        }
    }
}
