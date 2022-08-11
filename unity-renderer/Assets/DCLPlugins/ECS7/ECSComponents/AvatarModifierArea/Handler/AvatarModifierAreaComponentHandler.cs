using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.ECSComponents.Utils;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class AvatarModifierAreaComponentHandler : IECSComponentHandler<PBAvatarModifierArea>
    {
        private event Action<GameObject> OnAvatarEnter;
        private event Action<GameObject> OnAvatarExit;
        
        internal HashSet<GameObject> avatarsInArea = new HashSet<GameObject>();
        internal HashSet<Collider> excludedColliders;
        
        internal PBAvatarModifierArea model;
        private IDCLEntity entity;
        private UnityEngine.Vector3 boxArea;
        
        private readonly AvatarModifierFactory factory;
        private readonly IUpdateEventHandler updateEventHandler;
        private readonly DataStore_Player dataStore;
        
        public AvatarModifierAreaComponentHandler(IUpdateEventHandler updateEventHandler,DataStore_Player dataStore, AvatarModifierFactory factory)
        {
            this.factory = factory;
            this.dataStore = dataStore;
            this.updateEventHandler = updateEventHandler;
            
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            dataStore.otherPlayers.OnRemoved += OnOtherPlayersRemoved;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            // We detect current affected avatars
            var toRemove = avatarsInArea;
            var currentInArea = DetectAllAvatarsInArea();
            if (currentInArea != null)
                toRemove.UnionWith(currentInArea);

            // Remove the modifiers on all avatars
            RemoveAllModifiers(toRemove);
            if(avatarsInArea != null)
                avatarsInArea.Clear();
            
            // We unsubscribe from events
            dataStore.ownPlayer.OnChange -= OwnPlayerOnOnChange;
            dataStore.otherPlayers.OnAdded -= OtherPlayersOnOnAdded;
            dataStore.otherPlayers.OnRemoved -= OnOtherPlayersRemoved;

            updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAvatarModifierArea model)
        {
            if (model.Equals(this.model))
                return;
            
            // Clean up
            RemoveAllModifiers();
            OnAvatarEnter = null;
            OnAvatarExit = null;

            this.entity = entity;
            ApplyCurrentModel(model);
        }

        private void Update()
        {
            if (model == null)
                return;

            // Find avatars currently on the area
            HashSet<GameObject> newAvatarsInArea = DetectAllAvatarsInArea();
            if (AreSetEquals(avatarsInArea, newAvatarsInArea))
                return;

            // Call event for avatars that just entered the area
            foreach (GameObject avatarThatEntered in newAvatarsInArea.Except(avatarsInArea))
            {
                OnAvatarEnter?.Invoke(avatarThatEntered);
            }

            // Call events for avatars that just exited the area
            foreach (GameObject avatarThatExited in avatarsInArea.Except(newAvatarsInArea))
            {
                OnAvatarExit?.Invoke(avatarThatExited);
            }

            avatarsInArea = newAvatarsInArea;
        }

        private bool AreSetEquals(HashSet<GameObject> set1, HashSet<GameObject> set2)
        {
            if (set1 == null && set2 == null)
                return true;

            if (set1 == null || set2 == null)
                return false;

            return set1.SetEquals(set2);
        }

        private HashSet<GameObject> DetectAllAvatarsInArea()
        {
            if (entity?.gameObject == null)
                return null;
            
            UnityEngine.Vector3 center = entity.gameObject.transform.position;
            Quaternion rotation = entity.gameObject.transform.rotation;
            return ECSAvatarUtils.DetectAvatars(boxArea, center, rotation, excludedColliders);
        }

        private void RemoveAllModifiers()
        {
            RemoveAllModifiers(DetectAllAvatarsInArea());
            avatarsInArea.Clear();
        }

        private void RemoveAllModifiers(HashSet<GameObject> avatars)
        {
            if (avatars == null)
                return;

            foreach (GameObject avatar in avatars)
            {
                OnAvatarExit?.Invoke(avatar);
            }
        }

        private void ApplyCurrentModel(PBAvatarModifierArea model)
        {
            dataStore.ownPlayer.OnChange -= OwnPlayerOnOnChange;
            dataStore.otherPlayers.OnAdded -= OtherPlayersOnOnAdded;

            if (model.Modifiers.Count > 0)
            {
                // We set the unity engine Vector3 here, so we don't allocate a Vector3 each time we use it
                boxArea = ProtoConvertUtils.PBVectorToUnityVector(model.Area);
                
                // Add all listeners
                foreach (AvatarModifier modifierKey in model.Modifiers)
                {
                    var modifier = factory.GetOrCreateAvatarModifier(modifierKey);
                    
                    OnAvatarEnter += modifier.ApplyModifier;
                    OnAvatarExit += modifier.RemoveModifier;
                }

                // Set excluded colliders
                excludedColliders = GetExcludedColliders(model);

                if (model.ExcludeIds.Count > 0)
                {
                    dataStore.ownPlayer.OnChange += OwnPlayerOnOnChange;
                    dataStore.otherPlayers.OnAdded += OtherPlayersOnOnAdded;
                }

                this.model = model;
                
                // Force update due to after model update modifiers are removed and re-added
                // leaving a frame with the avatar without the proper modifications
                Update();
            }
        }
        
        internal HashSet<Collider> GetExcludedColliders(PBAvatarModifierArea model)
        {
            if (model.ExcludeIds.Count == 0)
                return null;

            var ownPlayer = dataStore.ownPlayer.Get();
            var otherPlayers = dataStore.otherPlayers;

            HashSet<Collider> result = new HashSet<Collider>();
            for (int i = 0; i < model.ExcludeIds.Count; i++)
            {
                if (ownPlayer != null && model.ExcludeIds[i] == ownPlayer.id)
                    result.Add(ownPlayer.collider);
                else if (otherPlayers.TryGetValue(model.ExcludeIds[i], out Player player))
                    result.Add(player.collider);
            }
            return result;
        }

        private void OtherPlayersOnOnAdded(string id, Player player) { RefreshExclusionList(player); }

        private void OwnPlayerOnOnChange(Player current, Player previous) { RefreshExclusionList(current); }

        private void OnOtherPlayersRemoved(string id, Player player) { excludedColliders?.Remove(player.collider); }

        private void RefreshExclusionList(Player player)
        {
            if (model.ExcludeIds.Count == 0 || !model.ExcludeIds.Contains(player.id))
                return;
            
            if (excludedColliders == null)
                excludedColliders = new HashSet<Collider>();
            
            excludedColliders.Add(player.collider);
        }
    }
}