using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CanDrawControl : NetworkBehaviour
{
    [SyncVar(hook = nameof(onCanDrawChanged))]
    public bool isCanDraw;

    GameObject img;

    private void Start()
    {
        img = GameObject.FindGameObjectWithTag("Finish");
        GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
        if (a.Length <= 1)
        {
            ChangeCanDraw2(true);
        }
        else
        {
            ChangeCanDraw2(false);
        }
    }
    private void Update()
    {
        img.SetActive(!isCanDraw);
    }

    private void onCanDrawChanged(bool _old, bool _new)
    {
        Debug.Log("onCanDrawChanged");
        isCanDraw = _new;
    }

    public void ChangeCanDraw2(bool t)
    {
        Debug.Log($"ChangeCanDraw2{t}");
        ChangeCanDraw(t);
    }

    [Command]
    public void ChangeCanDraw(bool t)
    {
        Debug.Log($"ChangeCanDraw{t}");
        isCanDraw = t;
    }
}
