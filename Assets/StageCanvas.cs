using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageCanvas : MonoBehaviour
{
    public static StageCanvas instance;
    public Text stageNameText;

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject); 
        stageNameText = transform.Find("StageNameText")
            .GetComponent<Text>();
    }
}