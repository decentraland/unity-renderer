// editor
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniOutline.Outline
{
	[CustomPropertyDrawer(typeof(OutlineSettingsInstance))]
	public class OutlineSettingsInstanceDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect rc, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(rc, label, property);
			OutlineSettingsEDT.DrawSettingsInstance(rc, property);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return OutlineSettingsEDT.GetSettingsInstanceHeight(property);
		}
	}
}
