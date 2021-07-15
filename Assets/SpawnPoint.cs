using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnType
{
    Player,
    Goblin,
    Skeleton,
    Boss,
}

public class SpawnPoint : MonoBehaviour
{
    public SpawnType spawnType;
    void Awake()
    {
        string spawnPrefabName;
        switch (spawnType)
        {
            case SpawnType.Player:  spawnPrefabName = "Player"; break;
            case SpawnType.Goblin:  spawnPrefabName = "Goblin"; break;
            case SpawnType.Skeleton:spawnPrefabName = "Skeleton"; break;
            case SpawnType.Boss:    spawnPrefabName = "Boss"; break;
            default:
                spawnPrefabName = "";
                break;
        }

        Instantiate(Resources.Load(spawnPrefabName), transform.position, Quaternion.identity);
    }
}
