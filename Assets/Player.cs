using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public static Player instance;
    private void Awake()
    {
        instance = this;
        m_state = StateType.NotInit;
    }

    public float speed = 5;
    float normalSpeed;
    public float walkDistance = 12;
    public float stopDistance = 7;
    //public Transform mousePointer;
    public Transform spriteTr;
    Plane plane = new Plane( new Vector3( 0, 1, 0), 0);

    [SerializeField] StateType m_state;
    StateType State
    {
        get { return m_state; }
        set
        {
            if (m_state == value)
                return;

            if (EditorOption.Options[OptionType.Player상태변화로그])
                Debug.Log($"state:{m_state} => value:{value}");

            m_state = value;
            animator.Play(m_state.ToString());
        }
    }
    NavMeshAgent agent;
    private void Start()
    {
        normalSpeed = speed;
        animator = GetComponentInChildren<Animator>();
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;
        agent = GetComponent<NavMeshAgent>();
        spriteTrailRenderer = GetComponentInChildren<SpriteTrailRenderer.SpriteTrailRenderer>();
        spriteTrailRenderer.enabled = false;
    }

    void Update()
    {
        if (StageManager.instance.gameState != GameStateType.Playing)
            return;

        if (CanMoveState())
        {
            Move();
            Jump();
        }

        bool isSucceedDash = Dash();

        Attack(isSucceedDash);
    }

    private bool CanMoveState()
    {
        if (State == StateType.Attack)
            return false;

        if (State == StateType.TakeHit)
            return false;

        if (State == StateType.Death)
            return false;

        return true;
    }

    private void Attack(bool isSucceedDash)
    {
        if (isSucceedDash)
            return;

        // 마우스 왼쪽 버튼 땠을때 공격 시키자.
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            StartCoroutine(AttackCo());
        }
    }

    public float attackTime = 1;
    public float attackApplyTime = 0.2f;
    public LayerMask enemyLayer;
    public SphereCollider attackCollider;
    public float power = 10;
    private IEnumerator AttackCo()
    {
        State = StateType.Attack;
        yield return new WaitForSeconds(attackApplyTime);
        //실제 어택하는 부분.

        Collider[] enemyColliders = Physics.OverlapSphere(
            attackCollider.transform.position
            , attackCollider.radius, enemyLayer);
        foreach (var item in enemyColliders)
        {
            item.GetComponent<Monster>().TakeHit(power);
        }

        yield return new WaitForSeconds(attackTime);
        State = StateType.Idle;
    }

    [Foldout("대시")] public float dashableDistance = 10;
    [Foldout("대시")] public float dashableTime = 0.4f;
    float mouseDownTime;
    Vector3 mouseDownPosition;
    private bool Dash()
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
                return true;
            }
        }

        return false;
    }

    [Foldout("대시")] public float dashTime = 0.3f;

    public float hp = 100;
    internal void TakeHit(int damge)
    {
        if (State == StateType.Death)
            return;

        hp -= damge;
        StartCoroutine(TakeHitCo());
    }

    public float takeHitTime = 0.3f;
    private IEnumerator TakeHitCo()
    {
        State = StateType.TakeHit; //피격 모션하자.
        yield return new WaitForSeconds(takeHitTime);

        if (hp > 0)
            State = StateType.Idle;
        else
            StartCoroutine(DeathCo());//hp < 0 으면 죽자.
    }

    public float deathTime = 0.5f;
    private IEnumerator DeathCo()
    {
        State = StateType.Death; //피격 모션하자.
        yield return new WaitForSeconds(deathTime);
        Debug.LogWarning("게임 종료");
    }
    SpriteTrailRenderer.SpriteTrailRenderer spriteTrailRenderer;
    [Foldout("대시")] public float dashSpeedMultiplySpeed = 4f;
    Vector3 dashDirection;
    private IEnumerator DashCo()
    {
        //방향을 바꿀 수 없게끔, -> 진행방향으로 이동 -> 대각선 이동 대각선이동 -> 드래그방향으로 이동할건지
        //    플레이이어이동방향 x이동할 껀지
        //// dashDirection x방향만 사용.
        spriteTrailRenderer.enabled = true;
        dashDirection = Input.mousePosition - mouseDownPosition;
        dashDirection.y = 0;
        dashDirection.z = 0;
        dashDirection.Normalize();
        speed = normalSpeed * dashSpeedMultiplySpeed;
        State = StateType.Dash;
        yield return new WaitForSeconds(dashTime);
        speed = normalSpeed;
        State = StateType.Idle;
        spriteTrailRenderer.enabled = false;
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
        NotInit,
        Idle,
        Walk,
        JumpUp,
        JumpDown,
        Dash,
        Attack,
        TakeHit,
        Death
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
        float previousY = float.MinValue;
        agent.enabled = false;  
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            y *= jumpYMultiply * Time.deltaTime; 
            transform.Translate(0, y, 0);
            yield return null;
             
            if (previousY > transform.position.y)
            {
                //떨어지는 모션으로 바꾸자.
                State = StateType.JumpDown;
            }

            if (transform.position.y < 0)
            {
                break;
            }

            previousY = transform.position.y;
            sumEvaluateTime += Time.deltaTime;
        }
        agent.enabled = true;
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
            //mousePointer.position = hitPoint;
            float distance = Vector3.Distance(hitPoint, transform.position);

            float movealbeDistance = stopDistance;
            // State가 Walk 일땐 7(stopDistance)사용.
            // Idle에서 Walk로 갈땐 12(WalkDistance)사용
            if (State == StateType.Idle)
                movealbeDistance = walkDistance;

            Vector3 dir = hitPoint - transform.position;

            if (State == StateType.Dash)
                dir = dashDirection;

            dir.Normalize();

            if (distance > movealbeDistance || State == StateType.Dash)
            {
                transform.Translate(dir * speed * Time.deltaTime, Space.World);

                if (ChangeableState())
                    State = StateType.Walk;
            }
            else
            {
                if (ChangeableState())
                    State = StateType.Idle;
            }

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

            bool ChangeableState()
            {
                if (jumpState == JumpStateType.Jump)
                    return false;

                if (m_state == StateType.Dash)
                    return false;

                return true;
            }
        }
    }
}
