using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDirectionUI : MonoBehaviour
{
    public static PlayerDirectionUI instance;
    Transform directionTr;
    void Awake()
    {
        instance = this;
        directionTr = transform.Find("DirectionParent");
    }

    public void SetDirection(float z)
    {
        directionTr.rotation = Quaternion.Euler(0, 0, z);
    }
}
