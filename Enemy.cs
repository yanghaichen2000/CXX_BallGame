using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering;


public interface Enemy
{
    public Vector3 GetPos();
    public float GetRadius();
    public void Move();
    public void Shoot();
    public bool IsDead();
    public void SetPos(Vector3 _pos);
    public String GetEnemyType();
}


public class SphereEnemy : Enemy
{
    public Vector3 pos;
    public float radius;
    public float speed;
    public Int32 hp;
    public GameObject obj;

    public SphereEnemy()
    {
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Collider[] colliders = obj.GetComponents<Collider>();
        //foreach (Collider collider in colliders) collider.enabled = false;
        obj.transform.SetParent(GameManager.basicTransform);
        obj.GetComponent<Renderer>().material = Resources.Load<Material>("enemy");
    }

    public void Initialize(Vector3 _pos)
    {
        SetPos(_pos);
        radius = 0.5f;
        speed = 0.8f;
        hp = 1000000;
        obj.transform.localScale = new Vector3(radius * 2.0f, radius * 2.0f, radius * 2.0f);
    }


    public void Move()
    {
        Vector3 playerPos = GameManager.player1.obj.transform.localPosition;
        Vector3 playerRelativePos = playerPos - pos;
        float playerDistance = playerRelativePos.magnitude;
        float moveDistance = Mathf.Min(playerDistance, speed * GameManager.deltaTime);
        Vector3 newPos = pos + playerRelativePos.normalized * moveDistance;

        foreach (Enemy enemy in GameManager.enemyLegion.enemies)
        {
            Vector3 desiredRelativePos = newPos - pos;
            Vector3 enemyRelativePos = enemy.GetPos() - pos;
            float distance = enemyRelativePos.magnitude;
            float newDistance = (enemy.GetPos() - newPos).magnitude;
            if (newDistance < enemy.GetRadius() + radius + GameManager.enemyAndEnemyIntersectionBias
                && newDistance < distance)
            {
                newPos -= Vector3.Dot(desiredRelativePos, enemyRelativePos.normalized) * enemyRelativePos.normalized;
            }
        }

        newPos = Vector3.Scale(newPos, new Vector3(1.0f, 0.0f, 1.0f));
        newPos.y = 0.5f;
        SetPos(newPos);
    }

    public void Shoot()
    {

    }

    public bool IsDead()
    {
        return hp <= 0;
    }

    public void SetPos(Vector3 _pos)
    {
        pos = _pos;
        obj.transform.localPosition = pos;
    }

    public Vector3 GetPos() { return pos; }

    public float GetRadius() { return radius; }

    public String GetEnemyType() { return "Sphere"; }
}


public class CubeEnemy : Enemy
{
    public Vector3 pos;
    public Vector3 dir;
    public float rotationY;
    public float maxRotationalSpeed;
    public float size; // length
    public float radius;
    public float speed;
    public Int32 hp;
    public GameObject obj;

    public CubeEnemy()
    {
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.transform.SetParent(GameManager.basicTransform);
        obj.GetComponent<Renderer>().material = Resources.Load<Material>("enemy");
    }

    public void Initialize(Vector3 _pos)
    {
        SetPos(_pos);
        dir = (GameManager.player1.obj.transform.localPosition - pos).normalized;
        rotationY = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;
        maxRotationalSpeed = 40;
        size = 0.8f;
        radius = size * 0.5f * 1.414f;
        speed = 0.8f;
        hp = 1000000;
        obj.transform.localScale = new Vector3(size, size, size);
    }

    /*
    public void ProcessBullets()
    {
        Vector3[] normals = new Vector3[4];
        normals[0] = dir.normalized;
        normals[1] = -normals[0];
        normals[2] = new Vector3(-normals[0].z, 0.0f, normals[0].x);
        normals[3] = -normals[2];

        float[] projectedDistances = new float[4];

        foreach (Bullet bullet in GameManager.bulletManager.bullets)
        {
            Vector3 bulletRelativePos = bullet.pos - pos;

            for (int i = 0; i < 4; i++)  
            {
                projectedDistances[i] = Vector3.Dot(bulletRelativePos, normals[i]);
            }
            float maxProjectedDistance = -9999999.9f;
            int maxProjectedDistanceIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                if (projectedDistances[i] > maxProjectedDistance)
                {
                    maxProjectedDistance = projectedDistances[i];
                    maxProjectedDistanceIndex = i;
                }
            }

            Vector3 normal = normals[maxProjectedDistanceIndex];
            if (maxProjectedDistance < size * 0.5 + bullet.radius - GameManager.enemyAndBulletIntersectionBias
                && Vector3.Dot(bullet.dir, normal) < 0.0f)
            {
                hp -= bullet.damage;

                bullet.pos += Mathf.Min(0.0f, size * 0.5f - maxProjectedDistance + bullet.radius) * normal;
                bullet.dir -= 2.0f * Vector3.Dot(normal, bullet.dir) * normal;
                bullet.dir.y = 0.0f;
                bullet.dir = bullet.dir.normalized;
            }
        }
    }
    */

    public void Move()
    {
        Vector3 playerPos = GameManager.player1.obj.transform.localPosition;
        Vector3 playerRelativePos = playerPos - pos;
        float playerDistance = playerRelativePos.magnitude;
        float moveDistance = Mathf.Min(playerDistance, speed * GameManager.deltaTime);
        Vector3 newPos = pos + playerRelativePos.normalized * moveDistance;

        foreach (Enemy enemy in GameManager.enemyLegion.enemies)
        {
            Vector3 desiredRelativePos = newPos - pos;
            Vector3 enemyRelativePos = enemy.GetPos() - pos;
            float distance = enemyRelativePos.magnitude;
            float newDistance = (enemy.GetPos() - newPos).magnitude;
            if (newDistance < enemy.GetRadius() + radius + GameManager.enemyAndEnemyIntersectionBias
                && newDistance < distance)
            {
                newPos -= Vector3.Dot(desiredRelativePos, enemyRelativePos.normalized) * enemyRelativePos.normalized;
            }
        }

        Vector3 desiredDir = (newPos - pos).normalized;
        newPos = Vector3.Scale(newPos, new Vector3(1.0f, 0.0f, 1.0f));
        newPos.y = 0.5f;
        SetPos(newPos);

        if (desiredDir.magnitude > 0.0f)
        {
            float desiredRotationY = Quaternion.LookRotation(desiredDir, Vector3.up).eulerAngles.y;
            float maxRotationAngle = maxRotationalSpeed * GameManager.deltaTime;
            rotationY = Mathf.Clamp(desiredRotationY, rotationY - maxRotationAngle, rotationY + maxRotationAngle);
            obj.transform.eulerAngles = new Vector3(0.0f, rotationY, 0.0f);
            dir = obj.transform.forward;
        }
    }

    public void Shoot()
    {

    }

    public bool IsDead()
    {
        return hp <= 0;
    }

    public void SetPos(Vector3 _pos)
    {
        pos = _pos;
        obj.transform.localPosition = pos;
    }

    public Vector3 GetPos() { return pos; }

    public float GetRadius() { return radius; }

    public String GetEnemyType() { return "Cube"; }
}

public class StaticCube : Enemy
{
    public Vector3 pos;
    public Vector3 dir;
    public float size; // length
    public float radius;
    public float speed;
    public GameObject obj;

    public StaticCube()
    {
        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Collider[] colliders = obj.GetComponents<Collider>();
        foreach (Collider collider in colliders) collider.enabled = false;
        obj.transform.SetParent(GameManager.basicTransform);
        obj.GetComponent<Renderer>().material = Resources.Load<Material>("ground");
    }

    public void Initialize(Vector3 _pos)
    {
        SetPos(_pos);
        dir = new Vector3(1.0f, 0.0f, 0.0f);
        size = 10.0f;
        radius = 0.0f;
        obj.transform.localScale = new Vector3(size, 1.0f, size);
    }

    /*
    public void ProcessBullets()
    {
        Vector3[] normals = new Vector3[4];
        normals[0] = dir.normalized;
        normals[1] = -normals[0];
        normals[2] = new Vector3(-normals[0].z, 0.0f, normals[0].x);
        normals[3] = -normals[2];

        float[] projectedDistances = new float[4];

        foreach (Bullet bullet in GameManager.bulletManager.bullets)
        {
            Vector3 bulletRelativePos = bullet.pos - pos;

            for (int i = 0; i < 4; i++)
            {
                projectedDistances[i] = Vector3.Dot(bulletRelativePos, normals[i]);
            }
            float maxProjectedDistance = -9999999.9f;
            int maxProjectedDistanceIndex = 0;
            for (int i = 0; i < 4; i++)
            {
                if (projectedDistances[i] > maxProjectedDistance)
                {
                    maxProjectedDistance = projectedDistances[i];
                    maxProjectedDistanceIndex = i;
                }
            }

            Vector3 normal = normals[maxProjectedDistanceIndex];
            if (maxProjectedDistance < size * 0.5 + bullet.radius - GameManager.enemyAndBulletIntersectionBias
                && Vector3.Dot(bullet.dir, normal) < 0.0f)
            {
                bullet.pos += Mathf.Min(0.0f, size * 0.5f - maxProjectedDistance + bullet.radius) * normal;
                bullet.dir -= 2.0f * Vector3.Dot(normal, bullet.dir) * normal;
                bullet.dir.y = 0.0f;
                bullet.dir = bullet.dir.normalized;
            }
        }
    }
    */

    public void Move()
    {
        
    }

    public void Shoot()
    {

    }

    public bool IsDead()
    {
        return false;
    }

    public void SetPos(Vector3 _pos)
    {
        pos = _pos;
        obj.transform.localPosition = pos;
    }

    public Vector3 GetPos() { return pos; }

    public float GetRadius() { return radius; }

    public String GetEnemyType() { return "StaticCube"; }
}

