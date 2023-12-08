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

    //������ ����====================
    [SyncVar(hook = nameof(onNameChanged))]
    public string userName = string.Empty;
    [SyncVar(hook = nameof(onIndexChanged))]
    public int index = -1;
    [SyncVar(hook = nameof(onScoreChanged))]
    public int score = 0;

    public bool isGameOver = true;
    //==============================


    //Client�� Server�� Connect �Ǿ��� �� Callback�Լ�
    public override void OnStartAuthority()
    {
        NameChange(SQL_Manager.instance.info.userName);
    }

    public override void OnStartClient()
    {
        //base.OnStartClient();
        StartCoroutine(wait());
    }




    #region [�÷��̾� ����]
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
        score += C_score;
    }
    #endregion

    #region [�ؽ��� ����]
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
        if (GameManager.instance.isGameStart == false) // ù ������ ���
        {
            GameManager.instance.isGameStart = true;

            GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < a.Length; i++)
            {
                a[i].GetComponent<RPCControl>().isGameOver = false;
            }

            // 0. result UI ��Ȱ��ȭ
            if (resultUI == null)
            {
                resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
            }
            GetComponent<RPCControl>().resultUI.transform.GetChild(0).gameObject.SetActive(false);
            GetComponent<RPCControl>().resultUI.transform.GetChild(1).gameObject.SetActive(false);

            // score �ʱ�ȭ
            GetComponent<RPCControl>().score = 0;

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

        // 4. ���þ� �̱� & Word (���þ�) UI Ȱ��ȭ - �׸��� �׸��� �����
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

    #region [���� ����]

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
        if (isGameOver)
        {
            return;
        }

        // 1. ���ӸŴ����� �˸� -> �������� ���� ���� (Ÿ�̸� Off)
        GameManager.instance.isCorrect = true;

        // 3. ���� UI ������Ʈ
        // Canvas�� Users�� �ڱ� ��ġ�� scoreText ������Ʈ?...

        // 4. �ش� ������ ���þ� ��� Ŭ���̾�Ʈ���� ����
        GameManager.instance.ShowCurrentWord(); // �ð������� ǥ��

        if (isLocalPlayer)
        {
            pen.GetComponent<ChatControl>().SendAnswer(); // ä��â�� ���� ���� ���
        }

        // 5. ������ ���� Ŭ���̾�Ʈ���� ���� �Ҵ�
        GameObject[] a = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < a.Length; i++)
        {
            a[i].GetComponent<CanDrawControl>().ChangeCanDraw2(false);
        }

        pen.GetComponent<CanDrawControl>().ChangeCanDraw2(true);

        Debug.Log($"isGameOver�� {isGameOver} �Դϴ�!!");

        // 6. ���� ���� ����
        if (isGameOver == false)
        {
            Debug.Log("���� ���� �����մϴ�");
            GameManager.instance.PushStartButton();
        }
    }

    #endregion

    #region [���� ����]

    [Client]
    public void GameOver() // Ŭ���̾�Ʈ�� ������ ����ٰ� �˸�
    {
        GameOver_Command();
    }

    [Command]
    private void GameOver_Command() // �������� Ŀ�ǵ�
    {
        GameOver_RPC();
    }

    [ClientRpc]
    private void GameOver_RPC() // ��� Ŭ���̾�Ʈ�鿡�� ����
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

        // ���� ����
        if (resultUI == null)
        {
            resultUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(5).gameObject;
        }

        // 1. ���� 1���ξִ� Win ������ lose UI ����
        if (GetComponent<RPCControl>().score == 2)
        {
            // Win ���
            resultUI.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            // Lose ���
            resultUI.transform.GetChild(1).gameObject.SetActive(true);
        }

        //Debug.Log($"{GetComponent<RPCControl>().userName}��  isGameOver : {isGameOver}");

        // 2. isGameStart �ʱ�ȭ
        GameManager.instance.isGameStart = false;

        // 3. TimerUI ��Ȱ��ȭ
        if (timerUI == null)
        {
            timerUI = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(2).gameObject;
        }
        timerUI.SetActive(false);

        // 4. StartButton Ȱ��ȭ
        if (startButton == null)
        {
            startButton = GameObject.FindGameObjectWithTag("Finish").transform.GetChild(4).gameObject;
        }
        startButton.SetActive(true);
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
