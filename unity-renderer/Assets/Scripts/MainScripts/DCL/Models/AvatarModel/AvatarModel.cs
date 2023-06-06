using System;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Decentraland.Sdk.Ecs6;
using MainScripts.DCL.Components;

[Serializable]
public class AvatarModel : BaseModel
{
    [Serializable]
    public class AvatarEmoteEntry
    {
        public int slot;
        public string urn;
    }

    public string id;
    public string name;
    public string bodyShape;
    public Color skinColor;
    public Color hairColor;
    public Color eyeColor;
    public List<string> wearables = new List<string>();
    public HashSet<string> forceRender = new HashSet<string>();

    public List<AvatarEmoteEntry> emotes = new List<AvatarEmoteEntry>();

    public string expressionTriggerId = null;
    public long expressionTriggerTimestamp = -1;
    public bool talking = false;

    public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
    {
        if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.AvatarShape)
            return Utils.SafeUnimplemented<AvatarModel, AvatarModel>(expected: ComponentBodyPayload.PayloadOneofCase.AvatarShape, actual: pbModel.PayloadCase);

        var model = new AvatarModel();
        if (pbModel.AvatarShape.HasId) model.id = pbModel.AvatarShape.Id;
        if (pbModel.AvatarShape.HasName) model.name = pbModel.AvatarShape.Name;
        if (pbModel.AvatarShape.HasTalking) model.talking = pbModel.AvatarShape.Talking;
        if (pbModel.AvatarShape.HasBodyShape) model.bodyShape = pbModel.AvatarShape.BodyShape;
        if (pbModel.AvatarShape.EyeColor != null) model.eyeColor = pbModel.AvatarShape.EyeColor.AsUnityColor();
        if (pbModel.AvatarShape.HairColor != null) model.hairColor = pbModel.AvatarShape.HairColor.AsUnityColor();
        if (pbModel.AvatarShape.SkinColor != null) model.skinColor = pbModel.AvatarShape.SkinColor.AsUnityColor();
        if (pbModel.AvatarShape.HasExpressionTriggerId) model.expressionTriggerId = pbModel.AvatarShape.ExpressionTriggerId;
        if (pbModel.AvatarShape.HasExpressionTriggerTimestamp) model.expressionTriggerTimestamp = pbModel.AvatarShape.ExpressionTriggerTimestamp;
        if (pbModel.AvatarShape.Wearables is { Count: > 0 }) model.wearables = pbModel.AvatarShape.Wearables.ToList();
        if (pbModel.AvatarShape.Emotes is { Count: > 0 })
        {
            model.emotes = new List<AvatarEmoteEntry>(pbModel.AvatarShape.Emotes.Count);
            for (var i = 0; i < pbModel.AvatarShape.Emotes.Count; i++)
            {
                if (pbModel.AvatarShape.Emotes[i] == null) continue;
                AvatarEmoteEntry emote = new AvatarEmoteEntry();

                if (pbModel.AvatarShape.Emotes[i].HasSlot) emote.slot = pbModel.AvatarShape.Emotes[i].Slot;
                if (pbModel.AvatarShape.Emotes[i].HasUrn) emote.urn = pbModel.AvatarShape.Emotes[i].Urn;
                model.emotes.Add(emote);
            }
        }

        return model;

    }

    public bool HaveSameWearablesAndColors(AvatarModel other)
    {
        if (other == null)
            return false;

        //wearables are the same
        if (!(wearables.Count == other.wearables.Count
              && wearables.All(other.wearables.Contains)
              && other.wearables.All(wearables.Contains)))
            return false;

        if (!(forceRender.Count == other.forceRender.Count
              && forceRender.All(other.forceRender.Contains)
              && other.forceRender.All(forceRender.Contains)))
            return false;

        //emotes are the same
        if (emotes == null && other.emotes != null)
            return false;
        if (emotes != null && other.emotes == null)
            return false;
        if (emotes != null && other.emotes != null)
        {
            if (emotes.Count != other.emotes.Count)
                return false;

            for (var i = 0; i < emotes.Count; i++)
            {
                AvatarEmoteEntry emote = emotes[i];
                if (other.emotes.FirstOrDefault(x => x.urn == emote.urn) == null)
                    return false;
            }
        }

        return bodyShape == other.bodyShape &&
               skinColor == other.skinColor &&
               hairColor == other.hairColor &&
               eyeColor == other.eyeColor;
    }

    public bool HaveSameExpressions(AvatarModel other)
    {
        return expressionTriggerId == other.expressionTriggerId &&
               expressionTriggerTimestamp == other.expressionTriggerTimestamp;
    }

    public bool Equals(AvatarModel other)
    {
        if (other == null) return false;

        bool wearablesAreEqual = wearables.All(other.wearables.Contains)
                                 && other.wearables.All(wearables.Contains)
                                 && wearables.Count == other.wearables.Count;

        bool forceRenderAreEqual = forceRender.All(other.forceRender.Contains)
                                 && other.forceRender.All(forceRender.Contains)
                                 && forceRender.Count == other.forceRender.Count;

        return id == other.id &&
               name == other.name &&
               bodyShape == other.bodyShape &&
               skinColor == other.skinColor &&
               hairColor == other.hairColor &&
               eyeColor == other.eyeColor &&
               expressionTriggerId == other.expressionTriggerId &&
               expressionTriggerTimestamp == other.expressionTriggerTimestamp &&
               wearablesAreEqual &&
               forceRenderAreEqual;
    }

    public void CopyFrom(AvatarModel other)
    {
        if (other == null)
            return;

        id = other.id;
        name = other.name;
        bodyShape = other.bodyShape;
        skinColor = other.skinColor;
        hairColor = other.hairColor;
        eyeColor = other.eyeColor;
        expressionTriggerId = other.expressionTriggerId;
        expressionTriggerTimestamp = other.expressionTriggerTimestamp;
        wearables = new List<string>(other.wearables);
        emotes = other.emotes.Select(x => new AvatarEmoteEntry() { slot = x.slot, urn = x.urn }).ToList();
        forceRender = new HashSet<string>(other.forceRender);
    }

    public override BaseModel GetDataFromJSON(string json) =>
        Utils.SafeFromJson<AvatarModel>(json);

    public AvatarModelDTO ToAvatarModelDto()
    {
        AvatarModelDTO avatarModelDto = new AvatarModelDTO
            {
                id = this.id,
                name = this.name,
                bodyShape = this.bodyShape,
                skinColor = this.skinColor,
                hairColor = this.hairColor,
                eyeColor = this.eyeColor,
                wearables = this.wearables,
                forceRender = this.forceRender.ToList(),
                emotes = this.emotes,
                expressionTriggerId = this.expressionTriggerId,
                expressionTriggerTimestamp = this.expressionTriggerTimestamp,
                talking = this.talking,
            };

        return avatarModelDto;
    }
}
