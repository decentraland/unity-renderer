// editor

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UniOutline.Outline
{
	[CustomPropertyDrawer(typeof(OutlineSettingsWithLayerMask))]
	
	public class OutlineSettingsWithLayerMaskDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect rc, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(rc, label, property);
			OutlineSettingsEDT.DrawSettingsWithMask(rc, property);
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return OutlineSettingsEDT.GetSettingsWithMaskHeight(property);
		}
	}
}
