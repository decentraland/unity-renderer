using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxDomeManager : MonoBehaviour
{
    public float domeSize;
    public float sizeOffset;

    public GameObject domesParent;
    public Transform[] domes;


    private void Awake()
    {
        RebuildArray();
    }

    private void FixedUpdate()
    {
        RebuildArray();
    }

    void SetDomesSizes()
    {
        for(int i = 1; i < domes.Length; i++)
        {
            float domeScale = domeSize - (sizeOffset * i);
            domes[i].localScale = new Vector3(domeScale, domeScale, domeScale);
        }
    }


    void RebuildArray()
    {       
        domes = domesParent.GetComponentsInChildren<Transform>();
        SetDomesSizes();
    }
}
