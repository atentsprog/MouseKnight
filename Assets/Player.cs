using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] StateType state = StateType.Idle;
    public float speed = 5;
    float normalSpeed;
    public float walkDistance = 12;
    public float stopDistance = 7;
    public Transform mousePointer;
    public Transform spriteTr;
    Plane plane = new Plane( new Vector3( 0, 1, 0), 0);

    StateType State
    {
        get { return state; }
        set
        {
            if (state == value)
                return;

            if (EditorOption.Options[OptionType.Player상태변화로그])
                Debug.Log($"state:{state} => value:{value}");

            state = value;
            animator.Play(state.ToString());
        }
    }

    private void Start()
    {
        normalSpeed = speed;
        animator = GetComponentInChildren<Animator>();
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;
    }

    void Update()
    {
        Move();

        Jump();

        Dash();
    }


    [Foldout("대시")] public float dashableDistance = 10;
    [Foldout("대시")] public float dashableTime = 0.4f;
    float mouseDownTime;
    Vector3 mouseDownPosition;
    private void Dash()
    {
        // 마우스 드래그를 
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mouseDownTime = Time.time;
            mouseDownPosition = Input.mousePosition; // 
        }


        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            bool isDashDrag = IsSucceesDashDrag();
            if(isDashDrag)
            {
                StartCoroutine(DashCo());
            }
        }
    }

    [Foldout("대시")] public float dashTime = 0.3f;
    [Foldout("대시")] public float dashSpeedMultiplySpeed = 4f;
    Vector3 dashDirection;
    private IEnumerator DashCo()
    {
        //방향을 바꿀 수 없게끔, -> 진행방향으로 이동 -> 대각선 이동 대각선이동 -> 드래그방향으로 이동할건지
        //    플레이이어이동방향 x이동할 껀지
        //// dashDirection x방향만 사용.
        dashDirection = Input.mousePosition - mouseDownPosition;
        dashDirection.y = 0;
        dashDirection.z = 0;
        dashDirection.Normalize();
        speed = normalSpeed * dashSpeedMultiplySpeed;
        State = StateType.Dash;
        yield return new WaitForSeconds(dashTime);
        speed = normalSpeed;
        State = StateType.Idle;
    }

    private bool IsSucceesDashDrag()
    {
        // 시간 체크.
        float dragTime = Time.time - mouseDownTime;
        if (dragTime > dashableTime)
            return false;

        // 거리체크.
        float dragDistance = Vector3.Distance(mouseDownPosition, Input.mousePosition);
        if (dragDistance < dashableDistance)
            return false;

        return true;
    }

    [BoxGroup("점프")] public AnimationCurve jumpYac;
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
        JumpUp,
        JumpDown,
        Dash,
        Attack,
    }

    Animator animator;
    JumpStateType jumpState;
    [BoxGroup("점프")] public float jumpYMultiply = 1;
    [BoxGroup("점프")] public float jumpTimeMultiply = 1;
    private IEnumerator JumpCo()
    {
        jumpState = JumpStateType.Jump;
        State = StateType.JumpUp;
        float jumpStartTime = Time.time;
        float jumpDuration = jumpYac[jumpYac.length - 1].time;
        jumpDuration *= jumpTimeMultiply;
        float jumpEndTime = jumpStartTime + jumpDuration;
        float sumEvaluateTime = 0;
        float previousY = 0;
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            y *= jumpYMultiply;
            transform.Translate(0, y, 0);
            yield return null;

            if (previousY > y)
            {
                //떨어지는 모션으로 바꾸자.
                State = StateType.JumpDown;
            }
            previousY = y;

            sumEvaluateTime += Time.deltaTime;
        }
        jumpState = JumpStateType.Ground;
        State = StateType.Idle;
    }

    private void Move()
    {
        if (Time.timeScale == 0)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            mousePointer.position = hitPoint;
            float distance = Vector3.Distance(hitPoint, transform.position);

            float movealbeDistance = stopDistance;
            // State가 Walk 일땐 7(stopDistance)사용.
            // Idle에서 Walk로 갈땐 12(WalkDistance)사용
            if (State == StateType.Idle)
                movealbeDistance = walkDistance;

            if (distance > movealbeDistance)
            {
                var dir = hitPoint - transform.position;
                dir.Normalize();

                if (State == StateType.Dash)
                    dir = dashDirection;

                transform.Translate(dir * speed * Time.deltaTime, Space.World);

                //방향(dir)에 따라서
                //오른쪽이라면 Y : 0
                //왼쪽이라면 Y : 180
                bool isRightSide = dir.x > 0;
                if (isRightSide)
                {
                    transform.rotation = Quaternion.Euler(Vector3.zero);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }

                if (ChangeableState())
                    State = StateType.Walk;
            }
            else
            {
                if (ChangeableState())
                    State = StateType.Idle;
            }

            bool ChangeableState()
            {
                if (jumpState == JumpStateType.Jump)
                    return false;

                if (state == StateType.Dash)
                    return false;

                return true;
            }
        }
    }
}
