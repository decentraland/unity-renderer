using System;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;

public class ObjectsOutlinerController : IDisposable
{
    private readonly OutlinerConfig outlinerConfig;
    private readonly DataStore_ObjectsOutliner dataStore;

    public ObjectsOutlinerController(OutlinerConfig outlinerConfig, DataStore_ObjectsOutliner dataStore)
    {
        
    }

    private void OnSet(IEnumerable<Renderer> renderers)
    {
    }

    public void Dispose()
    {
        
    }
}