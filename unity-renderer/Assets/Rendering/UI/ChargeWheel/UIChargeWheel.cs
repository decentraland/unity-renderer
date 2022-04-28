using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChargeWheel : MonoBehaviour
{
    Material material;

    public float speed = 1;

    public Color[] colors;
    int colorIndex = 0;


    float head;
    float tail;
    
    void Start()
    {
        Image img = GetComponent<Image>();
        material = new Material(img.material);
        img.material = material;
        material.SetColor("_color01", colors[colorIndex]);
    }

    
    void Update()
    {
        float tempSpeedH = Mathf.Abs(0.5f + head) * speed;
        float tempSpeedT = Mathf.Abs(0.5f + tail) * speed;

        if (head < 1)
        {
            head += tempSpeedH * Time.deltaTime;
            head = Mathf.Clamp01(head);
            material.SetFloat("_fillHead", head);
        }
        else if(tail < 1)
        {
            tail += tempSpeedT * Time.deltaTime;
            tail = Mathf.Clamp01(tail);
            material.SetFloat("_fillTail", tail);
        }
        else
        {
            head = 0;
            tail = 0;

            material.SetFloat("_fillHead", head);
            material.SetFloat("_fillTail", tail);

            ColorChange();
        }
    }

    void ColorChange()
    {
        if(colorIndex < colors.Length - 1)
        {
            colorIndex++;
        }
        else
        {
            colorIndex = 0;
        }

        material.SetColor("_color01", colors[colorIndex]);
    }
}
