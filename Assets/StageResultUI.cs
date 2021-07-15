using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageResultUI : MonoBehaviour, IInit
{
    public static StageResultUI instance;
    
    Text gradeText;
    Text enemiesKilledText;
    Text damageTakenText;

    public void InitInstance()
    {
        instance = this;

        Button continueButton = transform.Find("UI/ContinueButton").GetComponent<Button>();
        continueButton.AddListener(this, LoadNextStage);
        gradeText = transform.Find("UI/GradeText").GetComponent<Text>();
        enemiesKilledText = transform.Find("UI/EnemiesKilledText").GetComponent<Text>();
        damageTakenText = transform.Find("UI/DamageTakenText").GetComponent<Text>();
    }

    private void LoadNextStage()
    {
        string nextStageName = "Stage" + (SceneProperty.instance.stageID + 1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextStageName);
    }


    internal void Show(int allMonsterCount, int killedMonsterCount, float damageTaken)
    {
        gameObject.SetActive(true);
        enemiesKilledText.text = $"{killedMonsterCount} / {allMonsterCount}";
        damageTakenText.text = ((int)damageTaken).ToString();

        float killPercent = (float)killedMonsterCount / allMonsterCount;
        

        string gradeStr;
        float damageTakenPoint = 50 - damageTaken;
        float killPoint = 50 * killPercent; // 다 죽이면 50, 50% 죽였다면 25, 0%죽였다면 0
        
        float sumPoint = damageTakenPoint + killPoint;
        if (sumPoint > 90)  gradeStr = "SS";
        else if (sumPoint > 80) gradeStr = "S";
        else if (sumPoint > 70) gradeStr = "A";
        else if (sumPoint > 60) gradeStr = "B";
        else if (sumPoint > 50) gradeStr = "C";
        else gradeStr = "F";

        gradeText.text = gradeStr; // 몬스터 죽인 숫자와 피격 데미지를 기준으로 적절하게 등급 매겨보자.
    }

}
