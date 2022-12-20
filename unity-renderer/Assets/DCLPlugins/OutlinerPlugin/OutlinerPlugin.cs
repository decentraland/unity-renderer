using DCL;
using UnityEngine;

public class OutlinerPlugin : IPlugin
{
    private OutlinerController controller;

    public OutlinerPlugin()
    {
        controller = new OutlinerController(DataStore.i.outliner, Resources.Load<OutlineRenderersSO>("OutlineRenderers"));
    }

    public void Dispose()
    {
        controller.Dispose();
    }
}
