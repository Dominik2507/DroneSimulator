using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDotScript : MonoBehaviour
{
    public float sizeOnHover = 2.0f;
    public float initaialSize = 1.0f;
    public float scaleSpeed = 10.0f;

    private float t = 0;
    private bool isMouseOver = false;
    private void Update()
    {
        if(t > 0 && !isMouseOver)
        {
            t -= Time.deltaTime * scaleSpeed;
            t = Mathf.Clamp(t, 0, 1);
            gameObject.transform.localScale = Vector3.Lerp(Vector3.one * initaialSize, sizeOnHover * Vector3.one, t);
        }
    }

    private void OnMouseOver()
    {
        isMouseOver = true;
        t += Time.deltaTime * scaleSpeed;
        t = Mathf.Clamp(t, 0, 1);
        gameObject.transform.localScale = Vector3.Lerp(Vector3.one * initaialSize, sizeOnHover * Vector3.one, t);
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }
}
