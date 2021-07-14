using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Monster
{

    // 방패 막기 추가.
    // 공격 하는 타이밍에 공격 대신 막기 랜덤하게 진행.
    // 막고 있는 동안에는 데미지 없음(대신 막았다는 이펙트 생성)
    override protected void SelectAttackType()
    {
        if(Random.Range(0, 1f) > 0.5f)
            CurrentFsm = AttackFSM;
        else
            CurrentFsm = ShieldFSM;
    }

    bool isOnShield = false;
    public float activeShieldTime = 2;
    protected IEnumerator ShieldFSM()
    {
        PlayAinmation("Shield");
        isOnShield = true;
        yield return new WaitForSeconds(activeShieldTime);
        isOnShield = false;
        CurrentFsm = ChaseFSM;
    }

    public GameObject succeedBlockEffect;
    override public void TakeHit(float damage)
    {
        if(isOnShield)
        {
            Instantiate(succeedBlockEffect, transform.position, Quaternion.identity);
        }
        else
        {
            base.TakeHit(damage);
        }
    }
}
