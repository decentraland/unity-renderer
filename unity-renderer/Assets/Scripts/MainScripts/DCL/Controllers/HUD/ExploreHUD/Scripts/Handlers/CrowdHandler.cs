using System;

internal class CrowdHandler : ICrowdDataView
{
    public HotScenesController.HotSceneInfo info { private set; get; }
    public event Action<HotScenesController.HotSceneInfo> onInfoUpdate;

    public void SetCrowdInfo(HotScenesController.HotSceneInfo info)
    {
        this.info = info;
        onInfoUpdate?.Invoke(info);
    }
}
