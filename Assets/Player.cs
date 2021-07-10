using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        attackInfoMap = attackInfos.ToDictionary(x => x.attackState);
    }

    void Update()
    {
        Move();

        Jump();

        Attack();
    }

    private void Attack()
    {
        // 마우스 클릭하면 공격
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            ProcessAttack();
        }

        // 마우스 우클릭하면 점프

        // 점프 중에 공격하면 공중 공격

        // 점프중에 대시하면 대시 각도에 따라 점프앞대시공격, 점프 낙하대시공격

        // 마우스 드래그 하면 대시 

        // 대시 중에 공격하면 대시 공격
    }

    Coroutine attackHandle;
    private void ProcessAttack()
    {
        if (attackHandle != null)
            StopCoroutine(attackHandle);

        AttackInfo previousAttack;
        AttackInfo currentAttack;
        attackInfoMap.TryGetValue(state, out previousAttack);

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
    public List<AttackInfo> attackInfos;
    Dictionary<StateType, AttackInfo> attackInfoMap;
    private IEnumerator AttackCo(AttackInfo currentAttack)
    {
        State = currentAttack.attackState;
        yield return new WaitForSeconds(currentAttack.duration); // 공격이 끝날때까지 쉬자. 
        State = StateType.Idle;
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
        Attack1,
        Attack2,
        Attack3,
    }

    [SerializeField] StateType state = StateType.Idle;
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

    private bool IsIngAttack()
    {
        return State == StateType.Attack1
            || State == StateType.Attack2
            || State == StateType.Attack3;
    }
}
