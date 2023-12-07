using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public enum PlayerName
{
    오중근 = 0,
    박수진,
    김진원,
    이동길
}


public class Manager : MonoBehaviour
{
    public Text[] Player;
    public GameObject[] pro;


    public List<GameObject> control = new List<GameObject>();
    // Start is called before the first frame update
    public RawImage img;
    private PlayerName pname;



    private void Start()
    {
        StartCoroutine(Update_co());
    }

    void SetControll()
    {
        if(control.Count>0)
        {
            GameObject[] games = GameObject.FindGameObjectsWithTag("Player");
            for(int i=0;i < games.Length;i++)
            {
                if(Player[i].text == "User_Name")
                {
                    pname = (PlayerName)i;
                    Changename(i, pname.ToString());
                }
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
            foreach(GameObject g in games)
            {
                control.Add(g);
                pname = (PlayerName)0;
                Changename(0, pname.ToString());
            }
        }
    }

    // !g.GetComponent<NetworkIdentity>().isLocalPlayer == 내가 아니라면
    // g가 그릴 권한이 있는 사람이라면

    void Sync()
    {
        SetControll();
        foreach (GameObject g in control)
        {
            if (g.GetComponent<CanDrawControl>().isCanDraw) //나중에 여기다가 누가 권한을 가지고 있는가 추가
            {
                img.texture = g.GetComponent<RPCControl>().white;
            }
        }
    }
    IEnumerator Update_co()
    {
        while(true)
        {
            Sync();
            yield return null;
        }
    }


    private void Changename(int index, string str)
    {
        Debug.Log("Changename!!");
        Player[index].text = str;
        pro[index].SetActive(true);
    }

}
