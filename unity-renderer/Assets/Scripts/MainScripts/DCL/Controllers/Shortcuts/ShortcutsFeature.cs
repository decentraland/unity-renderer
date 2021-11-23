using UnityEngine.Assertions;
using UnityEngine.XR;

public class ShortcutsFeature : IPlugin
{
    internal ShortcutsController shortcutsController;

    public ShortcutsFeature ()
    {
        shortcutsController = new ShortcutsController();
    }

    public void Dispose() { shortcutsController?.Dispose(); }
}