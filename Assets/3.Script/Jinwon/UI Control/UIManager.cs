using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image timer_bar;
    [SerializeField] private Text timer_text;

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
}
