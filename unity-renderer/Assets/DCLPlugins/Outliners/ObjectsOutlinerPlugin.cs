using DCL;
using UnityEngine;

public class ObjectsOutlinerPlugin : IPlugin
{
    private readonly ObjectsOutlinerController controller;

    public ObjectsOutlinerPlugin() { controller = new ObjectsOutlinerController(Resources.Load<OutlinerConfig>("OutlinerConfig"), DataStore.i.objectsOutliner); }

    public void Dispose() { controller?.Dispose(); }
}