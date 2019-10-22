using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeScale : MonoBehaviour
{
    public FloatVariable scaleFactor;

    public float minScale;

    public float maxScale;

    void Start()
    {
        UpdateScale(scaleFactor.Get());
        scaleFactor.OnChange += FactorUpdated;
    }

    private void FactorUpdated(float current, float previous)
    {
        UpdateScale(current);
    }

    private void UpdateScale(float relative)
    {
        transform.localScale = Vector3.one * (Mathf.Lerp(minScale, maxScale, relative));
    }
}