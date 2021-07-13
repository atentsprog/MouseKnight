using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    Text pointText;
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);

        pointText = GameObject.Find("StageCanvas").transform.Find("Point/PointText").GetComponent<Text>();
        pointText.text = "0";


        Button nextStageButton = GameObject.Find("StageCanvas").transform.Find("NextStageButton").GetComponent<Button>();
        nextStageButton.AddListener(this, LoadNextStage);
        nextStageButton.gameObject.SetActive(false);
    }

    private void LoadNextStage()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Stage2");
    }

    int point;
    internal void AddPoint(int addPoint)
    {
        point += addPoint;
        pointText.text = point.ToNumber();
    }
}
