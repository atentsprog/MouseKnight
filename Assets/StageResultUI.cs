using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageResultUI : BaseUI<StageResultUI>
{
    public override string HierarchyPath => "StageCanvas/StageResultUI";

    Text gradeText;
    Text enemiesKilledText;
    Text damageTakenText;
    Button continueButton;

    override protected void OnInit()
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

    internal void Show(int enemiesKilledCount, int sumMonserCount
        , int damageTakenPoint)
    {
        base.Show(); // 원칙, 

        enemiesKilledText.text = $"{enemiesKilledCount} / {sumMonserCount}";
        damageTakenText.text = damageTakenPoint.ToString();
        gradeText.text = "A";
    }
}
