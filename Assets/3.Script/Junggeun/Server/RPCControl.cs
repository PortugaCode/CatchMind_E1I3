using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

public class RPCControl : NetworkBehaviour
{
    [SyncVar (hook = nameof(OnTextureChanged))]
    public Texture2D white;

    [SerializeField] private GameObject canvas;

    public static Action<Texture2D, Texture2D> onDraw;

    private Drawable drawable;

    [Header("UI")]
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject timerUI;
    [SerializeField] GameObject wordUI;
    [SerializeField] GameObject resultUI;
    [SerializeField] GameObject answerUI;
    [SerializeField] GameObject drawUI;

    [Header("Word Manager")]
    [SerializeField] private GameObject wordManager;

    public string currentWord;

    //유저의 정보====================
    [SyncVar(hook = nameof(OnNameChanged))]
    public string userName = string.Empty;
    [SyncVar(hook = nameof(OnIndexChanged))]
    public int index = -1;
    [SyncVar(hook = nameof(OnScoreChanged))]
    public int score = 0;

    public bool isGameOver = true;
    //==============================

    public AudioManager audioManager;
    public UIManager uIManager;

    public override void OnStartAuthority() // 클라이언트 접속 시 userName 할당 (OnStartClient보다 먼저 호출됨)
    {
        NameChange(SQL_Manager.instance.info.userName);
    }

    public override void OnStartClient()
    {
        StartCoroutine(Wait_co());
    }

    public override void OnStartLocalPlayer()
    {
        GetComponent<CanDrawControl>().CanDrawInit();
    }

    #region [플레이어 정보]
    private void OnNameChanged(string _old, string _new)
    {
        userName = _new;
    }

    private void OnIndexChanged(int _old, int _new)
    {
        index = _new;
    }

    private void OnScoreChanged(int _old, int _new)
    {
        score = _new;

        if(audioManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
        }
        audioManager.WinSound();
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        bool isLastRound = false;

        foreach (GameObject player in players)
        {
            if (player.GetComponent<RPCControl>().score == GameManager.instance.roundWinScore)
            {
                isLastRound = true;
            }
        }

        if (isLocalPlayer)
        {
            if (isLastRound)
            {
                GetComponent<RPCControl>().GameOver(gameObject);
            }
            else
            {
                GetComponent<RPCControl>().CorrectAnswer(gameObject);
            }
        }
    }

    public void NameChange(string name)
    {
        //메서드
        NameChange_Command(name);
    }

    public void IndexChanged(int index)
    {
        IndexChanged_Command(index);
    }

    public void ScoreChange(int score)
    {
        ScoreChange_Command(score);
    }

    [Command]
    public void NameChange_Command(string name)
    {
        userName = name;
    }

    [Command]
    public void IndexChanged_Command(int C_index)
    {
        index = C_index;
    }

    [Command]
    public void ScoreChange_Command(int C_score)
    {
        score = C_score;
    }
    #endregion

    #region [텍스쳐 변경]
    private void OnTextureChanged(Texture2D _old, Texture2D _new)
    {
        white = _new;
    }

    public void Draw_2(Texture2D t)
    {
        Draw(t);
    }

    [Command]
    public void Draw(Texture2D t)
    {
        white = t;
    }
    #endregion

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
        GameObject[] a = GameObject.FindGameObjectsWithTag("Player");

        if (uIManager == null)
        {
            uIManager = FindObjectOfType<UIManager>();
        }

        if (answerUI == null)
        {
            answerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(6).gameObject;
        }

        if (GameManager.instance.isGameStart == false) // 첫 라운드의 경우
        {
            GameManager.instance.isGameStart = true;
/*            ScoreChange(0);
            uIManager.ChangeScore(index, 0);*/
            for (int i = 0; i < a.Length; i++)
            {
                a[i].GetComponent<RPCControl>().isGameOver = false;
                a[i].GetComponent<RPCControl>().score = 0;
                if(a[i].GetComponent<RPCControl>().uIManager == null)
                {
                    a[i].GetComponent<RPCControl>().uIManager = FindObjectOfType<UIManager>();
                }
                a[i].GetComponent<RPCControl>().uIManager.ChangeScore(a[i].GetComponent<RPCControl>().index, 0);
            }

            // 0. result UI 비활성화
            if (resultUI == null)
            {
                resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
            }
            GetComponent<RPCControl>().resultUI.transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<RPCControl>().resultUI.transform.GetChild(1).gameObject.SetActive(false);

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

        // Answer UI 비활성화
        answerUI.SetActive(false);

        // 4. 제시어 뽑기 & Word (제시어) & 팔레트 활성화 - 그림을 그리는 사람만
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

            if (drawUI == null)
            {
                drawUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(7).gameObject;
            }

            if (isLocalPlayer)
            {
                string newWord = wordManager.GetComponent<WordManager>().GetRandomWord();

                SendWord(newWord);

                wordUI.SetActive(true);
                drawUI.SetActive(true);
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

    #region [정답 맞춘 경우]

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
        /*        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                for(int i = 0; i < players.Length; i++)
                {
                    players[i].GetComponent<RPCControl>().uIManager.
                }*/
        if(uIManager == null)
        {
            uIManager = FindObjectOfType<UIManager>();
        }
        uIManager.ChangeScore(index, score);

        if (isGameOver)
        {
            return;
        }

        GameObject[] a = GameObject.FindGameObjectsWithTag("Player");

        // 1. 게임매니저에 알림 -> 진행중인 라운드 종료 (타이머 Off)
        GameManager.instance.isCorrect = true;

        // 2. 제시어 UI 비활성화
        if (wordUI == null)
        {
            wordUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
        }
        wordUI.SetActive(false);

        // Draw UI 비활성화
        if (drawUI == null)
        {
            drawUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(7).gameObject;
        }
        drawUI.SetActive(false);

        // 3. 점수 UI 업데이트
        // Canvas의 Users의 자기 위치의 scoreText 업데이트?...

        // 4. 해당 라운드의 제시어 모든 클라이언트에게 공개
        if (answerUI == null)
        {
            answerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(6).gameObject;
        }
        answerUI.transform.GetChild(1).GetComponent<Text>().text = $"{pen.GetComponent<RPCControl>().userName}님 정답! (제시어 : {GameManager.instance.currentWord})";
        answerUI.SetActive(true);
        GameManager.instance.ShowCurrentWord(); // 시각적으로 표현

        // 5. 정답을 맞힌 클라이언트에게 권한 할당
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<CanDrawControl>().ChangeCanDraw2(false);
        }
        pen.GetComponent<CanDrawControl>().ChangeCanDraw2(true);

        // 6. 다음 라운드 시작
        if (isGameOver == false)
        {
            GameManager.instance.WaitForNextRound();
        }
    }

    #endregion

    #region [게임 종료]

    [Client]
    public void GameOver(GameObject pen) // 클라이언트가 정답을 맞췄다고 알림
    {
        GameOver_Command(pen);
    }

    [Command]
    private void GameOver_Command(GameObject pen) // 서버에서 커맨드
    {
        GameOver_RPC(pen);
    }

    [ClientRpc]
    private void GameOver_RPC(GameObject pen) // 모든 클라이언트들에서 실행
    {
        // 1. 게임매니저에 알림 -> 진행중인 라운드 종료 (타이머 Off)
        GameManager.instance.isCorrect = true;

        if (uIManager == null)
        {
            uIManager = FindObjectOfType<UIManager>();
        }
        uIManager.ChangeScore(index, score);
        // [모든 라운드 종료]

        if (isGameOver)
        {
            return;
        }

        GameObject[] a = GameObject.FindGameObjectsWithTag("Player"); // 클라이언트들 찾아놓기

        // 모든 클라이언트들의 isGameOver true로 바꾸기
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<RPCControl>().isGameOver = true;
        }

        if (resultUI == null) // Result UI 찾아놓기
        {
            resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
        }

        // 1. 점수 1등은 Win, 나머지 lose UI 띄우기
        /*if (isLocalPlayer)
        {
            if (GetComponent<RPCControl>().score == GameManager.instance.roundWinScore)
            {
                // Win 띄워
                resultUI.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                // Lose 띄워
                resultUI.transform.GetChild(1).gameObject.SetActive(true);
            }
        }*/

        // 제시어 UI 비활성화
        if (wordUI == null)
        {
            wordUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
        }
        wordUI.SetActive(false);

        // Draw UI 비활성화
        if (drawUI == null)
        {
            drawUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(7).gameObject;
        }
        drawUI.SetActive(false);

        // 스타트 버튼 비활성화
        if (startButton == null)
        {
            startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
        }
        startButton.SetActive(false);

        // 1. UI에 1등 이름 띄우기
        if (answerUI == null)
        {
            answerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(6).gameObject;
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].GetComponent<RPCControl>().score == GameManager.instance.roundWinScore)
            {
                answerUI.transform.GetChild(1).GetComponent<Text>().text = $"축하합니다! \n{a[i].GetComponent<RPCControl>().userName}님이 승리하셨습니다";
                answerUI.SetActive(true);
            }
        }

        // 2. isGameStart 초기화
        GameManager.instance.isGameStart = false;

        // 3. TimerUI 비활성화
        if (timerUI == null)
        {
            timerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(2).gameObject;
        }
        timerUI.SetActive(false);

        // 5. 정답을 맞힌 클라이언트에게 권한 할당
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<CanDrawControl>().ChangeCanDraw2(false);
        }
        pen.GetComponent<CanDrawControl>().ChangeCanDraw2(true);

        // 4.StartButton 활성화
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].GetComponent<CanDrawControl>().isCanDraw)
            {
                if (isLocalPlayer)
                {
                    if (startButton == null)
                    {
                        startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
                    }
                    startButton.SetActive(true);
                }
            }
        }
    }

    #endregion

    #region [시간 초과]

    [Client]
    public void TimeOver() // 시간이 초과됨을 알림
    {
        TimeOver_Command();
    }

    [Command]
    private void TimeOver_Command() // 서버에서 커맨드
    {
        TimeOver_RPC();
    }

    [ClientRpc]
    private void TimeOver_RPC() // 모든 클라이언트들에서 실행
    {
        if (isGameOver)
        {
            return;
        }

        // 다 꺼 -> OK
        GetComponent<CanDrawControl>().ChangeCanDraw2(false);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // 제시어 UI 비활성화
        if (wordUI == null)
        {
            wordUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
        }
        wordUI.SetActive(false);

        // Draw UI 비활성화
        if (drawUI == null)
        {
            drawUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(7).gameObject;
        }
        drawUI.SetActive(false);

        // 4. 해당 라운드의 제시어 모든 클라이언트에게 공개
        if (answerUI == null)
        {
            answerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(6).gameObject;
        }
        answerUI.transform.GetChild(1).GetComponent<Text>().text = $"시간 초과!\n(제시어 : {GameManager.instance.currentWord})";
        answerUI.SetActive(true);
        GameManager.instance.ShowCurrentWord(); // 시각적으로 표현

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<CanDrawControl>().isCanDraw)
            {
                int nextNum;

                if (i == players.Length - 1)
                {
                    nextNum = 0;
                }
                else
                {
                    nextNum = i + 1;
                }

                players[nextNum].GetComponent<CanDrawControl>().ChangeCanDraw2(true);

                if (isGameOver == false)
                {
                    Debug.Log("타임오버요");
                    GameManager.instance.WaitForNextRound();
                }

                break;
            }
        }
    }

    #endregion

    #region [펜 색깔 변경]

    [Client]
    public void ChangeColor() // 시간이 초과됨을 알림
    {
        ChangeColor_Command();
    }

    [Command]
    private void ChangeColor_Command() // 서버에서 커맨드
    {
        ChangeColor_RPC();
    }

    [ClientRpc]
    private void ChangeColor_RPC() // 모든 클라이언트들에서 실행
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.GetComponent<Mouse_Drawer>().pencolor = GetComponent<Image>().color;
        }
    }

    #endregion

    private IEnumerator Wait_co() // 클라이언트 userName 할당을 잠시 기다린 후 UI에 이름 띄우기
    {
        yield return new WaitForSeconds(0.25f);

        GameObject canvas = GameObject.FindGameObjectWithTag("Finish");

        if (isLocalPlayer)
        {
            GameManager.instance.localIndex = GetComponent<RPCControl>().index;
            canvas.GetComponent<UIManager>().ChangeNameByIndex();
        }
        else
        {
            canvas.GetComponent<UIManager>().ChangeNameByIndex();
        }

        yield break;
    }
}
