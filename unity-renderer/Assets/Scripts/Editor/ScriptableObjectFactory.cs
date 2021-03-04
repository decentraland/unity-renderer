using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A helper class for instantiating ScriptableObjects in the editor.
/// </summary>
public class ScriptableObjectFactory
{
	[MenuItem("Assets/Create/ScriptableObject")]
	public static void CreateScriptableObject()
	{
		var assembly = GetAssembly ();

		// Get all classes derived from ScriptableObject
		var allScriptableObjects = (from t in assembly.GetTypes()
									where t.IsSubclassOf(typeof(ScriptableObject))
		                            select t).ToArray();

		// Show the selection window.
		ScriptableObjectWindow.Init(allScriptableObjects);
	}

	/// <summary>
	/// Returns the assembly that contains the script code for this project (currently hard coded)
	/// </summary>
	private static Assembly GetAssembly ()
	{
		return Assembly.Load (new AssemblyName ("MainScripts"));
	}
}