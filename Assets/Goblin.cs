using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goblin : MonoBehaviour
{
    // 추격 할대 플레이어한테 공격 가능한 거리면 공격.
    // 공격후 추격
    // 추격 공격
    Animator animator;
    IEnumerator Start()
    {
        animator = GetComponentInChildren<Animator>();
        player = Player.instance;

        currentFsm = IdleFSM;
        while (true) // 상태를 무한히 반복해서 실행하는 부분.
        {
            yield return StartCoroutine(currentFsm());
        }
    }
    Func<IEnumerator> currentFsm;
    Player player;
    public float detectRange = 40;
    public float attackRange = 10;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private IEnumerator IdleFSM()
    {
        // 시작하면 Idle <- Idle 애니메이션 재생.
        animator.Play("Idle");

        ////IdleCo
        // 플레이어 근접하면 추격
        while (Vector3.Distance(transform.position, player.transform.position)
            > detectRange)
        {
            yield return null;
        }
        currentFsm = ChaseFSM;
    }
    public float speed = 34;
    private IEnumerator ChaseFSM()
    {
        animator.Play("Run");
        while (true)
        {
            Vector3 toPlayerDirection = player.transform.position
                - transform.position;
            toPlayerDirection.Normalize();
            transform.Translate(toPlayerDirection * speed * Time.deltaTime, Space.World);

            bool isRightSide = toPlayerDirection.x > 0;
            if (isRightSide)
            {
                transform.rotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 180, 0);
            }

            if (Vector3.Distance(transform.position, player.transform.position) < attackRange)
            {
                currentFsm = AttackFSM;
                yield break;
            }

            yield return null;
        }
    }

    public float attackTime = 1;
    public float attackApplyTime = 0.2f;
    public int power = 10;
    private IEnumerator AttackFSM()
    {
        animator.Play("Attack");
        yield return new WaitForSeconds(attackApplyTime);
        //실제 어택하자.
        if (Vector3.Distance(player.transform.position
            , transform.position) < attackRange)
        {
            //플레이어를 때리자.
            player.TakeHit(power);

            //TakeHit <- 공격 당할때 
        }

        yield return new WaitForSeconds(attackTime - attackApplyTime);
        currentFsm = ChaseFSM;
    }
}
