
using UnityEngine;


namespace UniOutline.Outline
{
	
	// Outline settings.
	
	[CreateAssetMenu(fileName = "OutlineSettings", menuName = "UniOutline/Outline/Outline Settings")]
	public sealed class OutlineSettings : ScriptableObject, IOutlineSettings
	{
		#region data

		// NOTE: There is a custom editor for OutlineSettings, so no need to show these in default inspector.
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

		#endregion

		#region interface

		public static bool Equals(IOutlineSettings lhs, IOutlineSettings rhs)
		{
			if (lhs == null || rhs == null)
			{
				return false;
			}

			return lhs.OutlineColor == rhs.OutlineColor &&
				lhs.OutlineWidth == rhs.OutlineWidth &&
				lhs.OutlineRenderMode == rhs.OutlineRenderMode &&
				Mathf.Approximately(lhs.OutlineIntensity, rhs.OutlineIntensity) &&
				Mathf.Approximately(lhs.OutlineAlphaCutoff, rhs.OutlineAlphaCutoff);
		}

		#endregion

		#region IOutlineSettings

		
		public Color OutlineColor
		{
			get
			{
				return _outlineColor;
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
				return _outlineWidth;
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
				return _outlineIntensity;
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
				return _outlineAlphaCutoff;
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
				return _outlineMode;
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
			return Equals(this, other);
		}

		#endregion

		#region Object

		
		public override bool Equals(object other)
		{
			return Equals(this, other as IOutlineSettings);
		}

		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
