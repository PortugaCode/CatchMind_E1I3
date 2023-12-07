using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseDrawing : MonoBehaviour
{
    public Color pencolor = Color.red;
    [SerializeField]
    int penWidth = 3;

    [SerializeField]
    bool erase = false;

    Vector2 defaultTexCoord = Vector2.zero;
    Vector2 previousTexCoord;

    Drawing drawObject;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Draw_co());
    }

    IEnumerator Update_co(bool mouseDown)
    {

        if (mouseDown)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                if (hitInfo.collider != null && hitInfo.collider is MeshCollider)
                {
                    drawObject = hitInfo.transform.GetComponent<Drawing>();
                    if (drawObject != null)
                    {
                        Vector2 currentTexCoord = hitInfo.textureCoord;
                        if (erase)
                        {
                            drawObject.Erase(currentTexCoord, previousTexCoord, penWidth);
                        }
                        else
                        {
                            drawObject.Draw(currentTexCoord, previousTexCoord, penWidth, pencolor);
                        }
                        previousTexCoord = currentTexCoord;

                    }
                }
                else
                {
                    Debug.LogWarning("If you want to draw using a RaycastHit, need set MeshCollider for object.");
                }
            }
            else
            {
                previousTexCoord = defaultTexCoord;
            }
        }
        else if (!mouseDown) // Mouse is released
        {
            previousTexCoord = defaultTexCoord;

        }

        yield return new WaitForSeconds(Random.Range(0, 0.1f));
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.E))
        {
            erase = true;
        }
        else
        {
            erase = false;
        }
        StartCoroutine(Update_co(Input.GetMouseButton(0)));

    }

    private IEnumerator Draw_co()
    {
        WaitForSeconds wfs = new WaitForSeconds(1.0f);

        while (true)
        {
            if (drawObject != null)
            {
                //GetComponent<RPCControl>().Draw_2(drawObject.drawableTexture);
            }

            yield return wfs;
        }
    }
}
