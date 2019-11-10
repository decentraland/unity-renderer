//----------------------------------------------
//            Hbx: WebGL
// Copyright © 2017-2018 Hogbox Studios
// DevicePixelRatio.cs
//----------------------------------------------

using System.Runtime.InteropServices;
using UnityEngine;

namespace Hbx.WebGL
{

	/// <summary>
	/// Device pixel ratio class, provides binding to the hbx_WebGL_DevicePixelRatio.jslib native plugin
	/// allows user to control device pixel ratio at run time. The WebGLRetinaTools Fix must have been applied
	/// to the project otherwise this class will have no effect at runtime
	/// </summary>
    
	public static class DevicePixelRatio
	{

		#region Native bindings		

		#if UNITY_WEBGL && !UNITY_EDITOR
		[DllImport("__Internal")]
	    private static extern float hbx_WebGL_GetWindowDevicePixelRatio();
		#else
		private static float hbx_WebGL_GetWindowDevicePixelRatio() { return 1f; }
		#endif

		#if UNITY_WEBGL && !UNITY_EDITOR
		[DllImport("__Internal")]
	    private static extern float hbx_WebGL_GetDevicePixelRatio();
		#else
		private static float hbx_WebGL_GetDevicePixelRatio() { return 1f; }
		#endif

		#if UNITY_WEBGL && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern void hbx_WebGL_SetDevicePixelRatio(float dpr);
		#else
		private static void hbx_WebGL_SetDevicePixelRatio(float dpr) { }
		#endif

		#if UNITY_WEBGL && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern void hbx_WebGL_ScaleDevicePixelRatio(float scale);
		#else
		private static void hbx_WebGL_ScaleDevicePixelRatio(float scale) { }
		#endif

		#endregion

		/// <summary>
		/// Returns the window.devicePixelRatio value used in javascript
		/// </summary>
		/// <returns>The window device pixel ratio.</returns>

		public static float GetWindowDevicePixelRatio() { return hbx_WebGL_GetWindowDevicePixelRatio(); }

		/// <summary>
		/// Gets the device pixel ratio used by the Retina fix, the value actually used when calculating canvas dimensions.
		/// </summary>
		/// <returns>The device pixel ratio.</returns>

		public static float GetDevicePixelRatio() { return hbx_WebGL_GetDevicePixelRatio(); }

		/// <summary>
		/// Set the device pixel ratio used to calculate canvas dimensions
		/// </summary>
		/// <param name="aDpr">A pixel ratio.</param>

		public static void SetDevicePixelRatio(float aDpr) { hbx_WebGL_SetDevicePixelRatio(aDpr); }


		/// <summary>
		/// Sets the device pixel ratio by scaling/multiplying the original window dpr
		/// </summary>
		/// <param name="aScale">A scale.</param>

		public static void ScaleDevicePixelRatio(float aScale) { hbx_WebGL_ScaleDevicePixelRatio(aScale); }

		
	}

} // end Hbx WebGL namespace
