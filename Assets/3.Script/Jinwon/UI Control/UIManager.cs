using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private Image timer_bar;
    [SerializeField] private Text timer_text;

    [Header("Players")]
    [SerializeField] private Text[] userNames_text;
    [SerializeField] private GameObject[] userProfiles;
    [SerializeField] private GameObject[] crownIcons;
    [SerializeField] private RawImage img;

    [Header("Score")]
    [SerializeField] private Text[] userScore;

    private void Update()
    {
        if (GameManager.instance.isTimerOn)
        {
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        timer_text.text = $"{(int)GameManager.instance.timer}";
        timer_bar.fillAmount = GameManager.instance.timer / GameManager.instance.roundTime;
    }

    public void SyncTexture(Texture t)
    {
        img.texture = t;
    }

    public void CrownIcon()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<CanDrawControl>().isCanDraw)
            {
                crownIcons[i].SetActive(true);
            }
            else
            {
                crownIcons[i].SetActive(false);
            }
        }
    }

    public void ChangeScore(int index, int score )
    {
        userScore[index].text = $"Score : {score}";
    }

    public void ChangeName(int index, string str)
    {
        userNames_text[index].text = str;
        userProfiles[index].SetActive(true);
    }

    public void ChangeNameByIndex()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        int currentIndex = 0;

        int count = 0;

        //Debug.Log($"isLocal : {isLocalPlayer}, local index : {GameManager.instance.localIndex}, rpc.index = {rpc.index}");

        GameObject p = null;

        while (currentIndex < players.Length)
        {
            count += 1;

            for (int i = 0; i < players.Length; i++)
            {
                if (i == GameManager.instance.localIndex)
                {
                    p = players[i];
                }

                if (players[i].GetComponent<RPCControl>().index == currentIndex)
                {
                    userNames_text[currentIndex].text = players[i].GetComponent<RPCControl>().userName;
                    userProfiles[currentIndex].SetActive(true);
                    currentIndex += 1;
                    break;
                }
            }

            if (count > 100)
            {
                return;
            }
        }

        if (p != null)
        {
            ChangeName(GameManager.instance.localIndex, $"{p.GetComponent<RPCControl>().userName} (Me)");
        }
    }
}
