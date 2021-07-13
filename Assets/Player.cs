﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Player : MonoBehaviour, IReceiveMeleeAttackInfo, ITakeHit
{
    public static Player instance;
    private void Awake()
    {
        instance = this;
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
        Attack1,
        Attack2,
        Attack3,
        DashMove,
        DashAttack,
        TakeHit,
        Death,
    }

    Animator animator;

    [SerializeField] StateType m_state = StateType.Idle;
    StateType State
    {
        get { return m_state; }
        set
        {
            if (m_state == value)
                return;

            if (EditorOption.Options[OptionType.Player상태변화로그])
                Debug.Log($"{m_state} -> {value}");

            m_state = value;
            animator.Play(m_state.ToString());
        }
    }
    public Transform mousePointer;
    public Transform spriteTr;


    [Header("이동")]
    public float speed = 5;
    [Tooltip("normalSpeed는 시작할때 speed 값으로 채워집니다")]
    public float normalSpeed;
    [FormerlySerializedAs("moveableDistance")]
    public float moveableStartDistance = 3;

    [Tooltip("moveableStartDistance 보다 작은값을 설정해야 합니다")]
    public float moveableStopDistance = 2;
    Plane plane = new Plane(new Vector3(0, 1, 0), 0);

    Image hpBar;
    private void Start()
    {
        hpBar = transform.Find("Canvas/HpBar").GetComponent<Image>();
        animator = GetComponentInChildren<Animator>();
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;
        trailRenderer = GetComponentInChildren<SpriteTrailRenderer.SpriteTrailRenderer>();
        agent = GetComponent<NavMeshAgent>();
        trailRenderer.enabled = false;

        normalSpeed = speed;
        attackInfoMap = attackInfos.ToDictionary(x => x.attackState);
    }

    void Update()
    {
        ////Test, 모든 몬스터 데미지 주기.
        //if(Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    FindObjectOfType<Goblin>().OnDamage(11);
        //}


        Move();

        Jump();

        // 마우스 드래그 하면 대시 
        bool isMouseUpByDash = Dash();

        Attack(isMouseUpByDash);
    }

    // 대시 공격 조건 확인
    [Header("대시")]
    [SerializeField] Vector3 mouseDownPoint;
    [SerializeField] float mouseDownTime;
    public float dashableTime = 0.3f;
    public float dashableDistance = 5f;

    public float dashTime = 0.3f;
    public float dashSpeedMultiply = 5f;

    Coroutine dashHandle;
    /// <summary>
    /// 
    /// </summary>
    /// <returns>대시했으면 true반환</returns>
    private bool Dash()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            bool isVaildDrag = IsValidDashDrag();

            //오른쪽으로 드래그 했다면 끝지점이 플레이어의 오른쪽에 있어야한다.
            if (isVaildDrag)
            {
                if (dashHandle != null)
                    StopCoroutine(dashHandle);
                dashHandle = StartCoroutine(DashCo());
                return true;
            }
        }

        // 드래그 했는가?
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mouseDownPoint = Input.mousePosition;
            mouseDownTime = Time.time;
        }

        return false;
    }
    
    private bool IsValidDashDrag()
    {
        float dragTime = Time.time - mouseDownTime;
        float dragDistance = Vector3.Distance(mouseDownPoint, Input.mousePosition);
        //print(dragDistance);

        if (dragTime > dashableTime)
        {
            //Debug.Log($"dragTime:{dragTime}, dashableTime:{dashableTime}");
            return false;
        }


        if (dragDistance < dashableDistance)
        {
            //Debug.Log($"dragDistance:{dragDistance}, dashableDistance:{dashableDistance}");
            return false;
        }


        //드래그한 방향이 올바른지 확인하자.
        Vector3 currentMouseWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isRightDirectionDrag = mouseDownPoint.x < Input.mousePosition.x;
        if (isRightDirectionDrag)
        {
            // 오른쪽 드래그
            // 마우스 현재 위치가 플레이어보다 왼쪽에 있다면 올바른 드래그가 아니다.
            if (transform.position.x > currentMouseWorldPoint.x)
            {
                //Debug.Log($"오른쪽 드래그 시도 실패, 플레이어보다 왼쪽에 있음 x:{transform.position.x}, currentMousePoint:{currentMouseWorldPoint.x}");
                return false;
            }
            dashDirection = new Vector3(1, 0, 0);
        }
        else // 왼쪽 드래그.
        {
            if (transform.position.x < currentMouseWorldPoint.x)
            {
                //Debug.Log($"왼쪽 드래그 시도 실패, 플레이어보다 오른쪽에 있음 x:{transform.position.x}, currentMousePoint:{currentMouseWorldPoint.x}");
                return false;
            }
            dashDirection = new Vector3(-1, 0, 0);
        }
        //Debug.Log("대시 성공");
        return true;
    }

    SpriteTrailRenderer.SpriteTrailRenderer trailRenderer;
    // 대시 중에는 방향 전환 안되게 하기(오른쪽 혹은 왼쪽으로만 이동되게 하기(대가건 대시 x)
    private IEnumerator DashCo()
    {
        //대시 하자.
        State = StateType.DashMove;
        speed = normalSpeed * dashSpeedMultiply;
        trailRenderer.enabled = true;
        yield return new WaitForSeconds(dashTime);
        trailRenderer.enabled = false;
        speed = normalSpeed;
        State = StateType.Idle;
    }

    private void Attack(bool isMouseUpByDash)
    {
        // 마우스 클릭하면 공격
        if (isMouseUpByDash == false) // 대시에 의해 마우스 클릭버튼이 들어 올려진경우는 공격으로 인식하지 않게 막자.
        {
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                ProcessAttack();
            }
        }



        // 대시 중에 공격하면 대시 공격

        // 점프 중에 공격하면 공중 공격

        // 점프중에 대시하면 대시 각도에 따라 점프앞대시공격, 점프 낙하대시공격
    }

    Coroutine attackHandle;
    private void ProcessAttack()
    {
        if (attackHandle != null)
            StopCoroutine(attackHandle);

        AttackInfo previousAttack;
        AttackInfo currentAttack;
        attackInfoMap.TryGetValue(m_state, out previousAttack);

        currentAttack = GetNextAttack(previousAttack);

        attackHandle = StartCoroutine(AttackCo(currentAttack));
    }

    private AttackInfo GetNextAttack(AttackInfo previousAttack)
    {
        AttackInfo currentAttack;

        if (previousAttack == null || attackInfoMap.TryGetValue(previousAttack.nextAttack, out currentAttack) == false)
            currentAttack = attackInfoMap[StateType.Attack1];

        return currentAttack;
    }

    [System.Serializable]
    public class AttackInfo
    {
        public StateType attackState;
        public StateType nextAttack;
        public float duration;
        public float attackTime; // 공격 시작되는 시점.
        public int damage;
        public Collider attackAreaCollider;
    }
    [Header("공격")]
    public List<AttackInfo> attackInfos;
    Dictionary<StateType, AttackInfo> attackInfoMap;
    AttackInfo currentAttack;
    private IEnumerator AttackCo(AttackInfo currentAttack)
    {
        this.currentAttack = currentAttack;
        State = currentAttack.attackState;
        float attackEndAnimationTime = Time.time + currentAttack.duration;
        yield return new WaitForSeconds(currentAttack.attackTime); //공격 시작되는 시점.

        // 방식 1) 트리거
        //// 단점 1) 콜라이더 2개중 1곳에 RigidBody가 있어야 한다.
        //// 단점 2) RigidBody가 움직여야 한다.
        //// 단점 3) 즉시 적용할 수 없다. 콜라이더를 껐다가 최소 1프레임 대기후 다시 꺼야한다. 
        //// 단점 4) 충돌체 Trriger체크 하고 Trigger를 감지할 수 있는 컴포넌트 추가해야한다.
        //currentAttack.attackAreaCollider.enabled = true;
        //yield return null;
        //currentAttack.attackAreaCollider.enabled = false;


        // 방식 2) 물리코드로 체크
        //// 단점 1) 물리로 충돌영역 확인하는 로직 추가해야한다.
        //// 장점) 트리거를 사용한 단점이 해결된다.
        ////공격 범위에 있는 몬스터를 때리자. -> 어떻게 감지할 것인가? 
        ////공격 콜라이더 위치참조해서 충돌 감지 로직 돌리자.
        SphereCollider sphereCollider = currentAttack.attackAreaCollider as SphereCollider;
        var enemyColliders = Physics.OverlapSphere(sphereCollider.transform.position, sphereCollider.radius, enemyLayer);
        foreach (var item in enemyColliders)
        {
            item.GetComponent<ITakeHit>().OnTakeHit(currentAttack.damage);
        }
        float waitTime = attackEndAnimationTime - Time.time;
        yield return new WaitForSeconds(waitTime); // 공격이 끝날때까지 쉬자. 
        State = StateType.Idle;
    }
    public LayerMask enemyLayer;
    private void Jump()
    {
        if (jumpState == JumpStateType.Jump)
            return;
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(JumpCo());
        }
    }

    [Header("점프")]
    JumpStateType jumpState;
    public AnimationCurve jumpYac;
    public float jumpYMultiply = 1;
    public float jumpTimeMultiply = 1;
    NavMeshAgent agent;
    private IEnumerator JumpCo()
    {
        jumpState = JumpStateType.Jump;
        State = StateType.JumpUp;
        float jumpStartTime = Time.time;
        float jumpDuration = jumpYac[jumpYac.length - 1].time;
        jumpDuration *= jumpTimeMultiply;
        float jumpEndTime = jumpStartTime + jumpDuration;
        float sumEvaluateTime = 0;
        float previousyHeight = 0;
        agent.enabled = false;
        
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            y *= jumpYMultiply;
            transform.Translate(0, y, 0); // 여기서움직여도 navMeshAgent때문에 0으로 간다. 그러므로 누적된 높이가 적용되는게 아니라 매번 땅에서 부터의 높이 설정이 된다)

            if (transform.position.y < 0)
            {
                var pos = transform.position;
                pos.y = 0;
                transform.position = pos;
                break;
            }

            if (State == StateType.JumpUp)
            {
                if(previousyHeight > transform.position.y)
                {
                    State = StateType.JumpDown;
                }
                else
                    previousyHeight = transform.position.y;
            }
            
            yield return null;
            sumEvaluateTime += Time.deltaTime;
        }
        agent.enabled = true;

        jumpState = JumpStateType.Ground;
        State = StateType.Idle;
    }

    Vector3 dashDirection;
    private void Move()
    {
        if (Time.timeScale == 0) // 타임 스케일이 0이라는건 디버깅을 위해서 스케일을 0으로 한경우이므로 애니메이션 업데이트 정지시킴
            return;

        if (IsIngAttack())
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            mousePointer.position = hitPoint;
            float distance = Vector3.Distance(hitPoint, transform.position);

            float moveableDistance = m_state == StateType.Walk ? moveableStopDistance : moveableStartDistance;

            Vector3 dir = SetSpriteDirection(hitPoint);

            if (State == StateType.DashMove || distance > moveableDistance)
            {
                transform.Translate(dir * speed * Time.deltaTime, Space.World);

                if (CanChangeWalkOrIdle())
                    State = StateType.Walk;
            }
            else
            {
                // 움지이지 않더라도 마우스 방향에 따라 오른쪽 왼쪽 보는건 수정하자
                if (CanChangeWalkOrIdle())
                    State = StateType.Idle;
            }
        }
    }

    private Vector3 SetSpriteDirection(Vector3 hitPoint)
    {
        Vector3 dir;
        if (State == StateType.DashMove)
        {
            dir = dashDirection;
        }
        else
        {
            dir = hitPoint - transform.position;
            dir.Normalize();
        }

        //방향(dir)에 따라서
        //오른쪽이라면 Y : 0, sprite X : 45
        //왼쪽이라면 Y : 180, sprite X : -45
        bool isRightSide = dir.x > 0;
        if (isRightSide)
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        return dir;
    }

    private bool CanChangeWalkOrIdle()
    {
        if (jumpState == JumpStateType.Jump)
            return false;
        if (State == StateType.DashMove)
            return false;
        if (State == StateType.TakeHit)
            return false;
        if (State == StateType.Death)
            return false;

        return true;
    }

    private bool IsIngAttack()
    {
        return State == StateType.Attack1
            || State == StateType.Attack2
            || State == StateType.Attack3;
    }

    public float hp = 100;
    public float MaxHp = 100;


    public void OnTriggerEnterFromChildCollider(Collider other)
    {
        other.GetComponent<ITakeHit>().OnTakeHit(currentAttack.damage);
    }

    public void OnTakeHit(float damage)
    {
        hp -= damage;
        hpBar.fillAmount = hp / MaxHp;
        if (hp <= 0)
        {
            State = StateType.Death;
            //게임 종료 확인창 표시하자.
        }
        else
        {
            StartCoroutine(TakeHitCo());
        }
    }

    public float takeHitTime = 0.5f;
    private IEnumerator TakeHitCo()
    {
        State = StateType.TakeHit;
        yield return new WaitForSeconds(takeHitTime);
        State = StateType.Idle;
    }
}
