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
        private readonly HashSet<Collider> excludedColliders = new HashSet<Collider>();

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

                if (model.excludedIds != null && model.excludedIds.Count > 0)
                    UpdateExcludedCollidersCollection(model.excludedIds);

                HashSet<GameObject> currentAvatarsInArea = ECSAvatarUtils.DetectAvatars(model.area, entityTransform.position,
                    entityTransform.rotation, excludedColliders);

                if (model.removed)
                {
                    foreach (GameObject avatarGO in currentAvatarsInArea)
                    {
                        model.OnAvatarExit?.Invoke(avatarGO);
                    }

                    continue;
                }

                if (AreSetsEqual(model.avatarsInArea, currentAvatarsInArea))
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

        private bool AreSetsEqual(HashSet<GameObject> set1, HashSet<GameObject> set2)
        {
            if (set1 == null && set2 == null)
                return true;

            if (set1 == null || set2 == null)
                return false;

            if (set1.Count != set2.Count)
                return false;

            return set1.SetEquals(set2);
        }

        private void UpdateExcludedCollidersCollection(HashSet<string> excludedIds)
        {
            var ownPlayer = dataStore.ownPlayer.Get();
            excludedColliders.Clear();

            foreach (string excludedId in excludedIds)
            {
                if (dataStore.otherPlayers.TryGetValue(excludedId, out Player player))
                    excludedColliders.Add(player.collider);
                else if (ownPlayer != null && excludedId == ownPlayer.id)
                    excludedColliders.Add(ownPlayer.collider);
            }
        }
    }
}
