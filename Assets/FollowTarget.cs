using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    // 타겟을 따라 다니지만 높이값은 원래 값을 유지하자.
    float originalY;

    public Transform target;
    void Start()
    {
        originalY = transform.position.y;
        //transform.parent = null;
    }

    void LateUpdate()
    {
        var newPos = target.position;
        newPos.y = originalY;
        transform.position = newPos;
    }
}
