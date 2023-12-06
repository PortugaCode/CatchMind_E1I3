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
            for(int i=0;i< games.Length;i++)
            {
                if(i>=control.Count)
                {
                    control.Add(games[i]);
                    pname = (PlayerName)i;
                    Changename(i, pname.ToString());
                }
               if (games[i].Equals(control[i])) continue;

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
    void Sync()
    {
        SetControll();
        foreach (GameObject g in control)
        {
            if (!g.GetComponent<NetworkIdentity>().isLocalPlayer) //나중에 여기다가 누가 권한을 가지고 있는가 추가
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
