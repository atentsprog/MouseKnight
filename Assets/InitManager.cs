using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
interface IInit
{
    void InitInstance();
}
public class InitManager : MonoBehaviour
{
    void Awake()
    {
        List<GameObject> allObject = new List<GameObject>(FindObjectsOfType<GameObject>());
        print(allObject.Count());

        var rootObj = allObject.Where(x => x.transform.parent == null);
        print(rootObj.Count());

        var inits = rootObj.Select(x => x.GetComponentsInChildren<IInit>(true));
        print(inits.Count());

        foreach (var itemArray in inits)
        {
            foreach( var item in itemArray)
                item.InitInstance();
        }
    }
}
