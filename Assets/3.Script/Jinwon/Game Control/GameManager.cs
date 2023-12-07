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

    [Header("Status")]
    public bool isTimerOn = false;
    public int currentRound = 0;
    private bool isCorrect = false; // ������ ������ ����°�

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

            for (int i = 0; i < games.Length; i++)
            {
                uiManager.GetComponent<UIManager>().Changename(i, $"�ӽ�{i}");

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
                uiManager.GetComponent<UIManager>().Changename(0, "�ӽ�0");
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

        if (isCorrect == true) // isCorrect ���� �ʱ�ȭ
        {
            isCorrect = false;
        }

        currentRound += 1;

        // ���þ� �̱�
        currentWord = wordManager.GetComponent<WordManager>().GetRandomWord();

        OnRoundChanged?.Invoke();

        StartTimer();
    }

    private IEnumerator EndRound_co()
    {
        // �ش� ������ ���� ��ο��� ����

        yield return new WaitForSeconds(3.0f);

        if (currentRound == roundCount - 1) // ������ ���� ���� ���
        {
            // ���� ����, ������ ���
        }
        else
        {
            // ���� ���� ����
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
