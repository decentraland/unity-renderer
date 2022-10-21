
using System;
using UnityEngine;
using UniOutline.Outline;


namespace UniOutline.Outline
{
	[Serializable]
	public class OutlineSettingsInstance : IOutlineSettings
	{
		#region data

#pragma warning disable 0649

		// NOTE: There are custom editors for public components, so no need to show these in default inspector.
		[SerializeField, HideInInspector]
		private OutlineSettings _outlineSettings;
		[SerializeField, HideInInspector]
		private Color _outlineColor = Color.red;
		[SerializeField, HideInInspector, Range(OutlineResources.MinWidth, OutlineResources.MaxWidth)]
		private int _outlineWidth = 4;
		[SerializeField, HideInInspector, Range(OutlineResources.MinIntensity, OutlineResources.MaxIntensity)]
		private float _outlineIntensity = 2;
		[SerializeField, HideInInspector, Range(OutlineResources.MinAlphaCutoff, OutlineResources.MaxAlphaCutoff)]
		private float _outlineAlphaCutoff = 0.9f;
		[SerializeField, HideInInspector]
		private OutlineRenderFlags _outlineMode;

#pragma warning restore 0649

		#endregion

		#region interface

		public bool RequiresCameraDepth
		{
			get
			{
				return (OutlineRenderMode & OutlineRenderFlags.EnableDepthTesting) != 0;
			}
		}

		public OutlineSettings OutlineSettings
		{
			get
			{
				return _outlineSettings;
			}
			set
			{
				_outlineSettings = value;
			}
		}

		#endregion

		#region IOutlineSettings

		
		public Color OutlineColor
		{
			get
			{
				return _outlineSettings is null ? _outlineColor : _outlineSettings.OutlineColor;
			}
			set
			{
				_outlineColor = value;
			}
		}

		
		public int OutlineWidth
		{
			get
			{
				return _outlineSettings is null ? _outlineWidth : _outlineSettings.OutlineWidth;
			}
			set
			{
				_outlineWidth = Mathf.Clamp(value, OutlineResources.MinWidth, OutlineResources.MaxWidth);
			}
		}

		
		public float OutlineIntensity
		{
			get
			{
				return _outlineSettings is null ? _outlineIntensity : _outlineSettings.OutlineIntensity;
			}
			set
			{
				_outlineIntensity = Mathf.Clamp(value, OutlineResources.MinIntensity, OutlineResources.MaxIntensity);
			}
		}

		
		public float OutlineAlphaCutoff
		{
			get
			{
				return _outlineSettings is null ? _outlineAlphaCutoff : _outlineSettings.OutlineAlphaCutoff;
			}
			set
			{
				_outlineAlphaCutoff = Mathf.Clamp(value, 0, 1);
			}
		}

		
		public OutlineRenderFlags OutlineRenderMode
		{
			get
			{
				return _outlineSettings is null ? _outlineMode : _outlineSettings.OutlineRenderMode;
			}
			set
			{
				_outlineMode = value;
			}
		}

		#endregion

		#region IEquatable

		public bool Equals(IOutlineSettings other)
		{
			return OutlineSettings.Equals(this, other);
		}

		#endregion

		#region implementation
		#endregion
	}
}
