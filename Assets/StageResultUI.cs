using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageResultUI : BaseUI<StageResultUI>
{
    Text gradeText;
    Text enemiesKilledText;
    Text damageTakenText;
    Button continueButton;

    void Start()
    {
        gradeText = transform.Find("GradeText").GetComponent<Text>();
        enemiesKilledText = transform.Find("EnemiesKilledText").GetComponent<Text>();
        damageTakenText = transform.Find("DamageTakenText").GetComponent<Text>();
        continueButton = transform.Find("ContinueButton").GetComponent<Button>();
        continueButton.AddListener(this, LoadNextStage);
    }

    private void LoadNextStage()
    {
        Debug.LogWarning("LoadNextStage");
    }

    override protected void OnShow()
    {
        enemiesKilledText.text = $"{StageManager.Instance.enemiesKilledCount} / {StageManager.Instance.sumMonserCount}";
        damageTakenText.text = StageManager.Instance.damageTakenPoint.ToString();
        gradeText.text = "A";
    }
}
