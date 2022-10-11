using System;
using System.Collections.Generic;

using System.Runtime.CompilerServices;
using UniOutlines.Outline;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.XR;

using UniOutline.Outline;

namespace UniOutline.Outline
{
	// original implementation for desktop platforms
	// Weissman 
	public readonly struct OutlineRenderer : IDisposable
	{
		#region data

		private readonly TextureDimension _rtDimention;
		private readonly RenderTargetIdentifier _rt;
		private readonly RenderTargetIdentifier _depth;
		private readonly CommandBuffer _commandBuffer; 
		private readonly OutlineResources _resources;

		#endregion

		#region interface

		// render target dimension
		public const CameraEvent RenderEvent = CameraEvent.AfterImageEffects;

		// render target texture
		public const RenderTextureFormat RtFormat = RenderTextureFormat.R8;

		// struct instance
		public OutlineRenderer(CommandBuffer cmd, OutlineResources resources)
			: this(cmd, resources, BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.Depth, Vector2Int.zero)
		{
		}

		// initialize struct instance
		public OutlineRenderer(CommandBuffer cmd, OutlineResources resources, RenderingPath renderingPath)
			: this(cmd, resources, BuiltinRenderTextureType.CameraTarget, GetBuiltinDepth(renderingPath), Vector2Int.zero)
		{
		}

		// initialize struct instance
		public OutlineRenderer(CommandBuffer cmd, OutlineResources resources, RenderTargetIdentifier dst)
			: this(cmd, resources, dst, BuiltinRenderTextureType.Depth, Vector2Int.zero)
		{
		}

		// initialize struct instance
		public OutlineRenderer(CommandBuffer cmd, OutlineResources resources, RenderTargetIdentifier dst, RenderingPath renderingPath, Vector2Int rtSize)
			: this(cmd, resources, dst, GetBuiltinDepth(renderingPath), rtSize)
		{
		}

		// initialize renderer
		public OutlineRenderer(CommandBuffer cmd, OutlineResources resources, RenderTargetIdentifier dst, RenderTargetIdentifier depth, Vector2Int rtSize)
		{
			if (cmd is null)
			{
				throw new ArgumentNullException(nameof(cmd));
			}

			if (resources is null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			if (rtSize.x <= 0)
			{
				rtSize.x = -1;
			}

			if (rtSize.y <= 0)
			{
				rtSize.y = -1;
			}

			if (XRSettings.enabled)
			{
				var rtDesc = XRSettings.eyeTextureDesc;

				rtDesc.shadowSamplingMode = ShadowSamplingMode.None;
				rtDesc.depthBufferBits = 0;
				rtDesc.colorFormat = RtFormat;

				cmd.GetTemporaryRT(resources.MaskTexId, rtDesc, FilterMode.Bilinear);
				cmd.GetTemporaryRT(resources.TempTexId, rtDesc, FilterMode.Bilinear);

				_rtDimention = rtDesc.dimension;
			}
			else
			{
				cmd.GetTemporaryRT(resources.MaskTexId, rtSize.x, rtSize.y, 0, FilterMode.Bilinear, RtFormat);
				cmd.GetTemporaryRT(resources.TempTexId, rtSize.x, rtSize.y, 0, FilterMode.Bilinear, RtFormat);

				_rtDimention = TextureDimension.Tex2D;
			}

			_rt = dst;
			_depth = depth;
			_commandBuffer = cmd;
			_resources = resources;
		}

		// render outline
		public OutlineRenderer(CommandBuffer cmd, OutlineResources resources, RenderTargetIdentifier dst, RenderTargetIdentifier depth, RenderTextureDescriptor rtDesc)
		{
			if (cmd is null)
			{
				throw new ArgumentNullException(nameof(cmd));
			}

			if (resources is null)
			{
				throw new ArgumentNullException(nameof(resources));
			}

			if (rtDesc.width <= 0)
			{
				rtDesc.width = -1;
			}

			if (rtDesc.height <= 0)
			{
				rtDesc.height = -1;
			}

			if (rtDesc.dimension == TextureDimension.None || rtDesc.dimension == TextureDimension.Unknown)
			{
				rtDesc.dimension = TextureDimension.Tex2D;
			}

			rtDesc.shadowSamplingMode = ShadowSamplingMode.None;
			rtDesc.depthBufferBits = 0;
			rtDesc.colorFormat = RtFormat;
			rtDesc.msaaSamples = 1;

			cmd.GetTemporaryRT(resources.MaskTexId, rtDesc, FilterMode.Bilinear);
			cmd.GetTemporaryRT(resources.TempTexId, rtDesc, FilterMode.Bilinear);

			_rtDimention = rtDesc.dimension;
			_rt = dst;
			_depth = depth;
			_commandBuffer = cmd;
			_resources = resources;
		}

		
		// Renders outline around a single object.
		
		public void Render(OutlineRenderObject obj)
		{
			Render(obj.Renderers, obj.OutlineSettings, obj.Tag);
		}

		// render the outline
		public void Render(IReadOnlyList<OutlineRenderObject> objects)
		{
			if (objects is null)
			{
				throw new ArgumentNullException(nameof(objects));
			}

			for (var i = 0; i < objects.Count; i++)
			{
				Render(objects[i]);
			}
		}

		// multiple render
		public void Render(IReadOnlyList<Renderer> renderers, IOutlineSettings settings, string sampleName = null)
		{
			if (renderers is null)
			{
				throw new ArgumentNullException(nameof(renderers));
			}

			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			if (renderers.Count > 0)
			{
				
				{
					RenderObjectClear(settings.OutlineRenderMode);

					for (var i = 0; i < renderers.Count; ++i)
					{
						DrawRenderer(renderers[i], settings);
					}

					RenderOutline(settings);
				}
				//_commandBuffer.EndSample(sampleName);
			}
		}

		// single render
		public void Render(Renderer renderer, IOutlineSettings settings, string sampleName = null)
		{
			if (renderer is null)
			{
				throw new ArgumentNullException(nameof(renderer));
			}

			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			
			{
				RenderObjectClear(settings.OutlineRenderMode);
				DrawRenderer(renderer, settings);
				RenderOutline(settings);
			}
			//_commandBuffer.EndSample(sampleName);
		}

		// render target
		public void RenderObjectClear(OutlineRenderFlags flags)
		{
			// NOTE: Use the camera depth buffer when rendering the mask. Shader only reads from the depth buffer (ZWrite Off).
			if ((flags & OutlineRenderFlags.EnableDepthTesting) != 0)
			{
				if (_rtDimention == TextureDimension.Tex2DArray)
				{
					// NOTE: Need to use this SetRenderTarget overload for XR, otherwise single pass instanced rendering does not function properly.
					_commandBuffer.SetRenderTarget(_resources.MaskTex, _depth, 0, CubemapFace.Unknown, -1);
				}
				else
				{
					_commandBuffer.SetRenderTarget(_resources.MaskTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, _depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
				}
			}
			else
			{
				if (_rtDimention == TextureDimension.Tex2DArray)
				{
					_commandBuffer.SetRenderTarget(_resources.MaskTex, 0, CubemapFace.Unknown, -1);
				}
				else
				{
					_commandBuffer.SetRenderTarget(_resources.MaskTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
				}
			}

			_commandBuffer.ClearRenderTarget(false, true, Color.clear);
		}

		// render outline
		public void RenderOutline(IOutlineSettings settings)
		{
			var mat = _resources.OutlineMaterial;
			var props = _resources.GetProperties(settings);

			_commandBuffer.SetGlobalFloatArray(_resources.GaussSamplesId, _resources.GetGaussSamples(settings.OutlineWidth));

			if (_rtDimention == TextureDimension.Tex2DArray)
			{
				// HPass
				_commandBuffer.SetRenderTarget(_resources.TempTex, 0, CubemapFace.Unknown, -1);
				Blit(_resources.MaskTex, OutlineResources.OutlineShaderHPassId, mat, props);

				// VPassBlend
				_commandBuffer.SetRenderTarget(_rt, 0, CubemapFace.Unknown, -1);
				Blit(_resources.TempTex, OutlineResources.OutlineShaderVPassId, mat, props);
			}
			else
			{
				// HPass
				_commandBuffer.SetRenderTarget(_resources.TempTex, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
				Blit(_resources.MaskTex, OutlineResources.OutlineShaderHPassId, mat, props);

				// VPassBlend
				_commandBuffer.SetRenderTarget(_rt, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
				Blit(_resources.TempTex, OutlineResources.OutlineShaderVPassId, mat, props);
			}
		}

		#endregion

		#region IDisposable

		// release textures
		public void Dispose()
		{
			_commandBuffer.ReleaseTemporaryRT(_resources.TempTexId);
			_commandBuffer.ReleaseTemporaryRT(_resources.MaskTexId);
		}

		#endregion

		#region implementation

		private void DrawRenderer(Renderer renderer, IOutlineSettings settings)
		{
			if (renderer && renderer.enabled && renderer.isVisible && renderer.gameObject.activeInHierarchy)
			{
				// NOTE: Accessing Renderer.sharedMaterials triggers GC.Alloc. That's why we use a temporary
				// list of materials, cached with the outline resources.
				renderer.GetSharedMaterials(_resources.TmpMaterials);

				if (_resources.TmpMaterials.Count > 0)
				{
					if (settings.IsAlphaTestingEnabled())
					{
						for (var i = 0; i < _resources.TmpMaterials.Count; ++i)
						{
							var mat = _resources.TmpMaterials[i];

							// Use material cutoff value if available.
							if (mat.HasProperty(_resources.AlphaCutoffId))
							{
								_commandBuffer.SetGlobalFloat(_resources.AlphaCutoffId, mat.GetFloat(_resources.AlphaCutoffId));
							}
							else
							{
								_commandBuffer.SetGlobalFloat(_resources.AlphaCutoffId, settings.OutlineAlphaCutoff);
							}

							_commandBuffer.SetGlobalTexture(_resources.MainTexId, _resources.TmpMaterials[i].mainTexture);
							_commandBuffer.DrawRenderer(renderer, _resources.RenderMaterial, i, OutlineResources.RenderShaderAlphaTestPassId);
						}
					}
					else
					{
						for (var i = 0; i < _resources.TmpMaterials.Count; ++i)
						{
							_commandBuffer.DrawRenderer(renderer, _resources.RenderMaterial, i, OutlineResources.RenderShaderDefaultPassId);
						}
					}
				}
				else
				{
					// NOTE: No materials set for renderer means we should still render outline for it.
					_commandBuffer.DrawRenderer(renderer, _resources.RenderMaterial, 0, OutlineResources.RenderShaderDefaultPassId);
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void Blit(RenderTargetIdentifier src, int shaderPass, Material mat, MaterialPropertyBlock props)
		{
			// Set source texture as _MainTex to match Blit behavior.
			_commandBuffer.SetGlobalTexture(_resources.MainTexId, src);

			// NOTE: SystemInfo.graphicsShaderLevel check is not enough sometimes (esp. on mobiles), so there is SystemInfo.supportsInstancing
			// check and a flag for forcing DrawMesh.
			if (SystemInfo.graphicsShaderLevel >= 35 && SystemInfo.supportsInstancing && !_resources.UseFullscreenTriangleMesh)
			{
				_commandBuffer.DrawProcedural(Matrix4x4.identity, mat, shaderPass, MeshTopology.Triangles, 3, 1, props);
			}
			else
			{
				_commandBuffer.DrawMesh(_resources.FullscreenTriangleMesh, Matrix4x4.identity, mat, 0, shaderPass, props);
			}
		}

		private static RenderTargetIdentifier GetBuiltinDepth(RenderingPath renderingPath)
		{
			return (renderingPath == RenderingPath.DeferredShading || renderingPath == RenderingPath.DeferredLighting) ? BuiltinRenderTextureType.ResolvedDepth : BuiltinRenderTextureType.Depth;
		}

		#endregion
	}
}
