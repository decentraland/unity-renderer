using DCL;
using UnityEngine;

public class GotoPanelHUDController : IHUD
{

    internal GotoPanelHUDView view { get; private set; }

    public GotoPanelHUDController()
    {
        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("GotoPanelHUD")).GetComponent<GotoPanelHUDView>();
        view.name = "_GotoPanelHUD";
        view.container.SetActive(false);
        DataStore.i.HUDs.gotoPanelVisible.OnChange += ChangeVisibility;
        DataStore.i.HUDs.gotoPanelCoordinates.OnChange += SetCoordinates;
    }

    private void ChangeVisibility(bool current, bool previous)
    {
        if (current == previous)
            return;

        SetVisibility(current);
    }

    private void SetCoordinates(ParcelCoordinates current, ParcelCoordinates previous) {
        if (current == previous)
            return;

        view.SetPanelInfo(current);
    }

    public void Dispose()
    {
        if (view)
        {
            UnityEngine.Object.Destroy(view.gameObject);
        }
    }

    public void SetVisibility(bool visible)
    {
        view.SetVisible(visible);
    }

}
