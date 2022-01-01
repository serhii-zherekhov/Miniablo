using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextSystem : MonoBehaviour
{
    [SerializeField]
    private Text message;
    [SerializeField]
    private RectTransform pos;
    private float timer = 0;
    private const float maxTimer = 100f;


    private void Update()
    {
        ShowMessage();
    }

    private void ShowMessage()
    {
        pos.localPosition = new Vector3(0, 30 + (maxTimer - timer), 0);

        if (timer < 0)
        {
            message.text = "";
            pos.localPosition = new Vector3(0, 30, 0);
        }

        timer -= Time.deltaTime * 100;
    }

    public void AddMessage(string text)
    {
        message.text = text;
        pos.localPosition = new Vector3(0, 30, 0);
        timer = maxTimer;
    }
}
