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
    public int roundWinScore = 3;

    [Header("Status")]
    public bool isTimerOn = false;
    public bool isCorrect = false; // 누군가 정답을 맞췄는가

    [Header("Clients")]
    public List<GameObject> control = new List<GameObject>();

    [Header("UI")]
    [SerializeField] private GameObject uiManager;

    [SerializeField] GameObject drawable;

    [Header("Word")]
    public string currentWord;

    private GameObject[] games;

    // Status
    public bool isGameStart = false;
    public int localIndex = -1;
    private int currentIndex = 0;

    public event Action OnRoundChanged;

    private CanDrawControl temp;

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
        if (control == null)
        {
            return;
        }

        if (control.Count > 0)
        {
            foreach (GameObject g in control)
            {
                /*if (!g.TryGetComponent(out temp))
                {
                    return;
                }*/

                if (g == null)
                {
                    return;
                }

                if (g.GetComponent<CanDrawControl>() == null)
                {
                    return;
                }

                if (g.GetComponent<CanDrawControl>().isCanDraw) //나중에 여기다가 누가 권한을 가지고 있는가 추가
                {
                    uiManager.GetComponent<UIManager>().SyncTexture(g.GetComponent<RPCControl>().white);
                }
            }
        }

        
    }

    private void SetControll()
    {
        games = GameObject.FindGameObjectsWithTag("Player");

        if (control.Count == 0)
        {
            if (games.Length == 0)
            {
                return;
            }

            control.Add(games[currentIndex]);
            control[currentIndex].GetComponent<RPCControl>().IndexChanged(currentIndex);
            currentIndex += 1;
        }
        else
        {
            if (games.Length <= currentIndex)
            {
                return;
            }

            if (control.Count < games.Length)
            {
                control.Add(games[currentIndex]);
                control[currentIndex].GetComponent<RPCControl>().IndexChanged(currentIndex);
                currentIndex += 1;
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
