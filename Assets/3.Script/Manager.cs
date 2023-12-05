using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Manager : MonoBehaviour
{


    public List<GameObject> control = new List<GameObject>();
    // Start is called before the first frame update
    public RawImage img;

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
            }
        }
    }
    void Sync()
    {
        SetControll();
        foreach (GameObject g in control)
        {
            if (!g.GetComponent<NetworkIdentity>().isLocalPlayer)
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

}
