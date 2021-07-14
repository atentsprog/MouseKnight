using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        var inits = GetComponentsInChildren<IInit>();
        foreach (var item in inits)
            item.InitInstance();
    }
}
