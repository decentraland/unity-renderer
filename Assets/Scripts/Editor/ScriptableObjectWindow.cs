using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

internal class EndNameEdit : EndNameEditAction
{
	#region implemented abstract members of EndNameEditAction
	public override void Action (int instanceId, string pathName, string resourceFile)
	{
		AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
	}

	#endregion
}

/// <summary>
/// Scriptable object window.
/// </summary>
public class ScriptableObjectWindow : EditorWindow
{
	private int selectedIndex;
	private static string[] names;
	
	private static Type[] types;
	
	private static Type[] Types
	{ 
		get { return types; }
		set
		{
			types = value;
			names = types.Select(t => t.FullName).ToArray();
		}
	}

	public static void Init(Type[] scriptableObjects)
	{
		Types = scriptableObjects;

		var window = EditorWindow.GetWindow<ScriptableObjectWindow>(true, "Create a new ScriptableObject", true);
		window.ShowPopup();
	}

	public void OnGUI()
	{
		GUILayout.Label("ScriptableObject Class");
		selectedIndex = EditorGUILayout.Popup(selectedIndex, names);

		if (GUILayout.Button("Create"))
		{
			var asset = ScriptableObject.CreateInstance(types[selectedIndex]);
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				asset.GetInstanceID(),
				ScriptableObject.CreateInstance<EndNameEdit>(),
				string.Format("{0}.asset", names[selectedIndex]),
				AssetPreview.GetMiniThumbnail(asset), 
				null);

			Close();
		}
	}
}