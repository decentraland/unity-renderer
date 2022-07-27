using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class csharptest : EditorWindow
{
    [MenuItem("Window/UI Toolkit/csharptest")]
    public static void ShowExample()
    {
        csharptest wnd = GetWindow<csharptest>();
        wnd.titleContent = new GUIContent("csharptest");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);
    }
}