using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideCursorManager : MonoBehaviour
{   
    void OnApplicationFocus(bool focus)
    {
        ShowCursor(!focus);
    }

    private void ShowCursor(bool visible)
    {
        Debug.Log("focus:" + visible);
        StartCoroutine(SetCursorVislbleCo(visible));
    }

    private IEnumerator SetCursorVislbleCo(bool visible)
    {
        yield return null;
        Cursor.visible = visible;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ShowCursor(!Cursor.visible);
    }
}
