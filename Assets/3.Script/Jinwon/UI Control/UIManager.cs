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
    [SerializeField] private RawImage img;
    private PlayerName pname;

    private void Start()
    {

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
