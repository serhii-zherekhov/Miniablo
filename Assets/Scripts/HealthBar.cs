using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private RectTransform rt;
    private float maxHeight = 0;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        maxHeight = rt.rect.height;
    }

    public void SetHeight(float percent)
    {
        rt.sizeDelta = new Vector2(rt.rect.width, maxHeight * percent);
    }
}
