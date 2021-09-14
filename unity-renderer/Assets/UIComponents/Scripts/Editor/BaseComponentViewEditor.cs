using UnityEditor;
using UnityEngine;

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