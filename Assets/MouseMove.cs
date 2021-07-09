using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMove : MonoBehaviour
{
    public Transform mousePointer;
    public float moveRange = 5;
    public LayerMask layer;
    public float speed = 5;

    void Update()
    {
        //마우스 가 캐릭터보다 일정범위 밖에 있다면 해당 방향으로 이동하자.
        float mouseDistance = 0;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        Vector3 move = Vector3.zero;
        if (Physics.Raycast(ray, out hitData, 1000, layer))
        {
            var pos = hitData.point;
            mousePointer.position = pos;
            mouseDistance = Vector3.Distance(transform.position, pos);
            float absMouseDistance = Mathf.Abs(mouseDistance);
            if (absMouseDistance > moveRange)
            {
                var dir = pos - transform.position;
                dir.Normalize();
                transform.Translate(dir * speed * Time.deltaTime);
            }
        }


        // 마우스 클릭하면 공격

        // 마우스 우클릭하면 점프

        // 점프 중에 공격하면 공중 공격

        // 점프중에 대시하면 대시 각도에 따라 점프앞대시공격, 점프 낙하대시공격

        // 마우스 드래그 하면 대시 

        // 대시 중에 공격하면 대시 공격
    }
}
