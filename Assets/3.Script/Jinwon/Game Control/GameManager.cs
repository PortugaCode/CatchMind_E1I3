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
    public int currentRound = 0;
    private bool isCorrect = false; // 누군가 정답을 맞췄는가

    [Header("Word Manager")]
    [SerializeField] private GameObject wordManager;
    public string currentWord;

    [Header("Clients")]
    public List<GameObject> control = new List<GameObject>();

    [Header("UI")]
    [SerializeField] private GameObject uiManager;

    public event Action OnRoundChanged;

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
                uiManager.GetComponent<UIManager>().Changename(i, $"임시{i}");

                if (control.Count < games.Length)
                {
                    if (i == games.Length - 1)
                    {
                        control.Add(games[i]);
                    }
                }
            }
        }
        else
        {
            GameObject[] games = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject g in games)
            {
                control.Add(g);
                uiManager.GetComponent<UIManager>().Changename(0, "임시0");
            }
        }
    }

    public void StartRound()
    {
        GameObject[] games = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject g in games)
        {
            g.GetComponent<RPCControl>().TurnOff();
        }

        if (isCorrect == true) // isCorrect 변수 초기화
        {
            isCorrect = false;
        }

        currentRound += 1;

        // 제시어 뽑기
        currentWord = wordManager.GetComponent<WordManager>().GetRandomWord();

        OnRoundChanged?.Invoke();

        StartTimer();
    }

    private IEnumerator EndRound_co()
    {
        // 해당 라운드의 정답 모두에게 공개

        yield return new WaitForSeconds(3.0f);

        if (currentRound == roundCount - 1) // 마지막 라운드 였던 경우
        {
            // 게임 종료, 점수판 출력
        }
        else
        {
            // 다음 라운드 시작
        }

        yield break;
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
                StartCoroutine(EndRound_co());
                yield break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        timer = 0;
        isTimerOn = false;
        StartCoroutine(EndRound_co());
        yield break;
    }
}
