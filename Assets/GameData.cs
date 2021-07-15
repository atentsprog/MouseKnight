using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class StageInfo
{
    public int stageID;
    public string titleString;
    public int rewardXP;
}

public class GameData : MonoBehaviour
{
    public static GameData instance;
    [SerializeField] private List<StageInfo> stageInfos;
    public static Dictionary<int, StageInfo> StageInfoMap 
        = new Dictionary<int, StageInfo>();
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        StageInfoMap = stageInfos.ToDictionary(x => x.stageID);
    }
}
