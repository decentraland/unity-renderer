using System;
using DCL;
using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

public class AvatarModifierArea : BaseComponent
{
    [Serializable]
    public class Model
    {
        // TODO: Change to TriggerArea and handle deserialization with subclasses
        public BoxTriggerArea area;
        public string[] modifiers;

    }

    [HideInInspector]
    public Model model = new Model();

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
    }

    public override object GetModel()
    {
        return model;
    }

    public override IEnumerator ApplyChanges(string newJson)
    {

        // Clean up
        RemoveAllModifiers();
        OnAvatarEnter = null;
        OnAvatarExit = null;

        // Update
        model = Utils.SafeFromJson<Model>(newJson);
        if (model.modifiers != null)
        {
            // Add all listeners
            foreach (string modifierKey in model.modifiers)
            {
                if (!modifiers.TryGetValue(modifierKey, out AvatarModifier modifier))
                    continue;

                OnAvatarEnter += modifier.ApplyModifier;
                OnAvatarExit += modifier.RemoveModifier;
            }
        }

        yield break;
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
        if (model?.area == null)
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
        return model.area.DetectAvatars(center, rotation);
    }

    private void RemoveAllModifiers()
    {
        RemoveAllModifiers(DetectAllAvatarsInArea());
    }

    private void RemoveAllModifiers(HashSet<GameObject> avatars)
    {
        if (model?.area == null)
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

}
