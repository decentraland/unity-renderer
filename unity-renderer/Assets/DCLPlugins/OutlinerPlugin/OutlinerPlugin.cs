using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using System.Threading;
using UnityEngine;

public class OutlinerPlugin : IPlugin
{
    private readonly CancellationTokenSource cts;
    private OutlinerController controller;

    public OutlinerPlugin(IAddressableResourceProvider resourceProvider)
    {
        cts = new CancellationTokenSource();

        CreateController(cts.Token).Forget();

        async UniTaskVoid CreateController(CancellationToken ct)
        {
            var outlineRenderersSo = await resourceProvider.GetAddressable<OutlineRenderersSO>("OutlineRenderers", ct);
            controller = new OutlinerController(DataStore.i.outliner, outlineRenderersSo);
        }
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
        controller.Dispose();
    }
}
