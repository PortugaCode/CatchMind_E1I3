using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class RPCControl : NetworkBehaviour
{
    [SyncVar (hook = nameof(onTextureChanged))]
    public Texture2D white;
    [SerializeField] private GameObject canvas;

    public static Action<Texture2D, Texture2D> onDraw;

    Drawable drawable;

    [Header("UI")]
    [SerializeField] GameObject startButton;

    private void onTextureChanged(Texture2D _old, Texture2D _new)
    {
        Debug.Log("onTextureChanged");
        white = _new;
        //canvas.transform.GetChild(0).GetComponent<RawImage>().texture = (Texture)white;

        /*if (drawable == null)
        {
            drawable = FindObjectOfType<Drawable>();
        }

        drawable.App(white);*/
    }

    //Client�� Server�� Connect �Ǿ��� �� Callback�Լ�
    public override void OnStartAuthority()
    {
        //if (isLocalPlayer)
        //{
        //    canvas = GameObject.FindGameObjectWithTag("Canvas");
        //}

        //drawable = FindObjectOfType<Drawable>();
    }

    public void Draw_2(Texture2D t)
    {
        Debug.Log("Draw_2");
        Draw(t);
    }

    [Command]
    public void Draw(Texture2D t)
    {
        Debug.Log("Draw");
        white = t;
    }

    [Client]
    public void TurnOff()
    {
        TurnOffButton();
    }

    [Command]
    private void TurnOffButton()
    {
        TurnOffButtonUI();
    }

    [ClientRpc]
    private void TurnOffButtonUI()
    {
        if (startButton == null)
        {
            startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
        }

        startButton.SetActive(false);
    }
}
