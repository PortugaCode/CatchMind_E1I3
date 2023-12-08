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
    //private PlayerName pname;

    private int playerCount = 0;

    private bool isRealFinished = false;

    private void Start()
    {

    }

    private void Update()
    {
        if (GameManager.instance.isTimerOn)
        {
            UpdateTimerUI();
        }

        //ChangeNameByIndex();
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


    //Update에서 돌고 있는 메서드
    public void ChangeNameByIndex()
    {
        GameObject[] gamess = GameObject.FindGameObjectsWithTag("Player");

        #region
        /*for (int i = 0; i < gamess.Length; i++)
        {
            if (gamess[i].GetComponent<RPCControl>().index == -1 || gamess[i].GetComponent<RPCControl>().userName.Equals(string.Empty))
            {
                Debug.Log("인덱스 또는 이름이 아직 할당 안됐습니다. 리턴합니다");
                return;
            }
        }*/

        /*if (gamess.Length == playerCount || gamess.Length < GameManager.instance.control.Count)
        {
            Debug.Log($"인식된 플레이어 명 수 : {gamess.Length}입니다. 리턴합니다");
            return;
        }*/

        //GameObject[] games = GameObject.FindGameObjectsWithTag("Player");

        //Debug.Log($"통과함, 인식된 플레이어 수 : {gamess.Length}");
        #endregion

        int currentIndex = 0;

        int count = 0;

        while (currentIndex < gamess.Length)
        {
            count += 1;

            for (int i = 0; i < gamess.Length; i++)
            {
                if (gamess[i].GetComponent<RPCControl>().index == currentIndex)
                {
                    userNames_text[currentIndex].text = gamess[i].GetComponent<RPCControl>().userName;
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
    }
}
