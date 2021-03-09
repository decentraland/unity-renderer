using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverObjectToggler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject[] targetObjects;

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetObjectsActiveState(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetObjectsActiveState(false);
    }

    void OnDisable()
    {
        OnPointerExit(null);
    }

    void SetObjectsActiveState(bool newState)
    {
        for (int i = 0; i < targetObjects.Length; i++)
        {
            targetObjects[i].SetActive(newState);
        }
    }
}
