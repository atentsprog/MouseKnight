using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  스테이지에서 발생하는 모든 이벤트 관리
///  스테이지 부서지면 같이 부서짐.
///  
/// 
/// 1. 플레이어 로드게임 시작시까지 플레이어 대기시킴
/// 2. 스테이지 시작시 화면 밝아지게함, 
/// 3. 몬스터 로드
/// </summary>
/// 
public enum GameStateType
{
    Ready,
    Playing,
    StageEnd,
}
public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    public GameStateType gameState = GameStateType.Ready;
    private void Awake()
    {
        instance = this;
        gameState = GameStateType.Ready;
    }
    IEnumerator Start()
    {
        //화면 어두운 상태로 만들고 2초동안 밝아지게 하자.
        CanvasGroup blackScreen = PersistCanvas.instance.blackScreen;
        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 1; // 0: 안보임, 1 : 보임, 1 -> 블랙 스크린을 보이게 하자
        blackScreen.DOFade(0, 1.7f);
        yield return new WaitForSeconds(1.7f);

        // 스테이지 이름 표시하자.
        string stageName = "Stage " + SceneProperty.instance.StageID;

        // 2 초 쉬었다가
        StageCanvas.instance.stageNameText.text = stageName;

        //플레이어를 움직일 수 있게 하자.
        gameState = GameStateType.Playing;
    }
}