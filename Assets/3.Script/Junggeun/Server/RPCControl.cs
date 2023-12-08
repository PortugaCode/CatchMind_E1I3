using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

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
    [SerializeField] GameObject resultUI;

    [Header("Word Manager")]
    [SerializeField] private GameObject wordManager;

    public string currentWord;

    //유저의 정보====================
    [SyncVar(hook = nameof(onNameChanged))]
    public string userName = string.Empty;
    [SyncVar(hook = nameof(onIndexChanged))]
    public int index = -1;
    [SyncVar(hook = nameof(onScoreChanged))]
    public int score = 0;

    public bool isGameOver = true;
    //==============================


    //Client가 Server에 Connect 되었을 때 Callback함수
    public override void OnStartAuthority()
    {
        NameChange(SQL_Manager.instance.info.userName);
    }

    public override void OnStartClient()
    {
        //base.OnStartClient();
        StartCoroutine(wait());
    }




    #region [플레이어 정보]
    private void onNameChanged(string _old, string _new)
    {
        userName = _new;
    }

    private void onIndexChanged(int _old, int _new)
    {
        index = _new;
    }
    private void onScoreChanged(int _old, int _new)
    {
        score = _new;
        score -= 1;
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
        score += C_score;
    }
    #endregion

    #region [텍스쳐 변경]
    private void onTextureChanged(Texture2D _old, Texture2D _new)
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
        if (GameManager.instance.isGameStart == false) // 첫 라운드의 경우
        {
            GameManager.instance.isGameStart = true;

            GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < a.Length; i++)
            {
                a[i].GetComponent<RPCControl>().isGameOver = false;
            }

            // 0. result UI 비활성화
            if (resultUI == null)
            {
                resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
            }
            GetComponent<RPCControl>().resultUI.transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<RPCControl>().resultUI.transform.GetChild(1).gameObject.SetActive(false);

            // score 초기화
            GetComponent<RPCControl>().score = 0;

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
        if (isGameOver)
        {
            return;
        }

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

        Debug.Log($"isGameOver가 {isGameOver} 입니다!!");

        // 6. 다음 라운드 시작
        if (isGameOver == false)
        {
            Debug.Log("다음 라운드 시작합니다");
            GameManager.instance.PushStartButton();
        }
    }

    #endregion

    #region [게임 종료]

    [Client]
    public void GameOver() // 클라이언트가 정답을 맞췄다고 알림
    {
        GameOver_Command();
    }

    [Command]
    private void GameOver_Command() // 서버에서 커맨드
    {
        GameOver_RPC();
    }

    [ClientRpc]
    private void GameOver_RPC() // 모든 클라이언트들에서 실행
    {
        if (isGameOver)
        {
            return;
        }

        GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
        for(int i = 0; i< a.Length; i++)
        {
            a[i].GetComponent<RPCControl>().isGameOver = true;
        }
        //GetComponent<RPCControl>().isGameOver = true;

        // 게임 종료
        if (resultUI == null)
        {
            resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
        }

        // 1. 점수 1등인애는 Win 나머지 lose UI 띄우기
        if (GetComponent<RPCControl>().score == 2)
        {
            // Win 띄워
            resultUI.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            // Lose 띄워
            resultUI.transform.GetChild(1).gameObject.SetActive(true);
        }

        //Debug.Log($"{GetComponent<RPCControl>().userName}의  isGameOver : {isGameOver}");

        // 2. isGameStart 초기화
        GameManager.instance.isGameStart = false;

        // 3. TimerUI 비활성화
        if (timerUI == null)
        {
            timerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(2).gameObject;
        }
        timerUI.SetActive(false);

        // 4. StartButton 활성화
        if (startButton == null)
        {
            startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
        }
        startButton.SetActive(true);
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
                    GameManager.instance.PushStartButton();
                }

                break;
            }
        }
    }

    #endregion

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject canvas = GameObject.FindGameObjectWithTag("Finish");
        canvas.GetComponent<UIManager>().ChangeNameByIndex();

        yield break;
    }
}
