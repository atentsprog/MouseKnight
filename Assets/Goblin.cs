using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Goblin : MonoBehaviour, ITakeHit
{
    public float watchRange = 25;
    public float attackRange = 10;
    public float speed = 40;
    public float hp = 100;
    public float power = 10;
    private float maxHp;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, watchRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    Image hpBar;
    Animator animator;
    Player target;
    Coroutine fsmHandle;
    IEnumerator Start()
    {
        target = Player.instance;
        animator = GetComponentInChildren<Animator>();
        originalSpeed = speed;
        animator.Play("Idle");

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        infoTextMesh = GetComponentInChildren<TextMesh>();
        hpBar = GetComponentInChildren<Image>();
        maxHp = hp;
        // 플레이어가 근접할때까지 대기
        fsm = IdleCo;

        while (true)
        {
            fsmChange = false;
            fsmHandle = StartCoroutine(fsm());

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
        while (Vector3.Distance(target.transform.position, transform.position) > watchRange)
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

            infoTextMesh.text = fsm.Method.Name.ToString(); ;
        }
    }
    TextMesh infoTextMesh;
    bool fsmChange = false;


    IEnumerator ChaseTargetFSM()
    {
        animator.Play("Run");
        while (fsmChange == false)
        {
            Vector3 toPlayerDirection = transform.position - target.transform.position;
            toPlayerDirection.y = 0;
            toPlayerDirection.Normalize();

            transform.Translate(toPlayerDirection * speed * Time.deltaTime);

            //플레이어가 오른쪽에 있을때 0, 왼쪽일땐 180
            if (toPlayerDirection.x > 0)
                animator.transform.rotation = Quaternion.Euler(0, 180, 0);
            else
                animator.transform.rotation = Quaternion.Euler(Vector3.zero);
            yield return null;
            if (Vector3.Distance(transform.position, target.transform.position) < attackRange)
            {
                Fsm = AttackFSM;
            }
        }
    }
    public float attackAnimationTime = 1;
    public float attackTime = 1;
    public float preAttackAnimationTime = 0.4f;
    IEnumerator AttackFSM()
    {
        animator.Play("Attack");
        //공격범위에 플레이어가 있다면 때린걸로 하자.
        float attackAnimationEndTime = Time.time + attackAnimationTime;
        yield return new WaitForSeconds(preAttackAnimationTime);
        float playerDistance = Vector3.Distance(transform.position, target.transform.position);
        if(playerDistance < attackRange)
        {
            //플레이어 공격 성공
            target.OnTakeHit(power);
        }
        yield return new WaitForSeconds(attackAnimationEndTime - Time.time);
        Fsm = ChaseTargetFSM;
    }
    public float disappearTimeWhenDeath = 1;
    private IEnumerator DeathFSM()
    {
        animator.Play("Death");
        yield return new WaitForSeconds(disappearTimeWhenDeath);
        //StageManager.instance.AddPoint(100);
        //DotTween사용해서 투명해진 다음에 사라지게 하자.
        Destroy(gameObject);
    }

    public float attackedTime = 0.7f;
    private float originalSpeed;

    IEnumerator TakeHitFSM()
    {
        animator.Play("TakeHit");
        speed = 0;
        yield return new WaitForSeconds(attackedTime);

        speed = originalSpeed;
        Fsm = ChaseTargetFSM;
    }

    public void OnTakeHit(float damage)
    {
        hp -= damage;
        hpBar.fillAmount = hp / maxHp;

        if (hp <= 0)
            Fsm = DeathFSM;
        else
            Fsm = TakeHitFSM;
    }
}
