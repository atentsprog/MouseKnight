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
            Vector3 toPlayerDirection = player.transform.position - transform.position;
            toPlayerDirection.Normalize();
            transform.Translate(toPlayerDirection * speed * Time.deltaTime);
            yield return null;
        }
    }
}
