using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    CanvasGroup blackScreen;
    void Start()
    {
        //검은 화면에서 밝게 한다.
        blackScreen = GameObject.Find("PersistCanvas").transform.Find("BlackScreen")
            .GetComponent<CanvasGroup>();

        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 1; // 0: 안보임, 1 : 보임, 1 -> 블랙 스크린을 보이게 하자
        blackScreen.DOFade(0, 1.7f)
            .OnComplete(() => blackScreen.gameObject.SetActive(false));

        ///!!!! TitleCanvas/Button 에 있던 LoadSceneButton 컴포넌트는 삭제하자.
        //뉴게임 누르면 어두워지게 한다음 스테이지1 로드
        Button button = GameObject.Find("TitleCanvas")
            .transform.Find("Button").GetComponent<Button>();
        button.AddListener(this, OnClickNewGame);
    }

    public void OnClickNewGame()
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOFade(1, 1.7f)
            .OnComplete(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Stage1");
            }); // 1 -> 알파 최대값 -> 어두움을 최대로 보이게 하자
    }
}
