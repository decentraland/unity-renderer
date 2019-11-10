using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotate : MonoBehaviour
{
    public RectTransform rectTransform;

    public Vector3 angularVelocity;

    private void Update()
    {
        rectTransform.Rotate(angularVelocity * Time.deltaTime);
    }
}
