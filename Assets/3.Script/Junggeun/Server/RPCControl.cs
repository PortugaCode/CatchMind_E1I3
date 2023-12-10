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

    //������ ����====================
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

    public override void OnStartAuthority() // Ŭ���̾�Ʈ ���� �� userName �Ҵ� (OnStartClient���� ���� ȣ���)
    {
        NameChange(SQL_Manager.instance.info.userName);
    }

    public override void OnStartClient()
    {
        StartCoroutine(Wait_co());
    }
    
    #region [�÷��̾� ����]
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
        //�޼���
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

    #region [�ؽ��� ����]
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

    #region [���� ����]

    [Client]
    public void GameStart() // �������� Start��ư ���� or ���� ���� ����
    {
        GameStart_Command();
    }

    [Command]
    private void GameStart_Command() // �������� Ŀ�ǵ�
    {
        GameStart_RPC();
    }

    [ClientRpc]
    private void GameStart_RPC() // ��� Ŭ���̾�Ʈ�鿡�� ����
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

        if (GameManager.instance.isGameStart == false) // ù ������ ���
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

            // 0. result UI ��Ȱ��ȭ
            if (resultUI == null)
            {
                resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
            }
            GetComponent<RPCControl>().resultUI.transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<RPCControl>().resultUI.transform.GetChild(1).gameObject.SetActive(false);

            // 1. ��ŸƮ ��ư ��Ȱ��ȭ
            if (startButton == null)
            {
                startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
            }
            startButton.SetActive(false);

            // 2. Timer UI Ȱ��ȭ
            if (timerUI == null)
            {
                timerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(2).gameObject;
            }
            timerUI.SetActive(true);
        }

        // Answer UI ��Ȱ��ȭ
        answerUI.SetActive(false);

        // 4. ���þ� �̱� & Word (���þ�) & �ȷ�Ʈ Ȱ��ȭ - �׸��� �׸��� �����
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

        // 5. Ÿ�̸� �޼��� ����
        GameManager.instance.StartRound();
    }

    #endregion

    #region [���þ� ����ȭ]

    [Client]
    private void SendWord(string word) // ���� ���� ���þ� �ѷ��ֱ�
    {
        SendWord_Command(word);
    }

    [Command]
    private void SendWord_Command(string word) // �������� Ŀ�ǵ�
    {
        SendWord_RPC(word);
    }

    [ClientRpc]
    private void SendWord_RPC(string word) // ��� Ŭ���̾�Ʈ�� GameManager�� �� ���þ� �Ҵ�
    {
        currentWord = word;
        GameManager.instance.currentWord = currentWord;
        wordUI.transform.GetChild(1).GetComponent<Text>().text = $"{GameManager.instance.currentWord}";
    }

    #endregion

    #region [���� ���� ���]

    [Client]
    public void CorrectAnswer(GameObject pen) // Ŭ���̾�Ʈ�� ������ ����ٰ� �˸�
    {
        CorrectAnswer_Command(pen);
    }

    [Command]
    private void CorrectAnswer_Command(GameObject pen) // �������� Ŀ�ǵ�
    {
        CorrectAnswer_RPC(pen);
    }

    [ClientRpc]
    private void CorrectAnswer_RPC(GameObject pen) // ��� Ŭ���̾�Ʈ�鿡�� ����
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

        // 1. ���ӸŴ����� �˸� -> �������� ���� ���� (Ÿ�̸� Off)
        GameManager.instance.isCorrect = true;

        // 2. ���þ� UI ��Ȱ��ȭ
        if (wordUI == null)
        {
            wordUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
        }
        wordUI.SetActive(false);

        // Draw UI ��Ȱ��ȭ
        if (drawUI == null)
        {
            drawUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(7).gameObject;
        }
        drawUI.SetActive(false);

        // 3. ���� UI ������Ʈ
        // Canvas�� Users�� �ڱ� ��ġ�� scoreText ������Ʈ?...

        // 4. �ش� ������ ���þ� ��� Ŭ���̾�Ʈ���� ����
        if (answerUI == null)
        {
            answerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(6).gameObject;
        }
        answerUI.transform.GetChild(1).GetComponent<Text>().text = $"{pen.GetComponent<RPCControl>().userName}�� ����! (���þ� : {GameManager.instance.currentWord})";
        answerUI.SetActive(true);
        GameManager.instance.ShowCurrentWord(); // �ð������� ǥ��

        // 5. ������ ���� Ŭ���̾�Ʈ���� ���� �Ҵ�
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<CanDrawControl>().ChangeCanDraw2(false);
        }
        pen.GetComponent<CanDrawControl>().ChangeCanDraw2(true);

        // 6. ���� ���� ����
        if (isGameOver == false)
        {
            GameManager.instance.WaitForNextRound();
        }
    }

    #endregion

    #region [���� ����]

    [Client]
    public void GameOver(GameObject pen) // Ŭ���̾�Ʈ�� ������ ����ٰ� �˸�
    {
        GameOver_Command(pen);
    }

    [Command]
    private void GameOver_Command(GameObject pen) // �������� Ŀ�ǵ�
    {
        GameOver_RPC(pen);
    }

    [ClientRpc]
    private void GameOver_RPC(GameObject pen) // ��� Ŭ���̾�Ʈ�鿡�� ����
    {
        // 1. ���ӸŴ����� �˸� -> �������� ���� ���� (Ÿ�̸� Off)
        GameManager.instance.isCorrect = true;

        if (uIManager == null)
        {
            uIManager = FindObjectOfType<UIManager>();
        }
        uIManager.ChangeScore(index, score);
        // [��� ���� ����]

        if (isGameOver)
        {
            return;
        }

        GameObject[] a = GameObject.FindGameObjectsWithTag("Player"); // Ŭ���̾�Ʈ�� ã�Ƴ���

        // ��� Ŭ���̾�Ʈ���� isGameOver true�� �ٲٱ�
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<RPCControl>().isGameOver = true;
        }

        if (resultUI == null) // Result UI ã�Ƴ���
        {
            resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
        }

        // 1. ���� 1���� Win, ������ lose UI ����
        /*if (isLocalPlayer)
        {
            if (GetComponent<RPCControl>().score == GameManager.instance.roundWinScore)
            {
                // Win ���
                resultUI.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                // Lose ���
                resultUI.transform.GetChild(1).gameObject.SetActive(true);
            }
        }*/

        // ���þ� UI ��Ȱ��ȭ
        if (wordUI == null)
        {
            wordUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
        }
        wordUI.SetActive(false);

        // Draw UI ��Ȱ��ȭ
        if (drawUI == null)
        {
            drawUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(7).gameObject;
        }
        drawUI.SetActive(false);

        // ��ŸƮ ��ư ��Ȱ��ȭ
        if (startButton == null)
        {
            startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
        }
        startButton.SetActive(false);

        // 1. UI�� 1�� �̸� ����
        if (answerUI == null)
        {
            answerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(6).gameObject;
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].GetComponent<RPCControl>().score == GameManager.instance.roundWinScore)
            {
                answerUI.transform.GetChild(1).GetComponent<Text>().text = $"�����մϴ�! \n{a[i].GetComponent<RPCControl>().userName}���� �¸��ϼ̽��ϴ�";
                answerUI.SetActive(true);
            }
        }

        // 2. isGameStart �ʱ�ȭ
        GameManager.instance.isGameStart = false;

        // 3. TimerUI ��Ȱ��ȭ
        if (timerUI == null)
        {
            timerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(2).gameObject;
        }
        timerUI.SetActive(false);

        // 5. ������ ���� Ŭ���̾�Ʈ���� ���� �Ҵ�
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<CanDrawControl>().ChangeCanDraw2(false);
        }
        pen.GetComponent<CanDrawControl>().ChangeCanDraw2(true);

        // 4.StartButton Ȱ��ȭ
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

    #region [�ð� �ʰ�]

    [Client]
    public void TimeOver() // �ð��� �ʰ����� �˸�
    {
        TimeOver_Command();
    }

    [Command]
    private void TimeOver_Command() // �������� Ŀ�ǵ�
    {
        TimeOver_RPC();
    }

    [ClientRpc]
    private void TimeOver_RPC() // ��� Ŭ���̾�Ʈ�鿡�� ����
    {
        if (isGameOver)
        {
            return;
        }

        // �� �� -> OK
        GetComponent<CanDrawControl>().ChangeCanDraw2(false);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // ���þ� UI ��Ȱ��ȭ
        if (wordUI == null)
        {
            wordUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(3).gameObject;
        }
        wordUI.SetActive(false);

        // Draw UI ��Ȱ��ȭ
        if (drawUI == null)
        {
            drawUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(7).gameObject;
        }
        drawUI.SetActive(false);

        // 4. �ش� ������ ���þ� ��� Ŭ���̾�Ʈ���� ����
        if (answerUI == null)
        {
            answerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(6).gameObject;
        }
        answerUI.transform.GetChild(1).GetComponent<Text>().text = $"�ð� �ʰ�!\n(���þ� : {GameManager.instance.currentWord})";
        answerUI.SetActive(true);
        GameManager.instance.ShowCurrentWord(); // �ð������� ǥ��

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
                    Debug.Log("Ÿ�ӿ�����");
                    GameManager.instance.WaitForNextRound();
                }

                break;
            }
        }
    }

    #endregion

    #region [�� ���� ����]

    [Client]
    public void ChangeColor() // �ð��� �ʰ����� �˸�
    {
        ChangeColor_Command();
    }

    [Command]
    private void ChangeColor_Command() // �������� Ŀ�ǵ�
    {
        ChangeColor_RPC();
    }

    [ClientRpc]
    private void ChangeColor_RPC() // ��� Ŭ���̾�Ʈ�鿡�� ����
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.GetComponent<Mouse_Drawer>().pencolor = GetComponent<Image>().color;
        }
    }

    #endregion

    private IEnumerator Wait_co() // Ŭ���̾�Ʈ userName �Ҵ��� ��� ��ٸ� �� UI�� �̸� ����
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
