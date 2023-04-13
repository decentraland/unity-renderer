using MainScripts.DCL.Controllers.HotScenes;
using System.Collections.Generic;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal partial class UsersMarkersColdAreaController
    {
        internal ColdUserMarkersStorage Storage_Test => storage;

        internal void RenewSceneInfos_Test(IReadOnlyList<IHotScenesController.HotSceneInfo> sceneInfos)
        {
            RenewSceneInfos(sceneInfos);
        }
    }
}
