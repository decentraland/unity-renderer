using System;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public interface IBIWGizmosController : IBIWController
{
    delegate void GizmoTransformDelegate(string gizmoType);

    event GizmoTransformDelegate OnGizmoTransformObjectStart;
    event GizmoTransformDelegate OnGizmoTransformObject;
    event GizmoTransformDelegate OnGizmoTransformObjectEnd;
    event Action<Vector3> OnChangeTransformValue;

    IBIWGizmos activeGizmo { get;  set; }

    string GetSelectedGizmo();
    void SetSnapFactor(float position, float rotation, float scale);
    void SetSelectedEntities(Transform selectionParent, List<BIWEntity> entities);
    void ShowGizmo();
    void HideGizmo(bool setInactiveGizmos = false);
    bool IsGizmoActive();
    void ForceRelativeScaleRatio();
    bool HasAxisHover();
    void SetGizmoType(string gizmoType);
}