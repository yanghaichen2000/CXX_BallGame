using System;
using UnityEngine;


public class CameraMotionManager
{
    Vector3 originalPosition;
    Quaternion originalRotation;
    Vector3 originalForward;

    Vector3 angleSpringCoeff;
    Vector3 angleAttenuationCoeff;

    Vector3 currentAngle;
    Vector3 currentAngleSpeed;

    public CameraMotionManager()
    {
        originalPosition = Camera.main.transform.position;
        originalRotation = Camera.main.transform.rotation;
        originalForward = Camera.main.transform.forward;

        // (Ðý×ª, ×óÓÒ, ÉÏÏÂ)
        angleSpringCoeff = new Vector3(80.0f, 80.0f, 80.0f);
        angleAttenuationCoeff = new Vector3(0.015f, 0.015f, 0.015f);
        currentAngle = new Vector3(0.0f, 0.0f, 0.0f); 
        currentAngleSpeed = new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void Update()
    {
        currentAngleSpeed += -GUtils.Vector3Mul(currentAngle, angleSpringCoeff) * GameManager.deltaTime;
        currentAngle += currentAngleSpeed * GameManager.deltaTime;
        currentAngle = GUtils.Vector3Mul(currentAngle, GUtils.Vector3Pow(angleAttenuationCoeff, GameManager.deltaTime));

        Quaternion rotation1 = Quaternion.AngleAxis(currentAngle.x, originalForward);
        Quaternion rotation2 = Quaternion.Euler(currentAngle.y, currentAngle.z, 0.0f);
        Camera.main.transform.rotation = rotation1 * rotation2 * originalRotation;
    }

    public void ShakeByRotation(float force = 1.0f)
    {
        Vector2 dir = new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f));
        dir.Normalize();
        float d = UnityEngine.Random.Range(-1.0f, 1.0f) > 0.0f ? 1.0f : -1.0f;
        currentAngleSpeed = new Vector3(d * 10.0f, dir.x, dir.y) * force;
        currentAngle = new Vector3(d * 2.0f, dir.x, dir.y) * force;
    }
}
