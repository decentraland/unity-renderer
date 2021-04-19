using UnityEngine;
using UnityEngine.EventSystems;

public class PointerOverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void Enter(PointerEventData eventData);
    public delegate void Exit(PointerEventData eventData);

    public event Enter OnEnter;
    public event Exit OnExit;

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnExit?.Invoke(eventData);
    }
}