using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyPosition : MonoBehaviour
{
    public RectTransform target;
    public RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    void Update()
    {
        if (target != null)
        {
            Vector2 difSize = (rectTransform.parent as RectTransform).sizeDelta - target.sizeDelta;
            rectTransform.anchoredPosition = target.anchoredPosition - difSize;
        }
    }
}
