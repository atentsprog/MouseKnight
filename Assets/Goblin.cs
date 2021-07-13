using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Goblin : MonoBehaviour
{
    public float watchRange = 25;
    public float attackRange = 10;
    public float speed = 40;
    public int hp = 100;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, watchRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    Animator animator;
    Transform target;
    IEnumerator fsmHandle;
    IEnumerator Start()
    {
        target = Player.instance.transform;
        animator = GetComponentInChildren<Animator>();
        originalSpeed = speed;
        animator.Play("Idle");

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        infoTextMesh = GetComponentInChildren<TextMesh>();

        // 플레이어가 근접할때까지 대기
        fsm = IdleCo;

        while (hp > 0)
        {
            fsmChange = false;
            fsmHandle = fsm();
            StartCoroutine(fsmHandle);

            while (fsmHandle != null)
            {
                yield return null;
            }
            //Debug.LogWarning("코루틴 1개 끝남");
        }
        //Debug.LogWarning("코루틴 완전 끝남");
    }

    IEnumerator IdleCo()
    {
        while (Vector3.Distance(target.position, transform.position) > watchRange)
            yield return null;

        Fsm = ChaseTargetFSM;
    }

    Func<IEnumerator> fsm;
    Func<IEnumerator> Fsm
    {
        set
        {
            if (fsmHandle != null)
            {
                StopCoroutine(fsmHandle);
                fsmHandle = null;
            }

            fsm = value;
            fsmChange = true;

            infoTextMesh.text = fsm.Method.ToString(); ;
        }
    }
    TextMesh infoTextMesh;
    bool fsmChange = false;


    IEnumerator ChaseTargetFSM()
    {
        animator.Play("Run");
        while (fsmChange == false)
        {
            Vector3 toPlayerDirection = transform.position - target.position;
            toPlayerDirection.y = 0;
            toPlayerDirection.Normalize();

            transform.Translate(toPlayerDirection * speed * Time.deltaTime);

            //플레이어가 오른쪽에 있을때 0, 왼쪽일땐 180
            if (toPlayerDirection.x > 0)
                animator.transform.rotation = Quaternion.Euler(0, 180, 0);
            else
                animator.transform.rotation = Quaternion.Euler(Vector3.zero);
            yield return null;
            if (Vector3.Distance(transform.position, target.position) < attackRange)
            {
                Fsm = AttackFsm;
            }
        }
    }
    public float attackTime = 1;
    IEnumerator AttackFsm()
    {
        animator.Play("Attack");
        yield return new WaitForSeconds(attackTime);
        Fsm = ChaseTargetFSM;
    }

    public void OnDamage(int damage)
    {
        hp -= damage;   
        Fsm = OnAttacked;
    }
    public float attackedTime = 0.7f;
    private float originalSpeed;

    IEnumerator OnAttacked()
    {
        animator.Play("TakeHit");
        speed = 0;
        yield return new WaitForSeconds(attackedTime);

        speed = originalSpeed;
        Fsm = ChaseTargetFSM;
    }
}
