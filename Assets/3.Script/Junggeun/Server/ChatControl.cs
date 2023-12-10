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

    public List<string> RoundText = new List<string>();

    private static event Action<string> onMessage;

    private List<string> chat_list = new List<string>();
    private int chatCount = 0;

    public override void OnStartAuthority()
    {
        if(isLocalPlayer)
        {
            canvas.SetActive(true);
        }

        onMessage += NewMessage;
    }

    private void NewMessage(string obj)
    {
        chatCount++;

        chat_list.Add(obj);

        if (chatCount == 6)
        {
            chat_list.RemoveAt(0);
            chatCount -= 1;
        }

        chatText.text = string.Empty;

        for (int i = 0; i<chat_list.Count; i++)
        {
            chatText.text += chat_list[i];
        }
    }

    [Client] //Client ���� (Server���� RPC ��û)
    public void Send()
    {
        if (isLocalPlayer && GetComponent<CanDrawControl>().isCanDraw) return;
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        //�������� RPC ��û �޼���
        CMDSendMessage(inputField.text);

        // ���� ���� �� ���� ���� ��û, ������ �� ������ ���� onScoreChanged �޼��忡�� ������ ������ �ٲ� �ڿ� Ȯ��
        if (inputField.text.Equals(GameManager.instance.currentWord))
        {
            GetComponent<RPCControl>().ScoreChange(GetComponent<RPCControl>().score + 1);
        }

        inputField.text = string.Empty;
    }

    [ClientCallback] //Client�� Server�� ������ ��
    private void OnDestroy()
    {
        if (!isLocalPlayer) return;
        onMessage -= NewMessage;
    }

    [Command] //Server ���� (Server���� �ٸ� Client���� �Ѹ��� �۾�)
    private void CMDSendMessage(string me)
    {
        RPCHandleMessage($"[{GetComponent<RPCControl>().userName}] : {me}");
        //RPCHandleMessage($"[{connectionToClient.connectionId}] : {me}");
    }

    [ClientRpc] //�� �޼��带 Client RPC�� ��� (��� Client�� ������ �ִ� Method)
    private void RPCHandleMessage(string me)
    {
        //Debug.Log("RPCHandleMessage");
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
