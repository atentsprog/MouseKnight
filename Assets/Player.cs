using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5;
    public float moveableDistance = 3;
    public Transform mousePointer;
    public Transform spriteTr;
    Plane plane = new Plane( new Vector3( 0, 1, 0), 0);

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;
    }

    void Update()
    {
        Move();

        Jump();
    }

    public AnimationCurve jumpYac;
    private void Jump()
    {
        if (jumpState == JumpStateType.Jump)
            return;
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(JumpCo());
        }
    }
    public enum JumpStateType
    {
        Ground,
        Jump,
    }
    public enum StateType
    {
        Idle,
        Walk,
        Jump,
        Attack,
    }

    StateType state = StateType.Idle;
    StateType State
    {
        get { return state; }
        set
        {
            if (state == value)
                return;
            state = value;
            animator.Play(state.ToString());
        }
    }
    Animator animator;
    JumpStateType jumpState;
    public float jumpYMultiply = 1;
    public float jumpTimeMultiply = 1;
    private IEnumerator JumpCo()
    {
        jumpState = JumpStateType.Jump;
        State = StateType.Jump;
        float jumpStartTime = Time.time;
        float jumpDuration = jumpYac[jumpYac.length - 1].time;
        jumpDuration *= jumpTimeMultiply;
        float jumpEndTime = jumpStartTime + jumpDuration;
        float sumEvaluateTime = 0;
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            y *= jumpYMultiply;
            transform.Translate(0, y, 0);
            yield return null;
            sumEvaluateTime += Time.deltaTime;
        }
        jumpState = JumpStateType.Ground;
        State = StateType.Idle;
    }

    private void Move()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            mousePointer.position = hitPoint;
            float distance = Vector3.Distance(hitPoint, transform.position);
            if (distance > moveableDistance)
            {
                var dir = hitPoint - transform.position;
                dir.Normalize();
                transform.Translate(dir * speed * Time.deltaTime, Space.World);

                //방향(dir)에 따라서
                //오른쪽이라면 Y : 0, sprite X : 45
                //왼쪽이라면 Y : 180, sprite X : -45
                bool isRightSide = dir.x > 0;
                if(isRightSide)
                {
                    transform.rotation = Quaternion.Euler(Vector3.zero);
                    spriteTr.rotation = Quaternion.Euler(45, 0, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    spriteTr.rotation = Quaternion.Euler(-45, 180, 0);
                }

                State = StateType.Walk;
            }
            else
            {
                State = StateType.Idle;
            }
        }
    }
}
