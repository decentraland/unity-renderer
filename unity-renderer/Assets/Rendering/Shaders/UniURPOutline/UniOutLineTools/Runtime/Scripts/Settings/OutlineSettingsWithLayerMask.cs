using System;
using UnityEngine;

namespace UniOutline.Outline
{
	[Serializable]
	public class OutlineSettingsWithLayerMask : OutlineSettingsInstance
	{
		#region data

#pragma warning disable 0649

		// NOTE: There are custom editors for public components, so no need to show these in default inspector.
		[SerializeField, HideInInspector]
		private OutlineFilterMode _filterMode;
		[SerializeField, HideInInspector]
		private LayerMask _layerMask;
		[SerializeField, HideInInspector]
		private uint _renderingLayerMask = 1;

#pragma warning restore 0649

		#endregion

		#region interface

		public int OutlineLayerMask
		{
			get
			{
				if (_filterMode == OutlineFilterMode.UseLayerMask)
				{
					return _layerMask;
				}

				if (_filterMode == OutlineFilterMode.UseRenderingLayerMask)
				{
					return -1;
				}

				return 0;
			}
		}

		public uint OutlineRenderingLayerMask
		{
			get
			{
				if (_filterMode == OutlineFilterMode.UseLayerMask)
				{
					return uint.MaxValue;
				}

				if (_filterMode == OutlineFilterMode.UseRenderingLayerMask)
				{
					return _renderingLayerMask;
				}

				return 0;
			}
		}

		#endregion

		
	}
}
