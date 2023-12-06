using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDraw : MonoBehaviour
{
    private LineRenderer line;
    public GameObject linePrefabs;
    List<Vector2> points = new List<Vector2>();

    [SerializeField] private float minDistance = 0.1f;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject go = Instantiate(linePrefabs);
            line = go.GetComponent<LineRenderer>();
            points.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            line.positionCount = 1;
            line.SetPosition(0, points[0]);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 currentPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(points[points.Count - 1], currentPosition) > minDistance)
            {
                points.Add(currentPosition);
                line.positionCount++;
                line.SetPosition(line.positionCount - 1, currentPosition);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            points.Clear();
        }

    }
}
