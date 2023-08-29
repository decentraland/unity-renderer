using DCL;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    [Serializable]
    public class ScreenshotMetadata
    {
        public string userName;
        public string userAddress;
        public string dateTime;
        public string realm;
        public Scene scene;
        public VisiblePerson[] visiblePeople;

        private static bool isWorld => DataStore.i.common.isWorld.Get();
        private static string realmName => DataStore.i.realm.realmName.Get();

        public static ScreenshotMetadata Create(DataStore_Player player, IAvatarsLODController avatarsLODController, Camera screenshotCamera)
        {
            Player ownPlayer = player.ownPlayer.Get();
            Vector2Int playerPosition = player.playerGridPosition.Get();

            var metadata = new ScreenshotMetadata
            {
                userName = UserProfileController.userProfilesCatalog.Get(ownPlayer.id).userName,
                userAddress = ownPlayer.id,
                dateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                realm = realmName,
                scene = new Scene
                {
                    name = isWorld? $"World {realmName}" : MinimapMetadata.GetMetadata().GetSceneInfo(playerPosition.x, playerPosition.y).name,
                    location = new Location(playerPosition),
                },
                visiblePeople = GetVisiblePeoplesMetadata(
                    visiblePlayers: CalculateVisiblePlayersInFrustum(ownPlayer, avatarsLODController, screenshotCamera)),
            };

            return metadata;
        }

        private static VisiblePerson[] GetVisiblePeoplesMetadata(List<Player> visiblePlayers)
        {
            var visiblePeople = new VisiblePerson[visiblePlayers.Count];
            UserProfileDictionary userProfilesCatalog = UserProfileController.userProfilesCatalog;

            for (var i = 0; i < visiblePlayers.Count; i++)
            {
                UserProfile profile = userProfilesCatalog.Get(visiblePlayers[i].id);

                visiblePeople[i] = new VisiblePerson
                {
                    userName = profile.userName,
                    userAddress = profile.userId,
                    isGuest = profile.isGuest,

                    wearables = FilterNonBaseWearables(profile.avatar.wearables),
                };
            }

            return visiblePeople;
        }

        private static string[] FilterNonBaseWearables(List<string> avatarWearables) =>
            avatarWearables.Where(wearable => !wearable.StartsWith(IWearablesCatalogService.BASE_WEARABLES_COLLECTION_ID)).ToArray();

        private static List<Player> CalculateVisiblePlayersInFrustum(Player player, IAvatarsLODController avatarsLODController, Camera screenshotCamera)
        {
            var list = new List<Player>();
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(screenshotCamera);

            foreach (IAvatarLODController lodController in avatarsLODController.LodControllers.Values)
                if (!lodController.IsInvisible && GeometryUtility.TestPlanesAABB(frustumPlanes, lodController.player.collider.bounds))
                    list.Add(lodController.player);

            if (GeometryUtility.TestPlanesAABB(frustumPlanes, player.collider.bounds))
                list.Add(player);

            return list;
        }

        public DateTime GetLocalizedDateTime()
        {
            if (!long.TryParse(dateTime, out long unixTimestamp)) return new DateTime();
            return DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).ToLocalTime().DateTime;
        }

        public DateTime GetStartOfTheMonthDate()
        {
            DateTime localizedDateTime = GetLocalizedDateTime();
            return new DateTime(localizedDateTime.Year, localizedDateTime.Month, 1);
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
    public class VisiblePerson
    {
        public string userName;
        public string userAddress;
        public bool isGuest;

        public string[] wearables;
    }
}
