using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorDiscTest : MonoBehaviour
{
    public Material mat;

    private void Start() { Invoke("ChangeColor", 10); }

    void ChangeColor()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetColor("_BaseColor", Color.black);
    }
}