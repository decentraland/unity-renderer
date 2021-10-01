#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

[ExcludeFromCodeCoverage]
[CustomEditor(typeof(BaseComponentView), true)]
public class BaseComponentViewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BaseComponentView myScript = (BaseComponentView)target;
        if (GUILayout.Button("REFRESH"))
        {
            myScript.RefreshControl();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif