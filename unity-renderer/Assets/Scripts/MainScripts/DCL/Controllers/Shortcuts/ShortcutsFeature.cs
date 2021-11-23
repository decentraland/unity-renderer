using UnityEngine.Assertions;
using UnityEngine.XR;

public class ShortcutsFeature : IPlugin
{
    internal ShortcutsController shortcutsController;

    public ShortcutsFeature ()
    {
        shortcutsController = new ShortcutsController();
    }

    public void Initialize()
    {
    }

    public void OnGUI()
    {
    }

    public void Update()
    {
    }

    public void LateUpdate()
    {
    }

    public void Dispose() { shortcutsController?.Dispose(); }
}