using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI.InWorldCamera.Scripts
{
    [Serializable]
    public class ScreenshotMetadata
    {
        public string userName;
        public string userAddress;
        public string dateTime;
        public string realm;
        public Scene scene;
        public VisiblePeople[] visiblePeople;

        public static ScreenshotMetadata Create(DataStore_Player player, IAvatarsLODController avatarsLODController, Camera screenshotCamera)
        {
            var ownPlayer = player.ownPlayer.Get();
            var playerPosition = player.playerGridPosition.Get();

            var metadata = new ScreenshotMetadata
                {
                    userName = ownPlayer.name,
                    userAddress = ownPlayer.id,
                    dateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    realm = DataStore.i.realm.realmName.Get(),
                    scene = new Scene
                    {
                        name = MinimapMetadata.GetMetadata().GetSceneInfo(playerPosition.x, playerPosition.y).name,
                        location = new Location(playerPosition),
                    },
                    visiblePeople = GetVisiblePeoplesMetadata(visiblePlayers: CalculateVisiblePlayersInFrustrum(avatarsLODController, screenshotCamera)),
                };

            return metadata;
        }

        private static VisiblePeople[] GetVisiblePeoplesMetadata(List<Player> visiblePlayers)
        {
            var visiblePeople = new VisiblePeople[visiblePlayers.Count];
            var bridge = new UserProfileWebInterfaceBridge();

            for (var i = 0; i < visiblePlayers.Count; i++)
                visiblePeople[i] = new VisiblePeople
                {
                    userName = visiblePlayers[i].name,
                    userAddress = visiblePlayers[i].id,
                    wearables = bridge.Get(visiblePlayers[i].id).avatar.wearables.ToArray(),
                };

            return visiblePeople;
        }

        private static List<Player> CalculateVisiblePlayersInFrustrum(IAvatarsLODController avatarsLODController, Camera screenshotCamera)
        {
            List<Player> list = new List<Player>();

            foreach (IAvatarLODController lodController in avatarsLODController.LodControllers.Values)
                if (!lodController.IsInvisible)
                {
                    Plane[] planes = GeometryUtility.CalculateFrustumPlanes(screenshotCamera);
                    var player = lodController.player;

                    if (GeometryUtility.TestPlanesAABB(planes, player.collider.bounds))
                        list.Add(player);
                }

            return list;
        }
    }

    [Serializable]
    public class Scene
    {
        public string name;
        public Location location;
    }

    [Serializable]
    public class Location
    {
        public string x;
        public string y;

        public Location(Vector2Int position)
        {
            x = position.x.ToString();
            y = position.y.ToString();
        }
    }

    [Serializable]
    public class VisiblePeople
    {
        public string userName;
        public string userAddress;
        public string[] wearables;
    }
}
