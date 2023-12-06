using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class ChatControl : NetworkBehaviour
{
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Button button;

    public List<string> RoundText = new List<string>();

    private static event Action<string> onMessage;



    public override void OnStartAuthority()
    {
        if(!isLocalPlayer)
        {
            canvas.SetActive(true);
        }

        onMessage += NewMessage;
    }

    private void NewMessage(string obj)
    {
        chatText.text += obj;
    }

    [ClientCallback] //Client�� Server�� ������ ��
    private void OnDestroy()
    {
        if (!isLocalPlayer) return;
        onMessage -= NewMessage;
    }

    [Client] //Client ���� (Server���� RPC ��û)
    public void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        //�������� RPC ��û �޼���
        CMDSendMessage(inputField.text);
        inputField.text = string.Empty;
    }



    [Command] //Server ���� (Server���� �ٸ� Client���� �Ѹ��� �۾�)
    private void CMDSendMessage(string me)
    {
        RPCHandleMessage($"[{connectionToClient.connectionId}] : {me}");
    }

    [ClientRpc] //�� �޼��带 Client RPC�� ��� (��� Client�� ������ �ִ� Method)
    private void RPCHandleMessage(string me)
    {
        onMessage?.Invoke($"\n{me}");
        RoundText.Add(me);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Send();
        }
    }
}
