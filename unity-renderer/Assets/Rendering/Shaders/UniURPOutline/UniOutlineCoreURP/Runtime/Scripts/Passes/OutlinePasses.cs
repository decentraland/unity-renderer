
using System.Collections.Generic;
using UniOutline.Outline;

using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



namespace UniOutline
{
	internal class OutlinePasses : ScriptableRenderPass
	{
		private const string _profilerTag = "OutlinePass";

		private readonly OutlineRenderFeature _renderFeature;
		private readonly List<OutlineRenderObject> _renderObjects = new List<OutlineRenderObject>();
		private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();

		private ScriptableRenderer _renderer;

		public OutlinePasses(OutlineRenderFeature renderFeature, string[] shaderTags)
		{
			_renderFeature = renderFeature;

			if (shaderTags != null && shaderTags.Length > 0)
			{
				foreach (var passName in shaderTags)
				{
					_shaderTagIdList.Add(new ShaderTagId(passName));
				}
			}
			else
			{
				_shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
				_shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
				_shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			}
		}

		public void Initialization(ScriptableRenderer renderer)
		{
			_renderer = renderer;
		}

		// from scriptable renderer
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			var outlineResources = _renderFeature.OutlineResources;
			var outlineSettings = _renderFeature.OutlineSettings;
			var camData = renderingData.cameraData;
			var depthTexture = new RenderTargetIdentifier("_CameraDepthTexture");

			if (_renderFeature.OutlineLayerMask != 0)
			{
				var cmd = CommandBufferPool.Get(_renderFeature.FeatureName);
				var filteringSettings = new FilteringSettings(RenderQueueRange.all, _renderFeature.OutlineLayerMask, _renderFeature.OutlineRenderingLayerMask);
				var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
				var sortingCriteria = camData.defaultOpaqueSortFlags;
				var drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);

				drawingSettings.enableDynamicBatching = true;
				drawingSettings.overrideMaterial = outlineResources.RenderMaterial;

				if (outlineSettings.IsAlphaTestingEnabled())
				{
					drawingSettings.overrideMaterialPassIndex = OutlineResources.RenderShaderAlphaTestPassId;
					cmd.SetGlobalFloat(outlineResources.AlphaCutoffId, outlineSettings.OutlineAlphaCutoff);
				}
				else
				{
					drawingSettings.overrideMaterialPassIndex = OutlineResources.RenderShaderDefaultPassId;
				}

				using (var renderer = new OutlineRenderer(cmd, outlineResources, _renderer.cameraColorTarget, depthTexture, camData.cameraTargetDescriptor))
				{
					renderer.RenderObjectClear(outlineSettings.OutlineRenderMode);
					context.ExecuteCommandBuffer(cmd);

					context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);

					cmd.Clear();
					renderer.RenderOutline(outlineSettings);
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}

			if (_renderFeature.OutlineLayers)
			{
				var cmd = CommandBufferPool.Get(OutlineResources.EffectName);

				using (var renderer = new OutlineRenderer(cmd, outlineResources, _renderer.cameraColorTarget, depthTexture, camData.cameraTargetDescriptor))
				{
					_renderObjects.Clear();
					_renderFeature.OutlineLayers.GetRenderObjects(_renderObjects);
					renderer.Render(_renderObjects);
				}

				context.ExecuteCommandBuffer(cmd);
				CommandBufferPool.Release(cmd);
			}
		}
	}
}
