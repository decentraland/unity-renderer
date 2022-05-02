using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingSpinner : MonoBehaviour
{
    Material _material;
    Image _img;

    public Color color;
    public float head;
    public float tail;

    void Start()
    {
        GetMaterial();
    }

    void GetMaterial()
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
