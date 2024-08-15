using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneMaterialInitializer : MonoBehaviour
{
    void Start()
    {
        Material gameMaterial = Resources.Load<Material>("ground");
        gameObject.GetComponent<MeshRenderer>().material = gameMaterial;
    }
}
