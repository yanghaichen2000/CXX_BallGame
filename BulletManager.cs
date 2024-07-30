using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager
{
    public void ShootOneBullet(Vector3 _pos, Vector3 _dir, float _speed, float _radius, int _damage, int bounces = 5, float lifeSpan = 6.0f)
    {
        GameManager.computeCenter.AppendPlayerShootRequest(_pos, _dir, _speed, _radius, _damage, bounces, lifeSpan);
    }
}
