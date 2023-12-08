using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Mirror;

public enum PlayerName
{
    오중근 = 0,
    박수진,
    김진원,
    이동길
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

    [Header("Status")]
    public bool isTimerOn = false;
    public bool isCorrect = false; // 누군가 정답을 맞췄는가

    [Header("Clients")]
    public List<GameObject> control = new List<GameObject>();

    [Header("UI")]
    [SerializeField] private GameObject uiManager;

    [Header("Word")]
    public string currentWord;

    // Status
    public bool isGameStart = false;

    public event Action OnRoundChanged;

    [Header("UserInfo")]
    public List<string> userinfo = new List<string>();


    private void Start()
    {
        StartCoroutine(Update_co());
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
        SetControll();

        foreach (GameObject g in control)
        {
            if (g.GetComponent<CanDrawControl>().isCanDraw) //나중에 여기다가 누가 권한을 가지고 있는가 추가
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
            g.GetComponent<RPCControl>().GameStart();
        }
    }

    public void StartRound()
    {
        if (isCorrect == true) // isCorrect 변수 초기화
        {
            isCorrect = false;
        }

        OnRoundChanged?.Invoke();

        StartTimer();
    }

    public void ShowCurrentWord()
    {
        // 해당 라운드의 정답 모두에게 공개, 정답을 맞힌 클라이언트 강조 효과
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
        yield break;
    }
}
