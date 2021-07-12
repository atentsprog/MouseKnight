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
    IEnumerator Start()
    {
        target = Player.instance.transform;
        animator = GetComponentInChildren<Animator>();
        originalSpeed = speed;
        animator.Play("Idle");

        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        // 플레이어가 근접할때까지 대기
        while (Vector3.Distance(target.position, transform.position) > watchRange)
            yield return null;

        // 플레이어 쫒아가서 때리자.
        fsm = ChaseTargetFSM;
        while (hp > 0)
        {
            fsmChange = false;
            fsmHandle = StartCoroutine(fsm());
            yield return fsmHandle;
        }
    }
    Coroutine fsmHandle;
    Func<IEnumerator> fsm;
    Func<IEnumerator> Fsm
    {
        set
        {
            if(fsmHandle!= null)
                StopCoroutine(fsmHandle);

            fsm = value;
            fsmChange = true;
        }
    }
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
