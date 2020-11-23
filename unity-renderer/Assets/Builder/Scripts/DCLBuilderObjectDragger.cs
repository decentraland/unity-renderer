using System;
using System.Collections.Generic;
using UnityEngine;

namespace Builder
{
    public class DCLBuilderObjectDragger : MonoBehaviour
    {
        public DCLBuilderRaycast builderRaycast;

        public static event Action OnDraggingObjectStart;
        public static event Action OnDraggingObject;
        public static event Action OnDraggingObjectEnd;

        private List<EditableEntity> selectedEntities;
        private Vector3 targetOffset;
        private Vector3 initialHitPoint;
        private Transform selectedEntitiesParent;
        private bool isDragging = false;

        private float snapFactorPosition = 0;

        private bool isGameObjectActive = false;

        private void OnEnable()
        {
            if (!isGameObjectActive)
            {
                DCLBuilderObjectSelector.OnSelectedObjectListChanged += OnSelectedObjectListChanged;
                DCLBuilderObjectSelector.OnEntityPressed += OnEntityPressed;
                DCLBuilderInput.OnMouseDrag += OnMouseDrag;
                DCLBuilderInput.OnMouseUp += OnMouseUp;
                DCLBuilderBridge.OnSetGridResolution += OnSetGridResolution;
            }
            isGameObjectActive = true;
        }

        private void OnDisable()
        {
            isGameObjectActive = false;
            DCLBuilderObjectSelector.OnSelectedObjectListChanged -= OnSelectedObjectListChanged;
            DCLBuilderObjectSelector.OnEntityPressed -= OnEntityPressed;
            DCLBuilderInput.OnMouseDrag -= OnMouseDrag;
            DCLBuilderInput.OnMouseUp -= OnMouseUp;
            DCLBuilderBridge.OnSetGridResolution -= OnSetGridResolution;
        }

        private void OnSelectedObjectListChanged(Transform selectionParent, List<EditableEntity> selectedEntities)
        {
            this.selectedEntitiesParent = selectionParent;
            this.selectedEntities = selectedEntities;
            if (isDragging)
            {
                targetOffset = selectedEntitiesParent.position - initialHitPoint;
            }
        }

        private void OnEntityPressed(DCLBuilderEntity entity, Vector3 hitPoint)
        {
            if (selectedEntities == null || !selectedEntities.Contains(entity))
            {
                return;
            }

            OnDraggingObjectStart?.Invoke();

            initialHitPoint = hitPoint;
            targetOffset = selectedEntitiesParent.position - hitPoint;
            builderRaycast.SetEntityHitPlane(hitPoint.y);
            isDragging = true;
        }

        private void OnMouseUp(int buttonId, Vector3 mousePosition)
        {
            // NOTE: only process mouse's left button press
            if (buttonId != 0)
            {
                return;
            }

            if (isDragging && selectedEntities != null)
            {
                OnDraggingObjectEnd?.Invoke();
            }
            isDragging = false;
        }

        private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
        {
            // NOTE: only process if mouse's left button is pressed and if there is any entity selected
            if (buttonId != 0 || selectedEntities == null)
            {
                return;
            }

            bool hasMouseMoved = (axisX != 0 || axisY != 0);
            if (isDragging && hasMouseMoved)
            {
                DragTargetEntity(mousePosition);
            }
        }

        private void DragTargetEntity(Vector3 mousePosition)
        {
            Vector3 hitPosition = builderRaycast.RaycastToEntityHitPlane(mousePosition);
            Vector3 targetPosition = hitPosition + targetOffset;
            targetPosition.y = selectedEntitiesParent.position.y;

            if (snapFactorPosition > 0)
            {
                targetPosition.x = targetPosition.x - (targetPosition.x % snapFactorPosition);
                targetPosition.z = targetPosition.z - (targetPosition.z % snapFactorPosition);
            }

            Vector3 moveAmount = targetPosition - selectedEntitiesParent.transform.position;
            selectedEntitiesParent.transform.position = targetPosition;

            OnDraggingObject?.Invoke();
        }

        private void OnSetGridResolution(float position, float rotation, float scale)
        {
            snapFactorPosition = position;
        }
    }
}