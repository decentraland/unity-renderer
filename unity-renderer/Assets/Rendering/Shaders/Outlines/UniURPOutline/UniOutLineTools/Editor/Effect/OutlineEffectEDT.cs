﻿// editor

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace UniOutline.Outline
{
	[CustomEditor(typeof(OutlineEffect))]
	public class OutlineEffectEDT : Editor
	{
		private OutlineEffect _effect;
		private bool _debugOpened;
		private bool _previewOpened;

		private void OnEnable()
		{
			_effect = (OutlineEffect)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			EditorGUI.BeginChangeCheck();
			var e = (CameraEvent)EditorGUILayout.EnumPopup("Render Event", _effect.RenderEvent);

			if (e != _effect.RenderEvent)
			{
				Undo.RecordObject(_effect, "Set Render Event");
				_effect.RenderEvent = e;
			}

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(_effect.gameObject);

				if (!EditorApplication.isPlayingOrWillChangePlaymode)
				{
					EditorSceneManager.MarkSceneDirty(_effect.gameObject.scene);
				}
			}

			if (_effect.OutlineLayers)
			{
				if (_effect.OutlineLayers.Count > 0)
				{
					_previewOpened = EditorGUILayout.Foldout(_previewOpened, "Preview", true);

					if (_previewOpened)
					{
						OutlineEditorUtility.RenderPreview(_effect.OutlineLayers, true);
					}
				}
			}
		}
	}
}
