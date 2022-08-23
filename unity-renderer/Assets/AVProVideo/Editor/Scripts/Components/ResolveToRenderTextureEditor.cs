using UnityEditor;
using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo.Editor
{
	/// <summary>
	/// Editor for the ResolveToRenderTexture component
	/// </summary>
	[CanEditMultipleObjects]
	[CustomEditor(typeof(ResolveToRenderTexture))]
	public class ResolveToRenderTextureEditor : UnityEditor.Editor
	{
		private SerializedProperty _propMediaPlayer;
		private SerializedProperty _propExternalTexture;
		private SerializedProperty _propResolveFlags;

		private SerializedProperty _propOptionsApplyHSBC;
		private SerializedProperty _propOptionsHue;
		private SerializedProperty _propOptionsSaturation;
		private SerializedProperty _propOptionsBrightness;
		private SerializedProperty _propOptionsContrast;
		private SerializedProperty _propOptionsGamma;
		private SerializedProperty _propOptionsTint;

		void OnEnable()
		{
			_propMediaPlayer = this.CheckFindProperty("_mediaPlayer");
			_propExternalTexture = this.CheckFindProperty("_externalTexture");
			_propResolveFlags = this.CheckFindProperty("_resolveFlags");
			_propOptionsApplyHSBC = this.CheckFindProperty("_options.applyHSBC");
			_propOptionsHue = this.CheckFindProperty("_options.hue");
			_propOptionsSaturation = this.CheckFindProperty("_options.saturation");
			_propOptionsBrightness = this.CheckFindProperty("_options.brightness");
			_propOptionsContrast = this.CheckFindProperty("_options.contrast");
			_propOptionsGamma = this.CheckFindProperty("_options.gamma");
			_propOptionsTint = this.CheckFindProperty("_options.tint");
		}

		private void ButtonFloatReset(SerializedProperty prop, float value)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(prop);
			if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
			{
				prop.floatValue = value;
			}
			GUILayout.EndHorizontal();
		}

		private void ButtonColorReset(SerializedProperty prop, Color value)
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(prop);
			if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
			{
				prop.colorValue = value;
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(_propMediaPlayer);
			EditorGUILayout.PropertyField(_propExternalTexture);
			_propResolveFlags.intValue = EditorGUILayout.MaskField("Resolve Flags", _propResolveFlags.intValue, System.Enum.GetNames(typeof( VideoRender.ResolveFlags)));

			EditorGUI.BeginChangeCheck();
			{
				EditorGUILayout.PropertyField(_propOptionsApplyHSBC);
				EditorGUI.BeginDisabledGroup(!_propOptionsApplyHSBC.boolValue);
				{
					EditorGUI.indentLevel++;
					ButtonFloatReset(_propOptionsHue, 0f);
					ButtonFloatReset(_propOptionsSaturation, 0.5f);
					ButtonFloatReset(_propOptionsBrightness, 0.5f);
					ButtonFloatReset(_propOptionsContrast, 0.5f);
					ButtonFloatReset(_propOptionsGamma, 1f);
					EditorGUI.indentLevel--;
				}
				EditorGUI.EndDisabledGroup();
				ButtonColorReset(_propOptionsTint, Color.white);
			}
			if (EditorGUI.EndChangeCheck())
			{
				Object[] resolves = this.serializedObject.targetObjects;
				if (resolves != null)
				{
					foreach (ResolveToRenderTexture resolve in resolves)
					{
						resolve.SetMaterialDirty();
					}
				}
			}

			serializedObject.ApplyModifiedProperties();

			{
				ResolveToRenderTexture resolve = this.target as ResolveToRenderTexture;
				if (resolve != null && resolve.TargetTexture != null)
				{
					Rect r = GUILayoutUtility.GetAspectRect(resolve.TargetTexture.width / (float)resolve.TargetTexture.height);
					GUI.DrawTexture(r, resolve.TargetTexture, ScaleMode.StretchToFill, true);
					if (GUILayout.Button("Select Texture"))
					{
						Selection.activeObject = resolve.TargetTexture;
					}
					Repaint();
				}
			}
		}
	}
}