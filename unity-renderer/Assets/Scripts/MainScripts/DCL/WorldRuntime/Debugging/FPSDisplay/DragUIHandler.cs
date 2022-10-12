using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
 
public class DragUIHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
 
    private Vector2 pointerOffset;
    private RectTransform canvasRectTransform;
    private RectTransform panelRectTransform;
 
    public void Start(){
       
        Canvas canvas = GetComponentInParent<Canvas> ();
        if(canvas != null){
            canvasRectTransform = canvas.transform as RectTransform;
            panelRectTransform = transform as RectTransform;
        }
    }
 
    #region IBeginDragHandler implementation
 
    public void OnBeginDrag (PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
    }
 
    #endregion
 
    #region IDragHandler implementation
 
    public void OnDrag (PointerEventData eventData)
    {
        if(panelRectTransform == null){
            return;
        }
        Vector2 localPointerPosition;
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle (canvasRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition)){
            panelRectTransform.localPosition = localPointerPosition - pointerOffset;
        }
    }
 
    #endregion
 
    #region IEndDragHandler implementation
 
    public void OnEndDrag (PointerEventData eventData)
    {
 
    }
 
    #endregion
}