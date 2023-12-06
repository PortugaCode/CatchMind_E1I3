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

    [ClientCallback] //Client가 Server를 나갔을 때
    private void OnDestroy()
    {
        if (!isLocalPlayer) return;
        onMessage -= NewMessage;
    }

    [Client] //Client 입장 (Server에게 RPC 요청)
    public void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        //서버에게 RPC 요청 메서드
        CMDSendMessage(inputField.text);
        inputField.text = string.Empty;
    }



    [Command] //Server 입장 (Server에서 다른 Client에게 뿌리는 작업)
    private void CMDSendMessage(string me)
    {
        RPCHandleMessage($"[{connectionToClient.connectionId}] : {me}");
    }

    [ClientRpc] //이 메서드를 Client RPC로 사용 (모든 Client가 가지고 있는 Method)
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
