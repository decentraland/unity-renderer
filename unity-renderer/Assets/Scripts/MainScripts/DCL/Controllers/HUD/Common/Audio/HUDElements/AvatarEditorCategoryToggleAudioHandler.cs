using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvatarEditorCategoryToggleAudioHandler : ButtonAudioHandler
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        if (selectable != null)
        {
            if (selectable.interactable)
                AudioScriptableObjects.listItemAppear.ResetPitch();
        }
    }
}
