#if UNITY_ANDROID
	#if USING_URP
		#define ANDROID_URP
	#endif
#endif

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//-----------------------------------------------------------------------------
// Copyright 2015-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// This script is needed to send the camera position to the stereo shader so that
	/// it can determine which eye it is rendering.  This is only needed for multi-pass
	/// rendering, as single pass has a built-in shader variable
	/// </summary>
	[AddComponentMenu("AVPro Video/Update Multi-Pass Stereo", 320)]
	[HelpURL("https://www.renderheads.com/products/avpro-video/")]
	public class UpdateMultiPassStereo : MonoBehaviour
	{
		[Header("Stereo camera")]
		[SerializeField] Camera _camera = null;

		public Camera Camera
		{
			get { return _camera; }
			set { _camera = value; }
		}

		private static readonly LazyShaderProperty PropWorldCameraPosition = new LazyShaderProperty("_WorldCameraPosition");
		private static readonly LazyShaderProperty PropWorldCameraRight = new LazyShaderProperty("_WorldCameraRight");

		// State

		private Camera _foundCamera;

		void Awake()
		{
			if (_camera == null)
			{
				Debug.LogWarning("[AVProVideo] No camera set for UpdateMultiPassStereo component. If you are rendering in multi-pass stereo then it is recommended to set this.");
			}
		}

		void Start()
		{
			LogXRDeviceDetails();

			#if ANDROID_URP
				if( GetComponent<Camera>() == null )
				{
					throw new MissingComponentException("[AVProVideo] When using URP the UpdateMultiPassStereo component must be on the Camera gameobject. This component is not required on all VR devices, but if it is then stereo eye rendering may not work correctly.");
				}
			#endif
		}

		private void LogXRDeviceDetails()
		{
#if UNITY_2019_1_OR_NEWER
			string logOutput = "[AVProVideo] XR Device details: UnityEngine.XR.XRSettings.loadedDeviceName = " + UnityEngine.XR.XRSettings.loadedDeviceName + " | supportedDevices = ";

			string[] aSupportedDevices = UnityEngine.XR.XRSettings.supportedDevices;
			int supportedDeviceCount = aSupportedDevices.Length;
			for (int i = 0; i < supportedDeviceCount; i++)
			{
				logOutput += aSupportedDevices[i];
				if( i < (supportedDeviceCount - 1 ))
				{
					logOutput += ", ";
				}
			}

			List<UnityEngine.XR.InputDevice> inputDevices = new List<UnityEngine.XR.InputDevice>();
			UnityEngine.XR.InputDevices.GetDevices(inputDevices);
			int deviceCount = inputDevices.Count;
			if (deviceCount > 0)
			{
				logOutput += " | XR Devices = ";

				for (int i = 0; i < deviceCount; i++)
				{
					logOutput += inputDevices[i].name;
					if( i < (deviceCount -1 ))
					{
						logOutput += ", ";
					}
				}
			}

			UnityEngine.XR.InputDevice headDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head);
			if( headDevice != null )
			{
				logOutput += " | headDevice name = " + headDevice.name + ", manufacturer = " + headDevice.manufacturer;
			}

			Debug.Log(logOutput);
#endif
		}


#if ANDROID_URP
		void OnEnable()
		{
			RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
		}
		void OnDisable()
		{
			RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
		}
#endif

		private static bool IsMultiPassVrEnabled()
		{
		#if UNITY_TVOS
			return false;
		#else
			#if UNITY_2017_2_OR_NEWER
			if (!UnityEngine.XR.XRSettings.enabled) return false;
			#endif
			#if UNITY_2018_3_OR_NEWER
			if (UnityEngine.XR.XRSettings.stereoRenderingMode != UnityEngine.XR.XRSettings.StereoRenderingMode.MultiPass) return false;
			#endif
			return true;
		#endif
		}


		// We do a LateUpdate() to allow for any changes in the camera position that may have happened in Update()
#if ANDROID_URP
		// Android URP
		private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
#else
		// Normal render pipeline
		private void LateUpdate()
#endif
		{
			if (!IsMultiPassVrEnabled())
			{
				return;
			}

			if (_camera != null && _foundCamera != _camera)
			{
				_foundCamera = _camera;
			}
			if (_foundCamera == null)
			{
				_foundCamera = Camera.main;
				if (_foundCamera == null)
				{
					Debug.LogWarning("[AVProVideo] Cannot find main camera for UpdateMultiPassStereo, this can lead to eyes flickering");
					if (Camera.allCameras.Length > 0)
					{
						_foundCamera = Camera.allCameras[0];
						Debug.LogWarning("[AVProVideo] UpdateMultiPassStereo using camera " + _foundCamera.name);
					}
				}
			}

			if (_foundCamera != null)
			{
				Shader.SetGlobalVector(PropWorldCameraPosition.Id, _foundCamera.transform.position);
				Shader.SetGlobalVector(PropWorldCameraRight.Id, _foundCamera.transform.right);
			}
		}
	}
}