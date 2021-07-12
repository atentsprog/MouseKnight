using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
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
        DashAttack
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

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteTr = GetComponentInChildren<SpriteRenderer>().transform;
        trailRenderer = GetComponentInChildren<SpriteTrailRenderer.SpriteTrailRenderer>();
        trailRenderer.enabled = false;

        normalSpeed = speed;
        attackInfoMap = attackInfos.ToDictionary(x => x.attackState);
    }

    void Update()
    {
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
        public float duration;
        public int damage;
        public StateType nextAttack;
    }
    [Header("공격")]
    public List<AttackInfo> attackInfos;
    Dictionary<StateType, AttackInfo> attackInfoMap;
    private IEnumerator AttackCo(AttackInfo currentAttack)
    {
        State = currentAttack.attackState;
        yield return new WaitForSeconds(currentAttack.duration); // 공격이 끝날때까지 쉬자. 
        State = StateType.Idle;
    }

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
        while (Time.time < jumpEndTime)
        {
            float y = jumpYac.Evaluate(sumEvaluateTime / jumpTimeMultiply);
            y *= jumpYMultiply;
            transform.Translate(0, y, 0); // 여기서움직여도 navMeshAgent때문에 0으로 간다. 그러므로 누적된 높이가 적용되는게 아니라 매번 땅에서 부터의 높이 설정이 된다)

            if (State == StateType.JumpUp)
            {
                if(previousyHeight > y)
                {
                    State = StateType.JumpDown;
                }
                else
                    previousyHeight = y;
            }
            
            yield return null;
            sumEvaluateTime += Time.deltaTime;
        }
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

            if (State == StateType.DashMove || distance > moveableDistance)
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

                transform.Translate(dir * speed * Time.deltaTime, Space.World);

                //방향(dir)에 따라서
                //오른쪽이라면 Y : 0, sprite X : 45
                //왼쪽이라면 Y : 180, sprite X : -45
                bool isRightSide = dir.x > 0;
                if (isRightSide)
                {
                    transform.rotation = Quaternion.Euler(Vector3.zero);
                    spriteTr.rotation = Quaternion.Euler(45, 0, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                    spriteTr.rotation = Quaternion.Euler(-45, 180, 0);
                }


                if (CanChangeWalkOrIdle())
                    State = StateType.Walk;
            }
            else
            {
                if (CanChangeWalkOrIdle())
                    State = StateType.Idle;
            }
        }
    }

    private bool CanChangeWalkOrIdle()
    {
        return jumpState != JumpStateType.Jump && State != StateType.DashMove;
    }

    private bool IsIngAttack()
    {
        return State == StateType.Attack1
            || State == StateType.Attack2
            || State == StateType.Attack3;
    }
}
