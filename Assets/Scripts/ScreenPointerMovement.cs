using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenPointerMovement : MonoBehaviour
{
    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        transform.position = worldPos;
    }
}
