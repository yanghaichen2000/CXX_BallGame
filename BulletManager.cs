using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.PlayerSettings;

public class BulletManager
{
    public InstancePool<Bullet> bulletPool = new InstancePool<Bullet>();
    public HashSet<Bullet> bullets = new HashSet<Bullet>();
    public Stack<Bullet> bulletRecycleBin = new Stack<Bullet>();

    public void Initialize()
    {
    }

    public void TickAllBullets()
    {
        using (new BallGameUtils.Profiler("CheckBulletDeath")) { CheckBulletDeath(); }
        using (new BallGameUtils.Profiler("MoveBulletsGPU")) { GameManager.gameManagerGPU.MovePlayerBullets(); }
        //using (new BallGameUtils.Profiler("MoveBullets")) { MoveBullets(); }
        using (new BallGameUtils.Profiler("RecycleDeadBullets")) { RecycleDeadBullets(); }
    }

    public void CheckBulletDeath()
    {
        foreach (Bullet bullet in bullets)
        {
            if ((GameManager.currentTime - bullet.createdTime).TotalSeconds > GameManager.bulletLifeSpan)
            {
                bulletRecycleBin.Push(bullet);
            }
        }
    }

    public void MoveBullets()
    {
        foreach (Bullet bullet in bullets)
        {
            bullet.pos += bullet.speed * bullet.dir * GameManager.deltaTime;
            bullet.obj.transform.localPosition = bullet.pos;
        }
    }

    public void RecycleDeadBullets()
    {
        while (bulletRecycleBin.TryPop(out var bullet))
        {
            bullet.MoveToSomeplace();
            bullets.Remove(bullet);
            bulletPool.Return(bullet);
        }
    }

    public void ShootOneBullet(Vector3 pos, Vector3 dir, float _speed, float _radius, float _damage)
    {
        var bullet = bulletPool.Get();
        bullet.Initialize(GameManager.currentTime, pos, dir, _speed, _radius, _damage);
        bullets.Add(bullet);
    }
}
