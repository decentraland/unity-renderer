
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UniOutline.Outline
{
	
	public class OutlineRenderFeature : ScriptableRendererFeature
	{
		#region data

#pragma warning disable 0649 // disable default value warning

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField]
		private OutlineSettingsWithLayerMask _outlineSettings;
		[SerializeField]
		private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
		[SerializeField]
		public string[] _shaderPassNames;

#pragma warning restore 0649 // restore default value warning

		private OutlinePasses _outlinePasses;
		private string _featureName;

		#endregion

		#region interface

		internal OutlineResources OutlineResources => _outlineResources; // for editor

		internal OutlineLayerCollection OutlineLayers => _outlineLayers;  // for editor

		internal IOutlineSettings OutlineSettings => _outlineSettings; // for editor

		internal int OutlineLayerMask => _outlineSettings.OutlineLayerMask; // for editor

		internal uint OutlineRenderingLayerMask => _outlineSettings.OutlineRenderingLayerMask;  // for editor

		internal string FeatureName => _featureName; // for editor

		#endregion

		#region ScriptableRendererFeature

		
		// Called when the feature is created.
		public override void Create()
		{
			if (_outlineSettings != null)
			{
				_featureName = OutlineResources.EffectName + '-' + _outlineSettings.OutlineLayerMask;
			}
			else
			{
				_featureName = OutlineResources.EffectName;
			}

			_outlinePasses = new OutlinePasses(this, _shaderPassNames)
			{
				renderPassEvent = _renderPassEvent
			};
		}

		
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			if (_outlineResources && _outlineResources.IsValid)
			{
				_outlinePasses.Initialization(renderer);
				renderer.EnqueuePass(_outlinePasses);
			}
		}

		#endregion
	}
}
