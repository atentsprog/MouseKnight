using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    CanvasGroup blackScreen;
    void Start()
    {
        //검은 화면에서 밝게 한다.
        //blackScreen = GameObject.Find("PersistCanvas").transform.Find("BlackScreen")
        //    .GetComponent<CanvasGroup>();
        blackScreen = PersistCanvas.instance.blackScreen;

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

    public enum SceneLoadType
    {
        동기식,
        비동기식
    }
    public SceneLoadType sceneLoadType = SceneLoadType.동기식;
    public void OnClickNewGame()
    {
        switch (sceneLoadType)
        {
            case SceneLoadType.동기식:
                LoadScene();
                break;
            case SceneLoadType.비동기식:
                StartCoroutine(LoadSceneAsync());
                break;
        }
    }

    void LoadScene()
    {
        blackScreen.gameObject.SetActive(true);
        blackScreen.DOKill(); //이전 페이드가 진행중인 경우 멈추게 하기
        blackScreen.DOFade(1, 1.7f)
            .OnComplete(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Stage1");
            }); // 1 -> 알파 최대값 -> 어두움을 최대로 보이게 하자
    }

    IEnumerator LoadSceneAsync()
    {
        var progressBarImage = GameObject.Find("TitleCanvas").transform.Find("ProgressBar").GetComponent<Image>();
        progressBarImage.gameObject.SetActive(true);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Stage1");
        while(asyncOperation.isDone == false)
        {
            float progress = asyncOperation.progress;
            print(progress);
            progressBarImage.fillAmount = 1 - progress;
            yield return null;
        }
    }
}
