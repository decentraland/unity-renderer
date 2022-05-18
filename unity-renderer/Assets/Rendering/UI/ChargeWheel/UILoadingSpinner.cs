using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingSpinner : MonoBehaviour
{
    Material _material;
    public Image img;

    public Color color;
    public float head;
    public float tail;

    void Start()
    {
        GetMaterial();
    }

    void GetMaterial()
    {
        if (img.maskable)
        {
            _material = img.materialForRendering;
        }
        else
        {
            _material = new Material(img.material);
            img.material = _material;
        }
    }

    void Update()
    {
        SetValues();
    }


    public void SetValues()
    {
        if(_material)
        {
            _material.SetColor("_color01", color);
            _material.SetFloat("_fillHead", head);
            _material.SetFloat("_fillTail", tail);
        }
        else
        {
            GetMaterial();
            SetValues();
        }
        
    }
}
