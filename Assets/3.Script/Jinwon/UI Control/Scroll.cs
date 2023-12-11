using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroll : MonoBehaviour
{
    public float speed = 5.0f;

    private RectTransform rect;

    private void Awake()
    {
        TryGetComponent(out rect);
    }

    private void Update()
    {
        rect.position -= new Vector3(5, 0, 0) * speed * Time.deltaTime;
    }
}
