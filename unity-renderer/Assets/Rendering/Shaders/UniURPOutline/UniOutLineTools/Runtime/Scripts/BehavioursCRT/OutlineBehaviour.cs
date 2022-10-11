using System;

using System.Collections;
using System.Collections.Generic;
using UniOutline.Outline;
using UnityEngine;
using UnityEngine.Rendering;


namespace UniOutlines.Outline
{
	
	// Attach this script to a game object to add outline effect. 
	
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	public sealed class OutlineBehaviour : MonoBehaviour, IOutlineSettings
	{
		#region data

#pragma warning disable 0649

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, HideInInspector]
		private OutlineSettingsInstance _outlineSettings;
		[SerializeField, HideInInspector]
		private int _ignoreLayerMask;
		[SerializeField, HideInInspector]
		private CameraEvent _cameraEvent = OutlineRenderer.RenderEvent;
		[SerializeField, HideInInspector]
		private Camera _targetCamera;
		[SerializeField, Tooltip("If set, list of object renderers is updated on each frame. Enable if the object has child renderers which are enabled/disabled frequently.")]
		private bool _updateRenderers;

#pragma warning restore 0649

		private Dictionary<Camera, CommandBuffer> _cameraMap = new Dictionary<Camera, CommandBuffer>();
		private List<Camera> _camerasToRemove = new List<Camera>();
		private OutlineRendererCollection _renderers;

		#endregion

		#region interface

		
		// Gets or sets resources used by the effect implementation.
		
		public OutlineResources OutlineResources
		{
			get
			{
				return _outlineResources;
			}
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException(nameof(OutlineResources));
				}

				_outlineResources = value;
			}
		}

		
		// Gets or sets outline settings. Set this to non-<see langword="null"/> value to share settings with other components.
		
		public OutlineSettings OutlineSettings
		{
			get
			{
				if (_outlineSettings == null)
				{
					_outlineSettings = new OutlineSettingsInstance();
				}

				return _outlineSettings.OutlineSettings;
			}
			set
			{
				if (_outlineSettings == null)
				{
					_outlineSettings = new OutlineSettingsInstance();
				}

				_outlineSettings.OutlineSettings = value;
			}
		}

		
		// Gets or sets layer mask to use for ignored <see cref="Renderer"/> components in this game object.
		
		public int IgnoreLayerMask
		{
			get
			{
				return _ignoreLayerMask;
			}
			set
			{
				if (_ignoreLayerMask != value)
				{
					_ignoreLayerMask = value;
					_renderers?.Reset(false, value);
				}
			}
		}

		
		// Gets or sets <see cref="CameraEvent"/> used to render the outlines.
		
		public CameraEvent RenderEvent
		{
			get
			{
				return _cameraEvent;
			}
			set
			{
				if (_cameraEvent != value)
				{
					foreach (var kvp in _cameraMap)
					{
						if (kvp.Key)
						{
							kvp.Key.RemoveCommandBuffer(_cameraEvent, kvp.Value);
							kvp.Key.AddCommandBuffer(value, kvp.Value);
						}
					}

					_cameraEvent = value;
				}
			}
		}

		
		// Gets outline renderers. By default all child <see cref="Renderer"/> components are used for outlining.
		
		public ICollection<Renderer> OutlineRenderers
		{
			get
			{
				CreateRenderersIfNeeded();
				return _renderers;
			}
		}

		
		// Gets or sets camera to render outlines to. If not set, outlines are rendered to all active cameras.
		
		public Camera Camera
		{
			get
			{
				return _targetCamera;
			}
			set
			{
				if (_targetCamera != value)
				{
					if (value)
					{
						_camerasToRemove.Clear();

						foreach (var kvp in _cameraMap)
						{
							if (kvp.Key && kvp.Key != value)
							{
								kvp.Key.RemoveCommandBuffer(_cameraEvent, kvp.Value);
								kvp.Value.Dispose();

								_camerasToRemove.Add(kvp.Key);
							}
						}

						foreach (var camera in _camerasToRemove)
						{
							_cameraMap.Remove(camera);
						}
					}

					_targetCamera = value;
				}
			}
		}

		
		// Gets all cameras outline data is rendered to.
		
		public ICollection<Camera> Cameras => _cameraMap.Keys;

		
		// Updates renderer list.
		
		public void UpdateRenderers()
		{
			_renderers?.Reset(false, _ignoreLayerMask);
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			OutlineResources.LogSrpNotSupported(this);
			OutlineResources.LogPpNotSupported(this);

			CreateRenderersIfNeeded();
			CreateSettingsIfNeeded();
		}

		private void OnEnable()
		{
			Camera.onPreRender += OnCameraPreRender;
		}

		private void OnDisable()
		{
			Camera.onPreRender -= OnCameraPreRender;

			foreach (var kvp in _cameraMap)
			{
				if (kvp.Key)
				{
					kvp.Key.RemoveCommandBuffer(_cameraEvent, kvp.Value);
				}

				kvp.Value.Dispose();
			}

			_cameraMap.Clear();
		}

		private void Update()
		{
			if (_outlineResources != null && _renderers != null)
			{
				_camerasToRemove.Clear();

				if (_updateRenderers)
				{
					_renderers.Reset(false, _ignoreLayerMask);
				}

				foreach (var kvp in _cameraMap)
				{
					var camera = kvp.Key;
					var cmdBuffer = kvp.Value;

					if (camera)
					{
						cmdBuffer.Clear();
						FillCommandBuffer(camera, cmdBuffer);
					}
					else
					{
						cmdBuffer.Dispose();
						_camerasToRemove.Add(camera);
					}
				}

				foreach (var camera in _camerasToRemove)
				{
					_cameraMap.Remove(camera);
				}
			}
		}

#if UNITY_EDITOR

		private void OnValidate()
		{
			CreateRenderersIfNeeded();
			CreateSettingsIfNeeded();
		}

		private void Reset()
		{
			if (_renderers != null)
			{
				_renderers.Reset(false, _ignoreLayerMask);
			}
		}

#endif

		#endregion

		#region IOutlineSettings

		
		public Color OutlineColor
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineColor;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineColor = value;
			}
		}

		
		public int OutlineWidth
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineWidth;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineWidth = value;
			}
		}

		
		public float OutlineIntensity
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineIntensity;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineIntensity = value;
			}
		}

		
		public float OutlineAlphaCutoff
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineAlphaCutoff;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineAlphaCutoff = value;
			}
		}

		
		public OutlineRenderFlags OutlineRenderMode
		{
			get
			{
				CreateSettingsIfNeeded();
				return _outlineSettings.OutlineRenderMode;
			}
			set
			{
				CreateSettingsIfNeeded();
				_outlineSettings.OutlineRenderMode = value;
			}
		}

		#endregion

		#region IEquatable

		
		public bool Equals(IOutlineSettings other)
		{
			return OutlineSettings.Equals(_outlineSettings, other);
		}

		#endregion

		#region implementation

		private void OnCameraPreRender(Camera camera)
		{
			if (camera && (!_targetCamera || _targetCamera == camera))
			{
				if (_outlineSettings.RequiresCameraDepth)
				{
					camera.depthTextureMode |= DepthTextureMode.Depth;
				}

				if (!_cameraMap.ContainsKey(camera))
				{
					var cmdBuf = new CommandBuffer();
					cmdBuf.name = string.Format("{0} - {1}", GetType().Name, name);
					camera.AddCommandBuffer(_cameraEvent, cmdBuf);

					_cameraMap.Add(camera, cmdBuf);
#if UNITY_EDITOR
					FillCommandBuffer(camera, cmdBuf);
#endif
				}
			}
		}

		private void FillCommandBuffer(Camera camera, CommandBuffer cmdBuffer)
		{
			if (_renderers.Count > 0)
			{
				using (var renderer = new OutlineRenderer(cmdBuffer, _outlineResources, camera.actualRenderingPath))
				{
					renderer.Render(_renderers.GetList(), _outlineSettings, name);
				}
			}
		}

		private void CreateSettingsIfNeeded()
		{
			if (_outlineSettings == null)
			{
				_outlineSettings = new OutlineSettingsInstance();
			}
		}

		private void CreateRenderersIfNeeded()
		{
			if (_renderers == null)
			{
				_renderers = new OutlineRendererCollection(gameObject);
				_renderers.Reset(false, _ignoreLayerMask);
			}
		}

		#endregion
	}
}
