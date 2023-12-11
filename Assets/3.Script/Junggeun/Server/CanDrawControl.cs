using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CanDrawControl : NetworkBehaviour
{
    [SyncVar(hook = nameof(onCanDrawChanged))]
    public bool isCanDraw;
    
    [Header("UI")]
    [SerializeField] GameObject img;
    [SerializeField] GameObject panel;
    [SerializeField] GameObject word;
    [SerializeField] GameObject startButton;

    private void Update()
    {
        if (isLocalPlayer)
        {
            img.SetActive(!isCanDraw);
            panel.SetActive(isCanDraw);
        }
    }

    private void onCanDrawChanged(bool _old, bool _new)
    {
        isCanDraw = _new;

        // �հ� ������ ����
        GameObject canvas = GameObject.FindGameObjectWithTag("Finish");
        canvas.GetComponent<UIManager>().CrownIcon();
    }

    public void CanDrawInit()
    {
        GameObject[] a = GameObject.FindGameObjectsWithTag("Player");

        if (a.Length <= 1)
        {
            ChangeCanDraw2(true);

            // ��ŸƮ ��ư Ȱ��ȭ
            if (startButton == null)
            {
                startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
            }
            startButton.SetActive(true);
        }
        else
        {
            ChangeCanDraw2(false);
        }
    }

    public void ChangeCanDraw2(bool t)
    {
        ChangeCanDraw(t);
        
        if (img == null)
        {
            img = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(1).gameObject;
        }
        if (panel == null)
        {
            panel = GameObject.FindGameObjectWithTag("Respawn").transform.GetChild(0).gameObject;
        }
        if (word == null)
        {
            word = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
        }
    }

    [Command]
    public void ChangeCanDraw(bool t)
    {
        isCanDraw = t;
    }
}
