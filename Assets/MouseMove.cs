using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseMove : MonoBehaviour
{
    public Transform mousePointer;
    public float moveRange = 5;
    public float normalMoveSpeed = 5;
    public float applyMoveSpeed;

    public AnimationCurve jumpHeightCurve;
    public AnimationCurve jumpHorizontalMoveSpeedCurve;

    Plane groundPlane;
    public void Awake()
    {
        applyMoveSpeed = normalMoveSpeed;
        groundPlane = new Plane(new Vector3(0f, 1f, 0f), 0);
    }



    void Update()
    {
        //마우스 가 캐릭터보다 일정범위 밖에 있다면 해당 방향으로 이동하자.

        float mouseDistance = 0;
        Vector3 move = Vector3.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            var pos = ray.GetPoint(rayDistance);

            mousePointer.position = pos;
            mouseDistance = Vector3.Distance(transform.position, pos);
            float absMouseDistance = Mathf.Abs(mouseDistance);
            if (absMouseDistance > moveRange)
            {
                var dir = pos - transform.position;
                dir.Normalize();

                transform.Translate(dir * applyMoveSpeed * Time.deltaTime);
            }
        }

        // 마우스 클릭하면 공격

        // 마우스 우클릭하면 점프
        ProcessJump();

        // 점프 중에 공격하면 공중 공격

        // 점프중에 대시하면 대시 각도에 따라 점프앞대시공격, 점프 낙하대시공격

        // 마우스 드래그 하면 대시 

        // 대시 중에 공격하면 대시 공격
    }

    public enum JumpStateType
    {
        Ground,
        Jump,
    }
    JumpStateType jumpState;
    private void ProcessJump()
    {
        if (jumpState == JumpStateType.Jump) // 공중 점프 안되게 막기.
            return;

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(JumpCo());
        }
    }

    public float jumpYDuration; // 몇초 동안 점프 할 것인가? -> 애니메이션 커브 시간을 그대로 사용하자.
    public float jumpXDuration;
    public float jumpTime = 1;
    public float jumpMoveYSpeedMultiply = 1f;
    public float jumpMoveXSpeedMultiply = 1;


    private IEnumerator JumpCo()
    {
        jumpState = JumpStateType.Jump;
        jumpXDuration = jumpHorizontalMoveSpeedCurve[jumpHeightCurve.length - 1].time;
        jumpYDuration = jumpHeightCurve[jumpHeightCurve.length - 1].time * jumpTime;
        float jumpStartTime = Time.time;
        float jumpEndTime = jumpStartTime + jumpYDuration;
        float sumEvaluateTime = 0;
        while (Time.time < jumpEndTime)
        {
            float evaluateTime = sumEvaluateTime / jumpTime;
            float y = jumpHeightCurve.Evaluate(evaluateTime) * jumpMoveYSpeedMultiply;
            transform.Translate(0, y, 0);

            applyMoveSpeed = normalMoveSpeed * jumpHorizontalMoveSpeedCurve.Evaluate(evaluateTime * jumpXDuration) * jumpMoveXSpeedMultiply;
            yield return null;
            sumEvaluateTime += Time.deltaTime;
        }
        jumpState = JumpStateType.Ground;
        applyMoveSpeed = normalMoveSpeed;
    }
}
