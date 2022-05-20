using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChargeWheel : MonoBehaviour
{
    Material _material;
    Image _img;
    public float speed = 1;

    public Color[] colors;
    int colorIndex = 0;


    float head;
    float tail;
    
    void Start()
    {
        _img = GetComponent<Image>();

        if (_img.maskable)
        {
            _material = _img.materialForRendering;
        }
        else
        {
            _material = new Material(_img.material);
            _img.material = _material;
        }

        _material.SetColor("_color01", colors[colorIndex]);

        head = Random.Range(0, 0.15f);
    }

    
    void Update()
    {
        float tempSpeedH = Mathf.Abs(0.5f + head) * speed;
        float tempSpeedT = Mathf.Abs(0.5f + tail) * speed;

        if (head < 1)
        {
            head += tempSpeedH * Time.deltaTime;
            head = Mathf.Clamp01(head);
            _material.SetFloat("_fillHead", head);
        }
        else if(tail < 1)
        {
            tail += tempSpeedT * Time.deltaTime;
            tail = Mathf.Clamp01(tail);
            _material.SetFloat("_fillTail", tail);
        }
        else
        {
            head = 0;
            tail = 0;

            _material.SetFloat("_fillHead", head);
            _material.SetFloat("_fillTail", tail);

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

        _material.SetColor("_color01", colors[colorIndex]);
    }
}
