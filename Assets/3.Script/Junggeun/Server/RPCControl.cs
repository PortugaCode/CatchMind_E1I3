using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class RPCControl : NetworkBehaviour
{
    [SyncVar (hook = nameof(onTextureChanged))]
    public Texture2D white;

    [SerializeField] private GameObject canvas;

    public static Action<Texture2D, Texture2D> onDraw;

    private Drawable drawable;

    [Header("UI")]
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject timerUI;
    [SerializeField] GameObject wordUI;

    [Header("Word Manager")]
    [SerializeField] private GameObject wordManager;

    public string currentWord;

    public int score = 0;

    private void onTextureChanged(Texture2D _old, Texture2D _new)
    {
        white = _new;
    }

    //Client가 Server에 Connect 되었을 때 Callback함수
    public override void OnStartAuthority()
    {

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

    #region [게임 시작]

    [Client]
    public void GameStart() // 누군가가 Start버튼 누름 or 다음 라운드 진행
    {
        GameStart_Command();
    }

    [Command]
    private void GameStart_Command() // 서버에서 커맨드
    {
        GameStart_RPC();
    }

    [ClientRpc]
    private void GameStart_RPC() // 모든 클라이언트들에서 실행
    {
        if (GameManager.instance.isGameStart == false) // 첫 라운드의 경우
        {
            GameManager.instance.isGameStart = true;

            // 1. 스타트 버튼 비활성화
            if (startButton == null)
            {
                startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
            }
            startButton.SetActive(false);

            // 2. Timer UI 활성화
            if (timerUI == null)
            {
                timerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(2).gameObject;
            }
            timerUI.SetActive(true);
        }

        // 4. 제시어 뽑기 & Word (제시어) UI 활성화 - 그림을 그리는 사람만
        if (GetComponent<CanDrawControl>().isCanDraw)
        {
            if (wordUI == null)
            {
                wordUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
            }

            if (wordManager == null)
            {
                wordManager = GameObject.FindGameObjectWithTag("WordManager");
            }

            if (isLocalPlayer)
            {
                string newWord = wordManager.GetComponent<WordManager>().GetRandomWord();

                SendWord(newWord);

                wordUI.SetActive(true);
            }
        }

        // 5. 타이머 메서드 시작
        GameManager.instance.StartRound();
    }

    #endregion

    #region [제시어 동기화]

    [Client]
    private void SendWord(string word) // 새로 뽑은 제시어 뿌려주기
    {
        SendWord_Command(word);
    }

    [Command]
    private void SendWord_Command(string word) // 서버에서 커맨드
    {
        SendWord_RPC(word);
    }

    [ClientRpc]
    private void SendWord_RPC(string word) // 모든 클라이언트의 GameManager에 새 제시어 할당
    {
        currentWord = word;
        GameManager.instance.currentWord = currentWord;
        wordUI.transform.GetChild(1).GetComponent<Text>().text = $"{GameManager.instance.currentWord}";
    }

    #endregion

    #region [정답 판정]

    [Client]
    public void CorrectAnswer(GameObject pen) // 클라이언트가 정답을 맞췄다고 알림
    {
        CorrectAnswer_Command(pen);
    }

    [Command]
    private void CorrectAnswer_Command(GameObject pen) // 서버에서 커맨드
    {
        CorrectAnswer_RPC(pen);
    }

    [ClientRpc]
    private void CorrectAnswer_RPC(GameObject pen) // 모든 클라이언트들에서 실행
    {
        // 1. 게임매니저에 알림 -> 진행중인 라운드 종료 (타이머 Off)
        GameManager.instance.isCorrect = true;

        // 3. 점수 UI 업데이트
        // Canvas의 Users의 자기 위치의 scoreText 업데이트?...

        // 4. 해당 라운드의 제시어 모든 클라이언트에게 공개
        GameManager.instance.ShowCurrentWord(); // 시각적으로 표현

        if (isLocalPlayer)
        {
            pen.GetComponent<ChatControl>().SendAnswer(); // 채팅창에 정답 라인 출력
        }

        // 5. 정답을 맞힌 클라이언트에게 권한 할당
        GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<CanDrawControl>().ChangeCanDraw2(false);
        }
        
        pen.GetComponent<CanDrawControl>().ChangeCanDraw2(true);
        
        if (pen.GetComponent<RPCControl>().score == 3)
        {
            // 게임 종료
        }

        // 6. 다음 라운드 시작
        GameManager.instance.PushStartButton();
    }

    #endregion
}
