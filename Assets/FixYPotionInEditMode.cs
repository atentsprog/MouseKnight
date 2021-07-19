using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FixYPotionInEditMode : MonoBehaviour
{
    void Start()
    {
        if (Application.isPlaying)
            Destroy(gameObject);
    }
    void Update()
    {
        var pos = transform.position;
        pos.y = 0;
        transform.position = pos;
    }

    private void OnDrawGizmos()
    {
        SpawnType spawnType = GetComponent<SpawnPoint>().spawnType;

        string iconName;
        switch (spawnType)
        {
            case SpawnType.Player: iconName = "Player"; break;
            case SpawnType.Goblin: iconName = "Goblin"; break;
            case SpawnType.Skeleton: iconName = "Skeleton"; break;
            case SpawnType.Boss: iconName = "Boss"; break;
            default:
                iconName = "";
                break;
        }
        var f = iconName + ".png";
        Gizmos.DrawIcon(transform.position,f, true);
    }
}
