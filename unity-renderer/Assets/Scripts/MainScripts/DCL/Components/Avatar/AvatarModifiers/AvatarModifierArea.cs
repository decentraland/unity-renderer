using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

public class AvatarModifierArea : BaseComponent
{
    [Serializable]
    public class Model : BaseModel
    {
        // TODO: Change to TriggerArea and handle deserialization with subclasses
        public BoxTriggerArea area = new ();
        public string[] modifiers;
        public string[] excludeIds;

        public override BaseModel GetDataFromJSON(string json) =>
            Utils.SafeFromJson<Model>(json);

        public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
        {
            if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.AvatarModifierArea)
                return Utils.SafeUnimplemented<AvatarModifierArea, Model>(expected: ComponentBodyPayload.PayloadOneofCase.AvatarModifierArea, actual: pbModel.PayloadCase);

            var pb = new Model();

            if (pbModel.AvatarModifierArea.Modifiers.Count != 0) pb.modifiers = pbModel.AvatarModifierArea.Modifiers.ToArray();
            if (pbModel.AvatarModifierArea.ExcludeIds.Count != 0) pb.excludeIds = pbModel.AvatarModifierArea.ExcludeIds.ToArray();
            if (pbModel.AvatarModifierArea.Area.Box != null) pb.area.box = pbModel.AvatarModifierArea.Area.Box.AsUnityVector3();

            return pb;
        }
    }

    private Model cachedModel = new ();

    private HashSet<GameObject> avatarsInArea = new ();
    private event Action<GameObject> OnAvatarEnter;
    private event Action<GameObject> OnAvatarExit;

    internal readonly Dictionary<string, IAvatarModifier> modifiers;

    private HashSet<Collider> excludedColliders;
    public override string componentName => "avatarModifierArea";

    public AvatarModifierArea()
    {
        // Configure all available modifiers
        this.modifiers = new Dictionary<string, IAvatarModifier>()
        {
            { "HIDE_AVATARS", new HideAvatarsModifier() },
            { "DISABLE_PASSPORTS", new HidePassportModifier() }
        };
        model = new Model();
    }

    public override IEnumerator ApplyChanges(BaseModel newModel)
    {

        // Clean up
        RemoveAllModifiers();
        OnAvatarEnter = null;
        OnAvatarExit = null;

        ApplyCurrentModel();

        return null;
    }

    private void Awake()
    {
        DataStore.i.player.otherPlayers.OnRemoved += OnOtherPlayersRemoved;
    }

    private void OnDestroy()
    {
        var toRemove = new HashSet<GameObject>();
        if (avatarsInArea != null)
            toRemove.UnionWith(avatarsInArea);

        var currentInArea = DetectAllAvatarsInArea();
        if (currentInArea != null)
            toRemove.UnionWith(currentInArea);

        RemoveAllModifiers(toRemove);

        DataStore.i.player.ownPlayer.OnChange -= OwnPlayerOnOnChange;
        DataStore.i.player.otherPlayers.OnAdded -= OtherPlayersOnOnAdded;
        DataStore.i.player.otherPlayers.OnRemoved -= OnOtherPlayersRemoved;
    }

    private void Update()
    {
        if (cachedModel?.area == null)
        {
            return;
        }

        // Find avatars currently on the area
        HashSet<GameObject> newAvatarsInArea = DetectAllAvatarsInArea();
        if (AreSetEquals(avatarsInArea, newAvatarsInArea))
            return;

        if (avatarsInArea == null)
            avatarsInArea = new HashSet<GameObject>();

        if (newAvatarsInArea == null)
            newAvatarsInArea = new HashSet<GameObject>();

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
        {
            return null;
        }

        Vector3 center = entity.gameObject.transform.position;
        Quaternion rotation = entity.gameObject.transform.rotation;
        return cachedModel.area?.DetectAvatars(center, rotation, excludedColliders);
    }

    private void RemoveAllModifiers()
    {
        RemoveAllModifiers(DetectAllAvatarsInArea());
        avatarsInArea = null;
    }

    private void RemoveAllModifiers(HashSet<GameObject> avatars)
    {
        if (cachedModel?.area == null)
        {
            return;
        }

        if (avatars != null)
        {
            foreach (GameObject avatar in avatars)
            {
                OnAvatarExit?.Invoke(avatar);
            }
        }
    }

    private void ApplyCurrentModel()
    {
        DataStore.i.player.ownPlayer.OnChange -= OwnPlayerOnOnChange;
        DataStore.i.player.otherPlayers.OnAdded -= OtherPlayersOnOnAdded;

        cachedModel = (Model)this.model;
        if (cachedModel.modifiers != null)
        {
            // Add all listeners
            foreach (string modifierKey in cachedModel.modifiers)
            {
                if (!modifiers.TryGetValue(modifierKey, out IAvatarModifier modifier))
                    continue;

                OnAvatarEnter += modifier.ApplyModifier;
                OnAvatarExit += modifier.RemoveModifier;
            }

            // Set excluded colliders
            excludedColliders = GetExcludedColliders(cachedModel);

            if (cachedModel.excludeIds != null && cachedModel.excludeIds.Length > 0)
            {
                DataStore.i.player.ownPlayer.OnChange += OwnPlayerOnOnChange;
                DataStore.i.player.otherPlayers.OnAdded += OtherPlayersOnOnAdded;
            }

            // Force update due to after model update modifiers are removed and re-added
            // leaving a frame with the avatar without the proper modifications
            Update();
        }
    }

    public override int GetClassId()
    {
        return (int)CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA;
    }

    private static HashSet<Collider> GetExcludedColliders(in Model componentModel)
    {
        string[] excludeIds = componentModel?.excludeIds;
        if (excludeIds == null || excludeIds.Length == 0)
        {
            return null;
        }

        var ownPlayer = DataStore.i.player.ownPlayer.Get();
        var otherPlayers = DataStore.i.player.otherPlayers;

        HashSet<Collider> result = new HashSet<Collider>();
        for (int i = 0; i < excludeIds.Length; i++)
        {
            if (ownPlayer != null && excludeIds[i] == ownPlayer.id)
            {
                result.Add(ownPlayer.collider);
            }
            else if (otherPlayers.TryGetValue(excludeIds[i], out Player player))
            {
                result.Add(player.collider);
            }
        }
        return result;
    }

    private void OtherPlayersOnOnAdded(string id, Player player)
    {
        RefreshExclusionList(player);
    }

    private void OwnPlayerOnOnChange(Player current, Player previous)
    {
        RefreshExclusionList(current);
    }

    private void OnOtherPlayersRemoved(string id, Player player)
    {
        excludedColliders?.Remove(player.collider);
    }

    private void RefreshExclusionList(Player player)
    {
        string[] excludeIds = cachedModel?.excludeIds;
        if (excludeIds == null || excludeIds.Length == 0)
        {
            return;
        }

        if (!excludeIds.Contains(player.id))
        {
            return;
        }

        if (excludedColliders == null)
        {
            excludedColliders = new HashSet<Collider>();
        }
        excludedColliders.Add(player.collider);
    }
}
