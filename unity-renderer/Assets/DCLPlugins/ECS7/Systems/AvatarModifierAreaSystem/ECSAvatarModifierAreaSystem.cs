using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ECSSystems.AvatarModifierAreaSystem
{
    public class ECSAvatarModifierAreaSystem
    {
        private IInternalECSComponent<InternalAvatarModifierArea> internalAvatarModifierArea;
        private readonly DataStore_Player dataStore;

        public ECSAvatarModifierAreaSystem(
            IInternalECSComponent<InternalAvatarModifierArea> internalAvatarModifierArea,
            DataStore_Player dataStore)
        {
            this.internalAvatarModifierArea = internalAvatarModifierArea;
            this.dataStore = dataStore;
        }

        public void Update()
        {
            var componentGroup = internalAvatarModifierArea.GetForAll();

            int entitiesCount = componentGroup.Count;
            for (int i = 0; i < entitiesCount; i++)
            {
                var scene = componentGroup[i].key1;
                var entityId = componentGroup[i].key2;
                var model = componentGroup[i].value.model;
                Transform entityTransform = scene.entities[entityId].gameObject.transform;

                HashSet<GameObject> currentAvatarsInArea = ECSAvatarUtils.DetectAvatars(model.area, entityTransform.position,
                    entityTransform.rotation, model.excludedIds.Count > 0 ? GetExcludedColliders(model.excludedIds) : null);

                if (model.removed)
                {
                    foreach (GameObject avatarGO in currentAvatarsInArea)
                    {
                        model.OnAvatarExit?.Invoke(avatarGO);
                    }

                    continue;
                }

                if (AreSetEquals(model.avatarsInArea, currentAvatarsInArea))
                    continue;

                // Apply modifier for avatars that just entered the area
                if (currentAvatarsInArea.Count > 0)
                {
                    foreach (GameObject avatarThatEntered in currentAvatarsInArea.Except(model.avatarsInArea))
                    {
                        model.OnAvatarEnter?.Invoke(avatarThatEntered);
                    }
                }

                // Reset modifier for avatars that just exited the area
                if (model.avatarsInArea?.Count > 0)
                {
                    foreach (GameObject avatarThatExited in model.avatarsInArea.Except(currentAvatarsInArea))
                    {
                        model.OnAvatarExit?.Invoke(avatarThatExited);
                    }
                }

                model.avatarsInArea = currentAvatarsInArea;
                internalAvatarModifierArea.PutFor(scene, entityId, model);
            }
        }

        private bool AreSetEquals(HashSet<GameObject> set1, HashSet<GameObject> set2)
        {
            if (set1 == null && set2 == null)
                return true;

            if (set1 == null || set2 == null)
                return false;

            return set1.SetEquals(set2);
        }

        private HashSet<Collider> GetExcludedColliders(HashSet<string> excludedIds)
        {
            var ownPlayer = dataStore.ownPlayer.Get();
            var otherPlayers = dataStore.otherPlayers;

            HashSet<Collider> result = new HashSet<Collider>();
            foreach (string excludedId in excludedIds)
            {
                string parsedExcludedId = excludedId.ToLower();
                if (otherPlayers.TryGetValue(parsedExcludedId, out Player player))
                    result.Add(player.collider);
                else if (ownPlayer != null && parsedExcludedId == ownPlayer.id)
                    result.Add(ownPlayer.collider);
            }

            return result;
        }
    }
}
