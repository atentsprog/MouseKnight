using DG.Tweening;
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
        /// 최초 instance초기화는 어떻게 할 것인가?
        /// GameObject가 켜져야지 Awake가 실행된다. -> 아직 호출 타이밍이 아닌데 켜야하나?
        /// 임시로 인터페이스를 통해 해결하자
        ///// 궁극적으로는 UIBase클래스를 잘 만들어서 해결하자.
        instance = this; 
    }
    Text xpPointText;
    public int allMonsterCount;
    public int killedMonsterCount;
    public float damageTaken;

    private void Start()
    {
        blackScreen = GameObject.Find("PersistCanvas").transform.Find("BlackScreen").GetComponent<CanvasGroup>();
        xpPointText = GameObject.Find("StageCanvas").transform.Find("TopRight/XpPoint/XpPointText").GetComponent<Text>();
        xpPointText.text = "0";

        blackScreen.alpha = 1;
        blackScreen.DOFade(0, 1);
    }

    internal void OnMonsterDie(Monster dieMonster)
    {
        killedMonsterCount++;
        GameManager.instance.playerXp += dieMonster.gainXp;

        //UI에 XP표시.
        xpPointText.text = GameManager.instance.playerXp.ToNumber();

        //모든 몬스터 죽었다면 스테이지 종료 UI표시
        if (Monster.Items.Count == 0)
        {
            ShowStageResultUI();
        }
    }

    public CanvasGroup blackScreen;
    
    private void ShowStageResultUI()
    {
        // 화면 점차적으로 검게 만들자.
        //// 검은색 UI를 화면에 점차적으로 나타나게 하자
        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 0;
        blackScreen.DOFade(1, 1) // 1초 동안 0의 값으로 만들겠다
            .OnComplete(() =>
            {
                //완료되면 밝아지면서 결과 화면 표시

                //결과 화면 표시한뒤 밝게 하자.
                StageResultUI.instance.Show(
                    allMonsterCount, killedMonsterCount
                    , damageTaken);
                // 결과화면에서 Continue누르면 다음 스테이지 로드.
                blackScreen.DOFade(0, 1);
            });
    }

    [Button("모든 몬스터 죽이기")]
    void AllKillMonster()
    {
        Monster.Items.ForEach(x => x.TakeHit(100000));
    }
}
