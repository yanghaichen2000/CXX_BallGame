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

    Vector3 displacementSpringCoeff;
    Vector3 displacementAttenuationCoeff;

    Vector3 currentDisplacement;
    Vector3 currentDisplacementSpeed;

    public CameraMotionManager()
    {
        originalPosition = Camera.main.transform.position;
        originalRotation = Camera.main.transform.rotation;
        originalForward = Camera.main.transform.forward;

        // (顺时针, 左右, 上下)
        angleSpringCoeff = new Vector3(80.0f, 80.0f, 80.0f);
        angleAttenuationCoeff = new Vector3(0.015f, 0.015f, 0.015f);
        currentAngle = new Vector3(0.0f, 0.0f, 0.0f); 
        currentAngleSpeed = new Vector3(0.0f, 0.0f, 0.0f);

        // (右, 上, 前)
        displacementSpringCoeff = new Vector3(20.0f, 20.0f, 20.0f);
        displacementAttenuationCoeff = new Vector3(0.005f, 0.005f, 0.005f);
        currentDisplacement = new Vector3(0.0f, 0.0f, 0.0f);
        currentDisplacementSpeed = new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void Update()
    {
        // rotation
        currentAngleSpeed += -GUtils.Vector3Mul(currentAngle, angleSpringCoeff) * GameManager.deltaTime;
        currentAngle += currentAngleSpeed * GameManager.deltaTime;
        currentAngle = GUtils.Vector3Mul(currentAngle, GUtils.Vector3Pow(angleAttenuationCoeff, GameManager.deltaTime));
        Quaternion rotation1 = Quaternion.AngleAxis(currentAngle.x, originalForward);
        Quaternion rotation2 = Quaternion.Euler(currentAngle.y, currentAngle.z, 0.0f);
        Camera.main.transform.rotation = rotation1 * rotation2 * originalRotation;

        // position
        currentDisplacementSpeed += -GUtils.Vector3Mul(currentDisplacement, displacementSpringCoeff) * GameManager.deltaTime;
        currentDisplacement += currentDisplacementSpeed * GameManager.deltaTime;
        currentDisplacement = GUtils.Vector3Mul(currentDisplacement, GUtils.Vector3Pow(displacementAttenuationCoeff, GameManager.deltaTime));
        Camera.main.transform.position = originalPosition +
            Camera.main.transform.right * currentDisplacement.x +
            Camera.main.transform.up * currentDisplacement.y +
            Camera.main.transform.forward * currentDisplacement.z;
    }

    public void ShakeByRotation(float force = 1.0f)
    {
        Vector2 dir = new Vector2(UnityEngine.Random.Range(-1.0f, 1.0f), UnityEngine.Random.Range(-1.0f, 1.0f));
        dir.Normalize();
        float d = UnityEngine.Random.Range(-1.0f, 1.0f) > 0.0f ? 1.0f : -1.0f;
        currentAngleSpeed = new Vector3(d * 10.0f, dir.x, dir.y) * force;
        currentAngle = new Vector3(d * 2.0f, dir.x, dir.y) * force;
    }

    public void ShakeByZDisplacement(float force = 1.0f)
    {
        currentDisplacementSpeed = new Vector3(0.0f, 0.0f, 3.0f) * force;
        currentDisplacement = new Vector3(0.0f, 0.0f, 0.2f) * force;
    }
}
