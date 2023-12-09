using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorControl : MonoBehaviour
{
    public void ChangePenColor()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.GetComponent<Mouse_Drawer>().pencolor = GetComponent<Image>().color;

            /*if (player.GetComponent<CanDrawControl>().isCanDraw)
            {
                player.GetComponent<RPCControl>().ChangeColor(GetComponent<Image>().color);
            }*/
        }
    }
}
