using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRenderingManager
{
    Material lavaMaterial;

    public SceneRenderingManager()
    {
        lavaMaterial = Resources.Load<Material>("background");
    }


    public void Update()
    {
        lavaMaterial.mainTextureOffset += new Vector2(0.01f, 0.0f) * GameManager.deltaTime;
    }
}
