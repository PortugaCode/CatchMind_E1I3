using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CanDrawControl : NetworkBehaviour
{
    [SyncVar(hook = nameof(onCanDrawChanged))]
    public bool isCanDraw;

    [SerializeField] GameObject img;
    [SerializeField] GameObject panel;



    private void Start()
    {
        //img = GameObject.FindGameObjectWithTag("Finish");
        //panel = GameObject.FindGameObjectWithTag("Respawn");
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
        panel.SetActive(isCanDraw);
    }

    private void onCanDrawChanged(bool _old, bool _new)
    {
        Debug.Log($"{_new}");
        isCanDraw = _new;
    }

    public void ChangeCanDraw2(bool t)
    {
        //Debug.Log($"ChangeCanDraw2{t}");
        ChangeCanDraw(t);
        
        if (img == null)
        {
            img = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(1).gameObject;
        }
        if (panel == null)
        {
            panel = GameObject.FindGameObjectWithTag("Respawn").transform.GetChild(0).gameObject;
        }

        //Debug.Log($"Change Can Draw2 에서는 {isCanDraw}");

    }

    [Command]
    public void ChangeCanDraw(bool t)
    {
        isCanDraw = t;
        //Debug.Log($"Change Can Draw 에서는 {isCanDraw}");
    }
}
