using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Frame : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
    }

    public void SetFrameColor(bool isObstacle)
    {
        if(isObstacle)
            sr.color = Color.red;
        else
            sr.color = Color.cyan;
    }
}
