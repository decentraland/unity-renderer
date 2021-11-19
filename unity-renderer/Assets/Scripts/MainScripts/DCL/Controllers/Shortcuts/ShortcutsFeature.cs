using UnityEngine.Assertions;
using UnityEngine.XR;

public class ShortcutsFeature : IPlugin
{
    internal ShortcutsController shortcutsController;

    public bool enabled { get; private set;  } = false;

    public void Initialize()
    {
        enabled = true;
        shortcutsController = new ShortcutsController();
    }

    public void Disable()
    {
        enabled = false;
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