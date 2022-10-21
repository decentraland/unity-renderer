using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace UniOutline.Outline
{
	// original implementation for desktop platforms
	// Weissman 
	
	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public sealed partial class OutlineEffect : MonoBehaviour
	{
		#region data

		[SerializeField, Tooltip(OutlineResources.OutlineResourcesTooltip)]
		private OutlineResources _outlineResources;
		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField, HideInInspector]
		private CameraEvent _cameraEvent = OutlineRenderer.RenderEvent;

		private Camera _camera;
		private CommandBuffer _commandBuffer;
		private List<OutlineRenderObject> _renderObjects = new List<OutlineRenderObject>(16);

		#endregion

		#region interface

		
		// GET Resources for outline effect
		
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

		// layers collection
		public OutlineLayerCollection OutlineLayers
		{
			get
			{
				return _outlineLayers;
			}
			set
			{
				_outlineLayers = value;
			}
		}

		// outline layers
		internal OutlineLayerCollection OutlineLayersInternal => _outlineLayers;

		// getter - setter for camera event and buffers
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
					if (_commandBuffer != null)
					{
						var camera = GetComponent<Camera>();

						if (camera)
						{
							camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
							camera.AddCommandBuffer(value, _commandBuffer);
						}
					}

					_cameraEvent = value;
				}
			}
		}

		// add the game object for outline rendering
		public void AddGameObject(GameObject go)
		{
			AddGameObject(go, 0);
		}

		
		public void AddGameObject(GameObject go, int layerIndex)
		{
			if (layerIndex < 0)
			{
				throw new ArgumentOutOfRangeException("layerIndex");
			}

			CreateLayersIfNeeded();

			while (_outlineLayers.Count <= layerIndex)
			{
				_outlineLayers.Add(OutlineLayer.CreateInstance());
			}

			_outlineLayers[layerIndex].Add(go);
		}

		// remove the game object from outline rendering
		public void RemoveGameObject(GameObject go)
		{
			if (_outlineLayers)
			{
				_outlineLayers.Remove(go);
			}
		}

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			OutlineResources.LogSrpNotSupported(this);
			OutlineResources.LogPpNotSupported(this);
		}

		private void OnEnable()
		{
			InitCameraAndCommandBuffer();
		}

		private void OnDisable()
		{
			ReleaseCameraAndCommandBuffer();
		}

		private void OnPreRender()
		{
			FillCommandBuffer();
		}

		private void OnDestroy()
		{
			
			if (_outlineLayers)
			{
				_outlineLayers.Reset();
			}
		}

#if UNITY_EDITOR

		//private void OnValidate()
		//{
		//	InitCameraAndCommandBuffer();
		//	FillCommandBuffer();
		//}

		private void Reset()
		{
			_outlineLayers = null;
		}

#endif

		#endregion

		#region implementation

		private void InitCameraAndCommandBuffer()
		{
			_camera = GetComponent<Camera>();

			if (_camera && _commandBuffer is null)
			{
				_commandBuffer = new CommandBuffer
				{
					name = string.Format("{0} - {1}", GetType().Name, name)
				};

				_camera.depthTextureMode |= DepthTextureMode.Depth;
				_camera.AddCommandBuffer(_cameraEvent, _commandBuffer);
			}
		}

		private void ReleaseCameraAndCommandBuffer()
		{
			if (_commandBuffer != null)
			{
				if (_camera)
				{
					_camera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
				}

				_commandBuffer.Dispose();
				_commandBuffer = null;
			}

			_camera = null;
		}

		private void FillCommandBuffer()
		{
			if (_camera && _outlineLayers && _commandBuffer != null)
			{
				_commandBuffer.Clear();

				if (_outlineResources && _outlineResources.IsValid)
				{
					using (var renderer = new OutlineRenderer(_commandBuffer, _outlineResources, _camera.actualRenderingPath))
					{
						_renderObjects.Clear();
						_outlineLayers.GetRenderObjects(_renderObjects);
						renderer.Render(_renderObjects);
					}
				}
			}
		}

		private void CreateLayersIfNeeded()
		{
			if (_outlineLayers is null)
			{
				_outlineLayers = ScriptableObject.CreateInstance<OutlineLayerCollection>();
				_outlineLayers.name = "OutlineLayers";
			}
		}

		#endregion
	}
}
