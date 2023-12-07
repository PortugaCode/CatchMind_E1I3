using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image timer_bar;
    [SerializeField] private Text timer_text;
    [SerializeField] private Text round_text;
    [SerializeField] private Text word_text;

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
}
