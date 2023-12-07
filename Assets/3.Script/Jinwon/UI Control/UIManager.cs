using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private Image timer_bar;
    [SerializeField] private Text timer_text;

    [Header("Round")]
    [SerializeField] private Text round_text;

    [Header("Word")]
    [SerializeField] private Text word_text;

    [Header("Players")]
    [SerializeField] private Text[] userNames_text;
    [SerializeField] private GameObject[] userProfiles;
    [SerializeField] private RawImage img;
    private PlayerName pname;

    private void Start()
    {
        GameManager.instance.OnRoundChanged += UpdateRoundUI;
        GameManager.instance.OnRoundChanged += UpdateWordUI;
    }

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

    private void UpdateRoundUI()
    {
        round_text.text = $"{GameManager.instance.currentRound}";
    }

    private void UpdateWordUI()
    {
        // 권한이 있는 사람만 setactive true 해야 함.
        word_text.text = $"{GameManager.instance.currentWord}";
    }

    public void SyncTexture(Texture t)
    {
        img.texture = t;
    }

    public void Changename(int index, string str)
    {
        userNames_text[index].text = str;
        userProfiles[index].SetActive(true);
    }
}
