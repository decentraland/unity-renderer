using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Builder
{
    public abstract class GizmoAxis : MonoBehaviour
    {
        public Vector3 axis;
        public Color color;
        public Color selectedColor;

        bool isSelected;
        Renderer objectRenderer;
        MaterialPropertyBlock props;
        public Vector3 originPointerPosition;

        static int colorPropertyID;
        static bool isColorPropertyIdSet = false;

        protected float snapFactor = 0;

        void Awake()
        {
            if (!isColorPropertyIdSet)
            {
                isColorPropertyIdSet = true;
                colorPropertyID = Shader.PropertyToID("_BaseColor");
            }
        }

        void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            props = new MaterialPropertyBlock();
            objectRenderer.GetPropertyBlock(props);
            props.SetColor(colorPropertyID, color);
            objectRenderer.SetPropertyBlock(props);
        }

        public void SelectAxis(bool selected)
        {
            if (isSelected != selected)
            {
                isSelected = selected;
                objectRenderer.GetPropertyBlock(props);
                if (selected)
                {
                    props.SetColor(colorPropertyID, selectedColor);
                }
                else
                {
                    props.SetColor(colorPropertyID, color);
                }
                objectRenderer.SetPropertyBlock(props);
            }
        }

        public void ResetTransformation()
        {
            originPointerPosition = Vector3.zero;
        }

        public abstract void UpdateTransformation(Vector3 inputPosition, Vector3 pointerPosition, GameObject selectedObject, Vector3 hitPoint);
        public abstract void SetSnapFactor(float position, float rotation, float scale);

    }
}