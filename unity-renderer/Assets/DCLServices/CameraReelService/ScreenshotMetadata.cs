﻿using DCL;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public VisiblePlayers[] visiblePeople;

        public static ScreenshotMetadata Create(DataStore_Player player, IAvatarsLODController avatarsLODController, Camera screenshotCamera)
        {
            Player ownPlayer = player.ownPlayer.Get();
            Vector2Int playerPosition = player.playerGridPosition.Get();

            var metadata = new ScreenshotMetadata
            {
                userName = UserProfileController.userProfilesCatalog.Get(ownPlayer.id).userName,
                userAddress = ownPlayer.id,
                dateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                realm = DataStore.i.realm.realmName.Get(),
                scene = new Scene
                {
                    name = MinimapMetadata.GetMetadata().GetSceneInfo(playerPosition.x, playerPosition.y).name,
                    location = new Location(playerPosition),
                },
                visiblePeople = GetVisiblePeoplesMetadata(
                    visiblePlayers: CalculateVisiblePlayersInFrustum(ownPlayer, avatarsLODController, screenshotCamera)),
            };

            return metadata;
        }

        private static VisiblePlayers[] GetVisiblePeoplesMetadata(List<Player> visiblePlayers)
        {
            var visiblePeople = new VisiblePlayers[visiblePlayers.Count];
            UserProfileDictionary userProfilesCatalog = UserProfileController.userProfilesCatalog;

            UserProfile profile;

            for (var i = 0; i < visiblePlayers.Count; i++)
            {
                profile = userProfilesCatalog.Get(visiblePlayers[i].id);

                visiblePeople[i] = new VisiblePlayers
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
    public class VisiblePlayers
    {
        public string userName;
        public string userAddress;
        public bool isGuest;

        public string[] wearables;
    }
}
