using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    void Awake()
    {
        instance = this;
    }
    Text xpPointText;

    private void Start()
    {
        xpPointText = GameObject.Find("StageCanvas").transform.Find("TopRight/XpPoint/XpPointText").GetComponent<Text>();
        xpPointText.text = "0";
    }
    public int playerXp;
    internal void OnMonsterDie(Goblin dieMonster)
    {
        playerXp += dieMonster.gainXp;

        //UI에 XP표시.
        xpPointText.text = playerXp.ToNumber();

        ////모든 몬스터 죽었다면 스테이지 종료 UI표시
        //if(Goblin.Items.Count == 0)
        //{

        //}
    }

    [Button("모든 몬스터 죽이기")]
    void AllKillMonster()
    {
        Goblin.Items.ForEach(x => x.TakeHit(100000));
    }
}
