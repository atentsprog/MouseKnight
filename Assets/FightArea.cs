using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightArea : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;

        var SpawnPoints = GetComponentsInChildren<SpawnPoint>();
        foreach(var item in SpawnPoints)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() == null)
            return;

        var SpawnPoints = GetComponentsInChildren<SpawnPoint>(true);
        foreach (var item in SpawnPoints)
        {
            item.gameObject.SetActive(true);
        }
    }

}
