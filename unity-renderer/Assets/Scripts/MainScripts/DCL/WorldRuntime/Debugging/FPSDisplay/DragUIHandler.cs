using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragUIHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
 
    private Vector2 pointerOffset;
    private RectTransform canvasRectTransform;
    private RectTransform panelRectTransform;
    private Vector3 startLocalScale;
    private Vector2 originalPivot;
    [SerializeField] private float scaleDownOnBeginDrag = 1f;
    private bool jumpOneFrame;

    public void Start(){
       
        Canvas canvas = GetComponentInParent<Canvas> ();
        if(canvas != null){
            canvasRectTransform = canvas.transform as RectTransform;
            panelRectTransform = transform as RectTransform;
        }
        startLocalScale = panelRectTransform.localScale;
        originalPivot = panelRectTransform.pivot;
    }
 
    #region IBeginDragHandler implementation
 
    public void OnBeginDrag (PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPointerForNewPivot);
        SetPivot(panelRectTransform, Rect.PointToNormalized(panelRectTransform.rect, localPointerForNewPivot));
        panelRectTransform.localScale = startLocalScale * scaleDownOnBeginDrag;
        RectTransformUtility.ScreenPointToLocalPointInRectangle (panelRectTransform, eventData.position, eventData.pressEventCamera, out pointerOffset);
    }
 
    #endregion
 
    #region IDragHandler implementation
 
    public void OnDrag (PointerEventData eventData)
    {
        if(panelRectTransform == null){
            return;
        }
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle (canvasRectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition)){
            panelRectTransform.localPosition = localPointerPosition - pointerOffset;
        }
    }
 
    #endregion
 
    #region IEndDragHandler implementation
 
    public void OnEndDrag (PointerEventData eventData)
    {
        panelRectTransform.localScale = startLocalScale;
        SetPivot(panelRectTransform, originalPivot);
    }
 
    #endregion
    
    private void SetPivot(RectTransform target, Vector2 pivot)
    {
        if (!target) return;
        var offset= pivot - target.pivot;
        offset.Scale(target.rect.size);
        var wordlPos= target.position + target.TransformVector(offset);
        target.pivot = pivot;
        target.position = wordlPos;
    }
}