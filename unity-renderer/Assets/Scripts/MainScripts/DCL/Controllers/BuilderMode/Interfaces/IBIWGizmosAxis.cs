using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public interface IBIWGizmosAxis
{
    void SetColorHighlight();
    void SetColorDefault();
    IBIWGizmos GetGizmo();

    Transform axisTransform { get; }
}