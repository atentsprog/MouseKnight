﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneProperty : MonoBehaviour
{
    public int stageID;
    public enum SceneType
    {
        Stage,
        Title,
    }
    public SceneType sceneType = SceneType.Stage;

    private void Awake()
    {
        switch(sceneType)
        {
            case SceneType.Stage: StageCanvasLoad(); break;
        }
    }

    private void StageCanvasLoad()
    {
        if (GameObject.Find("StageCanvas") != null)
            return;
        Instantiate(Resources.Load<GameObject>("StageCanvas"));
    }
}
