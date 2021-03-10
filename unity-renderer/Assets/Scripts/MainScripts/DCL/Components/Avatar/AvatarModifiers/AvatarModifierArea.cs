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
        
        public override BaseModel GetDataFromJSON(string json)
        {
            return Utils.SafeFromJson<Model>(json);
        }
    }

    private Model cachedModel = new Model();

    private HashSet<GameObject> avatarsInArea = new HashSet<GameObject>();
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
        var toRemove = new HashSet<GameObject>();
        if(avatarsInArea != null)
            toRemove.UnionWith(avatarsInArea);

        var currentInArea = DetectAllAvatarsInArea();
        if(currentInArea != null)
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
        HashSet<GameObject> newAvatarsInArea = DetectAllAvatarsInArea();
        if (AreSetEquals(avatarsInArea, newAvatarsInArea))
            return;

        if(avatarsInArea == null)
            avatarsInArea = new HashSet<GameObject>();

        if(newAvatarsInArea == null)
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
        return cachedModel.area?.DetectAvatars(center, rotation);
    }

    private void RemoveAllModifiers()
    {
        RemoveAllModifiers(DetectAllAvatarsInArea());
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

    public override int GetClassId()
    {
        return (int) CLASS_ID_COMPONENT.AVATAR_MODIFIER_AREA;
    }
}
