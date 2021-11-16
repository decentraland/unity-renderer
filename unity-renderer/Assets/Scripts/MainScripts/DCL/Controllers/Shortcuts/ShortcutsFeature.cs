using UnityEngine.Assertions;
using UnityEngine.XR;

public class ShortcutsFeature : PluginFeature
{
    internal ShortcutsController shortcutsController;

    public override void Initialize()
    {
        base.Initialize();
        Dispose();
        shortcutsController = new ShortcutsController();
    }

    public override void Dispose() { shortcutsController?.Dispose(); }
}