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

    //������ ����====================
    [SyncVar(hook = nameof(onNameChanged))]
    public string userName = string.Empty;
    [SyncVar(hook = nameof(onIndexChanged))]
    public int index = -1;
    [SyncVar(hook = nameof(onScoreChanged))]
    public int score = 0;
    //==============================


    //Client�� Server�� Connect �Ǿ��� �� Callback�Լ�
    public override void OnStartAuthority()
    {
        NameChange(SQL_Manager.instance.info.userName);
    }

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









    #region [�ؽ��� ����]
    private void onTextureChanged(Texture2D _old, Texture2D _new)
    {
        white = _new;
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
        
        if (pen.GetComponent<RPCControl>().score == 3)
        {
            // ���� ����
        }

        // 6. ���� ���� ����
        GameManager.instance.PushStartButton();
    }

    #endregion

    public override void OnStartClient()
    {
        //base.OnStartClient();
        StartCoroutine(wait());
    }
/*
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        StartCoroutine(wait());
    }*/

    private IEnumerator wait()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject canvas = GameObject.FindGameObjectWithTag("Finish");
        canvas.GetComponent<UIManager>().ChangeNameByIndex();

        yield break;
    }
}
