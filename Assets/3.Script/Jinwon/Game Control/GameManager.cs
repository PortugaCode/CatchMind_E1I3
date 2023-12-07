using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Round Setting")]
    private float timer;
    public int roundCount = 5;
    public float roundTime = 30.0f;

    private void StartTimer()
    {
        if (timer == 0)
        {
            timer = roundTime;
            StartCoroutine(Timer_co());
        }
    }

    private IEnumerator Timer_co()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        timer = 0;
        yield break;
    }
}
