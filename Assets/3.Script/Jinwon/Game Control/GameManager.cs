using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Mirror;

public enum PlayerName
{
    ���߱� = 0,
    �ڼ���,
    ������,
    �̵���
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Round Setting")]
    public float timer;
    public int roundCount = 5;
    public float roundTime = 30.0f;
    public int roundWinScore = 3;

    [Header("Status")]
    public bool isTimerOn = false;
    public bool isCorrect = false; // ������ ������ ����°�

    [Header("Clients")]
    public List<GameObject> control = new List<GameObject>();

    [Header("UI")]
    [SerializeField] private GameObject uiManager;

    [SerializeField] GameObject drawable;

    [Header("Word")]
    public string currentWord;

    // Status
    public bool isGameStart = false;
    public int localIndex = -1;

    public event Action OnRoundChanged;

    [Header("UserInfo")]
    public List<string> userinfo = new List<string>();


    private void Start()
    {
        StartCoroutine(Update_co());
    }

    private void Update()
    {
        SetControll();
    }

    private IEnumerator Update_co()
    {
        while (true)
        {
            Sync();
            yield return null;
        }
    }

    private void Sync()
    {
        foreach (GameObject g in control)
        {
            if (g.GetComponent<CanDrawControl>().isCanDraw) //���߿� ����ٰ� ���� ������ ������ �ִ°� �߰�
            {
                uiManager.GetComponent<UIManager>().SyncTexture(g.GetComponent<RPCControl>().white);
            }
        }
    }

    private void SetControll()
    {
        if (control.Count > 0)
        {
            GameObject[] games = GameObject.FindGameObjectsWithTag("Player");

            //Debug.Log($"control.Count = {control.Count} , games.Length = {games.Length}");

            for (int i = 0; i < games.Length; i++)
            {
                if (control.Count < games.Length)
                {
                    if (i == games.Length - 1 && !games[i].GetComponent<RPCControl>().userName.Equals(string.Empty))
                    {
                        control.Add(games[i]);
                        //uiManager.GetComponent<UIManager>().Changename(i, control[i].GetComponent<RPCControl>().userName);
                        control[i].GetComponent<RPCControl>().IndexChanged(i);
                    }
                }
            }
        }
        else
        {
            GameObject[] games = GameObject.FindGameObjectsWithTag("Player");

            foreach (GameObject g in games)
            {
                if (!g.GetComponent<RPCControl>().userName.Equals(string.Empty))
                {
                    control.Add(g);
                    //uiManager.GetComponent<UIManager>().Changename(0, control[0].GetComponent<RPCControl>().userName);
                    control[0].GetComponent<RPCControl>().IndexChanged(0);
                }
            }
        }
    }

    public void PushStartButton()
    {
        GameObject[] games = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject g in games)
        {
            if (drawable == null)
            {
                drawable = GameObject.FindGameObjectWithTag("Respawn").transform.GetChild(0).gameObject;
            }

            drawable.GetComponent<Drawable>().Reset_W(true);

            g.GetComponent<RPCControl>().GameStart();
        }
    }

    public void WaitForNextRound()
    {
        StartCoroutine(WaitForNextRound_co());
    }

    private IEnumerator WaitForNextRound_co()
    {
        yield return new WaitForSeconds(3.0f);

        PushStartButton();
    }

    public void StartRound()
    {
        if (isCorrect == true) // isCorrect ���� �ʱ�ȭ
        {
            isCorrect = false;
        }

        OnRoundChanged?.Invoke();

        StartTimer();
    }

    public void ShowCurrentWord()
    {
        // �ش� ������ ���� ��ο��� ����, ������ ���� Ŭ���̾�Ʈ ���� ȿ��
    }

    private void StartTimer()
    {
        if (timer == 0)
        {
            isTimerOn = true;
            timer = roundTime;
            StartCoroutine(Timer_co());
        }
    }

    private IEnumerator Timer_co()
    {
        while (timer > 0)
        {
            if (isCorrect == true)
            {
                timer = 0;
                isTimerOn = false;
                yield break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        timer = 0;
        isTimerOn = false;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<CanDrawControl>().isCanDraw)
            {
                players[i].GetComponent<RPCControl>().TimeOver();
            }
        }

        yield break;
    }
}
