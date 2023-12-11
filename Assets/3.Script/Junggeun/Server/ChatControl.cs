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
    [SerializeField] private UIManager UImanager;

    public List<string> RoundText = new List<string>();

    private static event Action<string> onMessage;

    private List<string> chat_list = new List<string>();
    private int chatCount = 0;

    private void Awake()
    {
        UImanager = FindObjectOfType<UIManager>();
    }

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

    [Client] //Client 입장 (Server에게 RPC 요청)
    public void Send()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;

        // 서버에게 RPC 요청 메서드
        CMDSendMessage(inputField.text);

        // 그림을 그리는 사람은 정답 체크 하지 않음
        if (isLocalPlayer && GetComponent<CanDrawControl>().isCanDraw)
        {
            inputField.text = string.Empty;
            return;
        }

        // 정답 판정 후 점수 변경 요청, 게임을 더 진행할 지는 onScoreChanged 메서드에서 점수가 완전히 바뀐 뒤에 확인
        
        if (GameManager.instance.currentWord != string.Empty && inputField.text.Equals(GameManager.instance.currentWord))
        {
            GetComponent<RPCControl>().ScoreChange(GetComponent<RPCControl>().score + 1);
        }

        if (GameManager.instance.currentWord != string.Empty)
        {
            Debug.Log("currentWord 비어있음");
        }

        if (!inputField.text.Contains(GameManager.instance.currentWord))
        {
            Debug.Log("포함 안하고 있음");
        }

        inputField.text = string.Empty;
    }

    [ClientCallback] //Client가 Server를 나갔을 때
    private void OnDestroy()
    {
        if (!isLocalPlayer) return;
        onMessage -= NewMessage;
    }

    [Command] //Server 입장 (Server에서 다른 Client에게 뿌리는 작업)
    private void CMDSendMessage(string me)
    {
        RPCHandleMessage($"[{GetComponent<RPCControl>().userName}] : {me}");
        //RPCHandleMessage($"[{connectionToClient.connectionId}] : {me}");
    }

    [ClientRpc] //이 메서드를 Client RPC로 사용 (모든 Client가 가지고 있는 Method)
    private void RPCHandleMessage(string me)
    {
        onMessage?.Invoke($"\n{me}");
        RoundText.Add(me);
    }

    public string DebugCheck()
    {
        return GameManager.instance.currentWord;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Send();
        }
    }
}
