using System;

using System.Collections.Generic;
using System.Diagnostics;

using UnityEditor;
using UnityEngine;

using UnityEngine.Rendering;


namespace UniOutline.Outline
{
	// runtime resources
	[CreateAssetMenu(fileName = "OutlineResources", menuName = "UniOutline/Outline/Outline Resources")]
	public sealed class OutlineResources : ScriptableObject
	{
		#region data

		[SerializeField]
		private Shader _renderShader;
		[SerializeField]
		private Shader _outlineShader;

		private Material _renderMaterial;
		private Material _outlineMaterial;
		private MaterialPropertyBlock _props;
		private Mesh _fullscreenTriangleMesh;
		private float[][] _gaussSamples;
		private bool _useDrawMesh;

		#endregion

		#region interface

		// outline min width
		public const int MinWidth = 1;

		// max width
		public const int MaxWidth = 32;

		// min intensity
		public const int MinIntensity = 1;

		// max intensity
		public const int MaxIntensity = 64;

		// solid color intensity
		public const int SolidIntensity = 100;

		// min aplpha cut off
		public const float MinAlphaCutoff = 0;

		// max alpha cut off
		public const float MaxAlphaCutoff = 1;

		// main tex
		public const string MainTexName = "_MainTex";

		// mask tex
		public const string MaskTexName = "_MaskTex";

		// temp tex
		public const string TempTexName = "_TempTex";

		// shader color
		public const string ColorName = "_Color";

		// shader width
		public const string WidthName = "_Width";

		
		// Name of _Intensity shader parameter.
		
		public const string IntensityName = "_Intensity";

		
		// Name of _Cutoff shader parameter.
		
		public const string AlphaCutoffName = "_Cutoff";

		
		// Name of _GaussSamples shader parameter.
		
		public const string GaussSamplesName = "_GaussSamples";

		
		// Name of the _USE_DRAWMESH shader feature.
		
		public const string UseDrawMeshFeatureName = "_USE_DRAWMESH";

		
		// Name of the outline effect.
		
		public const string EffectName = "Outline";

		
		// Tooltip text 
		
		public const string OutlineResourcesTooltip = "Outline resources to use (shaders, materials etc)";

		
		// Tooltip text for  field.
		
		public const string OutlineLayerCollectionTooltip = "Collection of outline layers to use. Multiple cameras";

		
		// Tooltip text for outline field.
		
		public const string OutlineLayerMaskTooltip = "Layer mask for outined objects.";

		
		// Tooltip text for outline <field.
		
		public const string OutlineRenderingLayerMaskTooltip = "Rendering layer mask for outined objects.";

		
		// Index of the default pass in the outline shader.
		
		public const int RenderShaderDefaultPassId = 0;

		
		// Index of the alpha-test pass 
		
		public const int RenderShaderAlphaTestPassId = 1;

		
		// Index of the HPass 
		
		public const int OutlineShaderHPassId = 0;

		
		// Index of the VPass in <
		
		public const int OutlineShaderVPassId = 1;

		
		// SRP not supported message.
		
		internal const string SrpNotSupported = "{0} works with built-in render pipeline only. It does not support SRP (including URP and HDRP).";

		
		// Post-processing not supported message.
		
		internal const string PpNotSupported = "{0} does not support Unity Post-processing stack v2. It might not work as expected.";

		
		// Hashed name of _MainTex shader parameter.
		
		public readonly int MainTexId = Shader.PropertyToID(MainTexName);

		
		// Texture identifier for _MainTex shader parameter.
		
		public readonly RenderTargetIdentifier MainTex = new RenderTargetIdentifier(MainTexName);

		
		// Hashed name of _MaskTex shader parameter.
		
		public readonly int MaskTexId = Shader.PropertyToID(MaskTexName);

		
		// Texture identifier for _MaskTex shader parameter.
		
		public readonly RenderTargetIdentifier MaskTex = new RenderTargetIdentifier(MaskTexName);

		
		// Hashed name of _TempTex shader parameter.
		
		public readonly int TempTexId = Shader.PropertyToID(TempTexName);

		
		// Texture identifier for _TempTex shader parameter.
		
		public readonly RenderTargetIdentifier TempTex = new RenderTargetIdentifier(TempTexName);

		
		// Hashed name of _Color shader parameter.
		
		public readonly int ColorId = Shader.PropertyToID(ColorName);

		
		// Hashed name of _Width shader parameter.
		
		public readonly int WidthId = Shader.PropertyToID(WidthName);

		
		// Hashed name of _Intensity shader parameter.
		
		public readonly int IntensityId = Shader.PropertyToID(IntensityName);

		
		// Hashed name of _Cutoff shader parameter.
		
		public readonly int AlphaCutoffId = Shader.PropertyToID(AlphaCutoffName);

		
		// Hashed name of _GaussSamples shader parameter.
		
		public readonly int GaussSamplesId = Shader.PropertyToID(GaussSamplesName);

		
		// Temp materials list. Used by <see cref="OutlineRenderer"/> to avoid GC allocations.
		
		internal readonly List<Material> TmpMaterials = new List<Material>();

		
		// Gets a <see cref="Shader"/> that renders objects outlined with a solid while color.
		
		public Shader RenderShader
		{
			get
			{
				return _renderShader;
			}
		}

		
		// Gets a shader that renders outline around the mask, that was generated with <see cref="RenderShader"/>.
		
		public Shader OutlineShader
		{
			get
			{
				return _outlineShader;
			}
		}

		
		// Gets a based material.
		
		public Material RenderMaterial
		{
			get
			{
				if (_renderMaterial == null)
				{
					UnityEngine.Debug.Assert(_renderShader != null, "No RenderShader is set in outline resources.", this);

					_renderMaterial = new Material(_renderShader)
					{
						name = "Outline - RenderColor",
						hideFlags = HideFlags.HideAndDontSave
					};
				}

				return _renderMaterial;
			}
		}

		
		// Gets a <see cref="OutlineShader"/>-based material.
		
		public Material OutlineMaterial
		{
			get
			{
				if (_outlineMaterial == null)
				{
					UnityEngine.Debug.Assert(_outlineShader != null, "No OutlineShader is set in outline resources.", this);

					_outlineMaterial = new Material(_outlineShader)
					{
						name = "Outline - Main",
						hideFlags = HideFlags.HideAndDontSave
					};

					if (_useDrawMesh)
					{
						_outlineMaterial.EnableKeyword(UseDrawMeshFeatureName);
					}
				}

				return _outlineMaterial;
			}
		}

		
		// blend material
		
		public MaterialPropertyBlock Properties
		{
			get
			{
				if (_props is null)
				{
					_props = new MaterialPropertyBlock();
				}

				return _props;
			}
		}

		
		// Gets or sets a fullscreen triangle mesh. The mesh is lazy-initialized on the first access.
		
		// <remarks>
		// This is used by OutlineRenderer to avoid Blit calls and use DrawMesh passing
		// this mesh as the first argument. When running on a device with Shader Model 3.5 support this
		// should not be used at all, as the vertices are generated in vertex shader with DrawProcedural() call.
		// </remarks>
		
		public Mesh FullscreenTriangleMesh
		{
			get
			{
				if (_fullscreenTriangleMesh == null)
				{
					_fullscreenTriangleMesh = new Mesh()
					{
						name = "Outline - FullscreenTriangle",
						hideFlags = HideFlags.HideAndDontSave,
						vertices = new Vector3[] { new Vector3(-1, -1, 0), new Vector3(3, -1, 0), new Vector3(-1, 3, 0) },
						triangles = new int[] { 0, 1, 2 }
					};

					_fullscreenTriangleMesh.UploadMeshData(true);
				}

				return _fullscreenTriangleMesh;
			}
			set
			{
				_fullscreenTriangleMesh = value;
			}
		}

		
		// Gets or sets a value indicating whether <see cref="FullscreenTriangleMesh"/> is used for image effects rendering even when procedural rendering is available.
		
		public bool UseFullscreenTriangleMesh
		{
			get
			{
				return _useDrawMesh;
			}
			set
			{
				if (_useDrawMesh != value)
				{
					_useDrawMesh = value;

					if (_outlineMaterial)
					{
						if (_useDrawMesh)
						{
							_outlineMaterial.EnableKeyword(UseDrawMeshFeatureName);
						}
						else
						{
							_outlineMaterial.DisableKeyword(UseDrawMeshFeatureName);
						}
					}
				}
			}
		}

		
		// Gets a value indicating whether the instance is in valid state.
		
		public bool IsValid => RenderShader && OutlineShader;

		
		// Returns a <see cref="MaterialPropertyBlock"/> instance initialized with values from <paramref name="settings"/>.
		
		public MaterialPropertyBlock GetProperties(IOutlineSettings settings)
		{
			if (_props is null)
			{
				_props = new MaterialPropertyBlock();
			}

			_props.SetFloat(WidthId, settings.OutlineWidth);
			_props.SetColor(ColorId, settings.OutlineColor);

			if ((settings.OutlineRenderMode & OutlineRenderFlags.Blurred) != 0)
			{
				_props.SetFloat(IntensityId, settings.OutlineIntensity);
			}
			else
			{
				_props.SetFloat(IntensityId, SolidIntensity);
			}

			return _props;
		}

		
		// Gets cached gauss samples for the specified outline <paramref name="width"/>.
		
		public float[] GetGaussSamples(int width)
		{
			var index = Mathf.Clamp(width, 1, MaxWidth) - 1;

			if (_gaussSamples is null)
			{
				_gaussSamples = new float[MaxWidth][];
			}

			if (_gaussSamples[index] is null)
			{
				_gaussSamples[index] = GetGaussSamples(width, null);
			}

			return _gaussSamples[index];
		}

		
		// Resets the resources to defaults.
		
		public void ResetToDefaults()
		{
			_renderShader = Shader.Find("Hidden/UnityFx/OutlineColor");
			_outlineShader = Shader.Find("Hidden/UnityFx/Outline");
		}

		
		// Calculates value of Gauss function for the specified <paramref name="x"/> and <paramref name="stdDev"/> values.
		
		// <seealso href="https://en.wikipedia.org/wiki/Gaussian_blur"/>
		// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
		public static float Gauss(float x, float stdDev)
		{
			var stdDev2 = stdDev * stdDev * 2;
			var a = 1 / Mathf.Sqrt(Mathf.PI * stdDev2);
			var gauss = a * Mathf.Pow((float)Math.E, -x * x / stdDev2);

			return gauss;
		}

		
		// Samples Gauss function for the specified <paramref name="width"/>.
		
		// <seealso href="https://en.wikipedia.org/wiki/Normal_distribution"/>
		public static float[] GetGaussSamples(int width, float[] samples)
		{
			// NOTE: According to '3 sigma' rule there is no reason to have StdDev less then width / 3.
			// In practice blur looks best when StdDev is within range [width / 3,  width / 2].
			var stdDev = width * 0.5f;

			if (samples is null)
			{
				samples = new float[MaxWidth];
			}

			for (var i = 0; i < width; i++)
			{
				samples[i] = Gauss(i, stdDev);
			}

			return samples;
		}

		
		// Writes a console warning if SRP is detected.
		
		public static void LogSrpNotSupported(UnityEngine.Object obj)
		{
			if (GraphicsSettings.renderPipelineAsset)
			{
				UnityEngine.Debug.LogWarningFormat(obj, SrpNotSupported, obj.GetType().Name);
			}
		}

		
		// Writes a console warning if Post Processing Stack v2 is detected.
		
		[Conditional("UNITY_POST_PROCESSING_STACK_V2")]
		public static void LogPpNotSupported(UnityEngine.Object obj)
		{
			UnityEngine.Debug.LogWarningFormat(obj, PpNotSupported, obj.GetType().Name);
		}

		#endregion

		#region ScriptableObject

		private void OnValidate()
		{
			if (_renderMaterial)
			{
				_renderMaterial.shader = _renderShader;
			}

			if (_outlineMaterial)
			{
				_outlineMaterial.shader = _outlineShader;
			}
		}

		#endregion
	}
}
