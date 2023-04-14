using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSlotsHUDController : IHUD
{
    private AvatarSlotsDefinitionSO avatarSlotsDefinition;

    public AvatarSlotsHUDController()
    {
        avatarSlotsDefinition = Resources.Load<AvatarSlotsDefinitionSO>("AvatarSlotsDefinition");
    }

    private void GenerateSlots()
    {
        foreach (SerializableKeyValuePair<string, List<string>> section in avatarSlotsDefinition.slotsDefinition)
        {
            foreach (string avatarSlotSection in section.value)
            {
                //generate elements in section
            }
        }
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void SetVisibility(bool visible)
    {
        throw new NotImplementedException();
    }
}
