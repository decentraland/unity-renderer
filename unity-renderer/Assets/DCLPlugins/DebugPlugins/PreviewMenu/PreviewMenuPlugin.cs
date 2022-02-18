using DCL;

public class PreviewMenuPlugin : IPlugin
{
    private readonly IBaseVariable<bool> isPreviewMenuActive;

    internal PreviewMenuController menuController;

    public PreviewMenuPlugin() : this(DataStore.i.debugConfig.isPreviewMenuActive) { }

    internal PreviewMenuPlugin(IBaseVariable<bool> isPreviewMenuActive)
    {
        this.isPreviewMenuActive = isPreviewMenuActive;
        isPreviewMenuActive.OnChange += OnIsPreviewMenuActiveChange;

        OnIsPreviewMenuActiveChange(isPreviewMenuActive.Get(), false);
    }

    public void Dispose()
    {
        isPreviewMenuActive.OnChange -= OnIsPreviewMenuActiveChange;
        DisposeView();
    }

    private void InitializeView()
    {
        menuController ??= new PreviewMenuController();
    }

    private void DisposeView()
    {
        menuController?.Dispose();
        menuController = null;
    }

    private void OnIsPreviewMenuActiveChange(bool current, bool previous)
    {
        if (current)
        {
            InitializeView();
        }
        else
        {
            DisposeView();
        }
    }
}