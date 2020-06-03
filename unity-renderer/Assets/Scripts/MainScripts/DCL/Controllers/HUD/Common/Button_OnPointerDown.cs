using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// In oder to lock/unlock the cursor in the same frame a button is clicked we need to use OnPointerDown (event not accesible in the regular Button)
/// </summary>
public class Button_OnPointerDown : Button
{
    public event UnityAction onPointerDown;

    public override void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke();
    }
}
