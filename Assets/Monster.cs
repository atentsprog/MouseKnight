using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Monster : MonoBehaviour
{
    public static List<Monster> Items = new List<Monster>();
    private void Awake()
    {
        Items.Add(this);
    }

    // 추격 할대 플레이어한테 공격 가능한 거리면 공격.
    // 공격후 추격
    // 추격 공격
    Animator animator;
    SpriteRenderer spriteRenderer;
    IEnumerator Start()
    {
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        player = Player.instance;

        CurrentFsm = IdleFSM;

        while (true) // 상태를 무한히 반복해서 실행하는 부분.
        {
            fsmHandle = StartCoroutine(CurrentFsm());
            while (fsmHandle != null)
                yield return null;
        }
    }
    Coroutine fsmHandle;
    protected Func<IEnumerator> CurrentFsm
    {
        get { return m_currentFsm; }
        set { 
            m_currentFsm = value;
            fsmHandle = null;
        }
    }
    Func<IEnumerator> m_currentFsm;
    protected Player player;
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
        PlayAinmation("Idle");

        ////IdleCo
        // 플레이어 근접하면 추격
        while (Vector3.Distance(transform.position, player.transform.position)
            > detectRange)
        {
            yield return null;
        }
        CurrentFsm = ChaseFSM;
    }
    public float speed = 34;
    protected IEnumerator ChaseFSM()
    {
        PlayAinmation("Run");
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
                SelectAttackType();
                yield break;
            }

            yield return null;
        }
    }

    virtual protected void SelectAttackType()
    {
        CurrentFsm = AttackFSM;
    }

    public float attackTime = 1;
    public float attackApplyTime = 0.2f;
    public int power = 10;
    protected IEnumerator AttackFSM()
    {
        PlayAinmation("Attack");
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
        CurrentFsm = ChaseFSM;
    }

    protected void PlayAinmation(string clipName)
    {
        //Debug.Log(clipName);
        animator.Play(clipName, 0, 0);
    }

    public float hp = 100;
    virtual public void TakeHit(float damage)
    {
        if (hp < 0)
            return;

        hp -= damage;        
        StopCoroutine(fsmHandle);
        CurrentFsm = TakeHitFSM;
    }

    public float takeHitTime = 0.3f;
    private IEnumerator TakeHitFSM()
    {
        PlayAinmation("TakeHit");
        yield return new WaitForSeconds(takeHitTime);
        if (hp > 0)
            CurrentFsm = IdleFSM;
        else
            CurrentFsm = DeathFSM;//hp < 0 으면 죽자.
    }
    public float deathTime = 0.5f;
    internal int gainXp = 50;

    private IEnumerator DeathFSM()
    {
        PlayAinmation("Death");

        Items.Remove(this);
        Debug.Log($"남은 몬스터 수 : {Items.Count}");
        if (Items.Count == 0)
        {
            //StageResultUI.instance.Show();
        }
        yield return new WaitForSeconds(deathTime);

        spriteRenderer.DOFade(0, 1).OnComplete(() => 
            {
                Destroy(gameObject);
            });

        //myAction = DestroySelf;

        //myAction = () =>
        //{
        //    Destroy(gameObject);
        //};

        //myAction();

        //spriteRenderer.DOFade(0, 1).OnComplete(DestroySelf);
    }

    //void DestroySelf()
    //{
    //    Destroy(gameObject);
    //}

    //Action myAction;
}
