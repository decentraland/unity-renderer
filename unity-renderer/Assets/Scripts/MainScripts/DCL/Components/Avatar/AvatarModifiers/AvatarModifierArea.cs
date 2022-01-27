using System;
using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using DCL.Models;

public class AvatarModifierArea : BaseComponent
{
    [Serializable]
    public class Model : BaseModel
    {
        // TODO: Change to TriggerArea and handle deserialization with subclasses
        public BoxTriggerArea area;
        public string[] modifiers;

        public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<Model>(json); }
    }

    private Model cachedModel = new Model();

    private HashSet<Collider> avatarsInArea = new HashSet<Collider>();
    private event Action<GameObject> OnAvatarEnter;
    private event Action<GameObject> OnAvatarExit;
    internal readonly Dictionary<string, AvatarModifier> modifiers;

    public AvatarModifierArea()
    {
        // Configure all available modifiers
        this.modifiers = new Dictionary<string, AvatarModifier>()
        {
            { "HIDE_AVATARS", new HideAvatarsModifier() },
            { "DISABLE_PASSPORTS", new DisablePassportModifier() }
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

    private void OnDestroy()
    {
        var toRemove = new HashSet<Collider>();
        if (avatarsInArea != null)
            toRemove.UnionWith(avatarsInArea);

        var currentInArea = DetectAllAvatarsInArea();
        if (currentInArea != null)
            toRemove.UnionWith(currentInArea);

        RemoveAllModifiers(toRemove);
    }

    private void Update()
    {
        if (cachedModel?.area == null)
        {
            return;
        }

        // Find avatars currently on the area
        HashSet<Collider> newAvatarsInArea = DetectAllAvatarsInArea();
        if (AreSetEquals(avatarsInArea, newAvatarsInArea))
            return;

        if (avatarsInArea == null)
            avatarsInArea = new HashSet<Collider>();

        if (newAvatarsInArea == null)
            newAvatarsInArea = new HashSet<Collider>();

        // Call event for avatars that just entered the area
        foreach (Collider avatarThatEntered in newAvatarsInArea.Except(avatarsInArea))
        {
            OnAvatarEnter?.Invoke(ColliderToAvatarGO(avatarThatEntered));
        }

        // Call events for avatars that just exited the area
        foreach (Collider avatarThatExited in avatarsInArea.Except(newAvatarsInArea))
        {
            OnAvatarExit?.Invoke(ColliderToAvatarGO(avatarThatExited));
        }

        avatarsInArea = newAvatarsInArea;
    }

    private bool AreSetEquals(HashSet<Collider> set1, HashSet<Collider> set2)
    {
        if (set1 == null && set2 == null)
            return true;

        if (set1 == null || set2 == null)
            return false;

        return set1.SetEquals(set2);
    }

    private HashSet<Collider> DetectAllAvatarsInArea()
    {
        if (entity?.gameObject == null)
        {
            return null;
        }

        Vector3 center = entity.gameObject.transform.position;
        Quaternion rotation = entity.gameObject.transform.rotation;
        return cachedModel.area?.DetectAvatars(center, rotation);
    }

    private void RemoveAllModifiers() { RemoveAllModifiers(DetectAllAvatarsInArea()); }

    private void RemoveAllModifiers(HashSet<Collider> avatars)
    {
        if (cachedModel?.area == null)
        {
            return;
        }

        if (avatars != null)
        {
            foreach (Collider avatar in avatars)
            {
                OnAvatarExit?.Invoke(ColliderToAvatarGO(avatar));
            }
        }
    }

    private void ApplyCurrentModel()
    {
        cachedModel = (Model)this.model;
        if (cachedModel.modifiers != null)
        {
            // Add all listeners
            foreach (string modifierKey in cachedModel.modifiers)
            {
                if (!modifiers.TryGetValue(modifierKey, out AvatarModifier modifier))
                    continue;

                OnAvatarEnter += modifier.ApplyModifier;
                OnAvatarExit += modifier.RemoveModifier;
            }
        }
    }

    public override int GetClassId() { return (int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA; }

    private static GameObject ColliderToAvatarGO(Collider collider)
    {
        return collider.transform.parent.gameObject;
    }
}