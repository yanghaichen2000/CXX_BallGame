using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet
{
    public Vector3 pos;
    public Vector3 dir;
    public float speed;
    public float radius;
    public float damage;
    public DateTime createdTime;
    public GameObject obj;

    public Bullet()
    {
        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Collider[] colliders = obj.GetComponents<Collider>();
        foreach (Collider collider in colliders) collider.enabled = false;
        obj.transform.SetParent(GameManager.basicTransform);
        obj.GetComponent<Renderer>().material = Resources.Load<Material>("bullet");
    }

    public void Initialize(DateTime _createdTime, Vector3 _pos, Vector3 _dir, float _speed, float _radius, float _damage)
    {
        createdTime = _createdTime;
        pos = _pos;
        dir = _dir;
        speed = _speed;
        radius = _radius;
        damage = _damage;

        obj.transform.localPosition = pos;
        obj.transform.localScale = new Vector3(radius * 2.0f, radius * 2.0f, radius * 2.0f);
    }

    public void SetPos(Vector3 _pos)
    {
        pos = _pos;
        obj.transform.localPosition = pos;
    }

    public void MoveToSomeplace()
    {
        pos = GameManager.bulletPoolRecyclePosition;
        obj.transform.localPosition = pos;
    }
}