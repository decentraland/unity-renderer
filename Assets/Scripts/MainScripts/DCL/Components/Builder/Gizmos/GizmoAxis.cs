using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Models;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class GizmoAxis : MonoBehaviour
{
    public Gizmo gizmo;
    public Vector3 axis;
    public Color color;
    public Color selectedColor;
    bool isSelected;
    Renderer renderer;
    MaterialPropertyBlock props;
    public Vector3 originPointerPosition;

    int colorPropertyID;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
        props = new MaterialPropertyBlock();
        colorPropertyID = Shader.PropertyToID("_BaseColor");
        renderer.GetPropertyBlock(props);
        props.SetColor(colorPropertyID, color);
        renderer.SetPropertyBlock(props);
    }

    public void SelectAxis(bool selected)
    {
        if (isSelected != selected)
        {
            isSelected = selected;
            renderer.GetPropertyBlock(props);
            if (selected)
            {
                props.SetColor(colorPropertyID, selectedColor);
            }
            else
            {
                props.SetColor(colorPropertyID, color);
            }
            renderer.SetPropertyBlock(props);
        }
    }

    public void ResetTransformation()
    {
        originPointerPosition = Vector3.zero;
    }

    public abstract void UpdateTransformation(Vector3 inputPosition, Vector3 pointerPosition, GameObject selectedObject, RaycastHit hitInfo);





}
