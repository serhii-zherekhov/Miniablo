using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonElement : MonoBehaviour
{
    private float transparencyTimer = 0;
    SpriteRenderer sr;
    private void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        //SetBrightness(0.5f);
        //SetBrightness(0f);
        SetBrightness(0.1f);
    }

    private void Update()
    {
        Transparent();
    }

    private void Transparent()
    {
        if (transparencyTimer > 0)
        {
            Color clr = sr.color;
            float coeff = (1 - transparencyTimer) * 0.8f + 0.2f;

            clr.a = coeff - coeff % 0.2f;
            sr.color = clr;
            transparencyTimer -= Time.deltaTime;
        }
    }

    public void MakeTransparent()
    {
        transparencyTimer = 1;
    }

    public void SetBrightness(float brightness)
    {
        Color clr = sr.color;
        clr.r = brightness;
        clr.g = brightness;
        clr.b = brightness;
        sr.color = clr;
    }
}
