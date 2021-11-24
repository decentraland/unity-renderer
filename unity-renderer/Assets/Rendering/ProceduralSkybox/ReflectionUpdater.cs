using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionUpdater : MonoBehaviour
{
    ReflectionProbe _refProbe;

    float currentTime;
    public float refreshTime;
    void Start()
    {
        _refProbe = GetComponent<ReflectionProbe>();
        currentTime = refreshTime;
    }

    
    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        else
        {
            _refProbe.RenderProbe();
            currentTime = refreshTime;
        }

    }
}
