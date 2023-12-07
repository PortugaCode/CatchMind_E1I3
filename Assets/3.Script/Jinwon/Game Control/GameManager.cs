using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public event Action OnRoundChanged;

    private void Start()
    {
        StartRound();
    }

    private void StartRound()
    {
        if (isCorrect == true) // isCorrect 변수 초기화
        {
            isCorrect = false;
        }

        currentRound += 1;

        // 그림을 그릴 사람 선정, 권한 부여

        // 제시어 뽑기
        currentWord = wordManager.GetComponent<WordManager>().GetRandomWord();

        // 그림을 그릴 사람(권한이 있는사람)에게만 제시어 보여주기

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
