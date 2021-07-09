using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public float originalY;
    private void Awake()
    {
        target = transform.parent;
        originalY = transform.position.y;
    }
    void LateUpdate()
    {
        var newPos = target.position;
        newPos.y = originalY;

        transform.position = newPos;
    }
}
