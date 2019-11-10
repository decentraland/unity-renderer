//----------------------------------------------
//            Hbx: WebGL
// Copyright © 2017-2018 Hogbox Studios
// DynamicResolutionManger.cs
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hbx.WebGL
{

	/// <summary>
	/// A simple method for using the device pixel ratio to automatically scale resolution based on performance
	/// If the average fps over _interval seconds drops below a threshold we lower the quality. If that has no effect on
	/// fps then we raise it again.
	/// </summary>

	public class DynamicResolutionManger : MonoBehaviour
	{
		/// <summary>
		/// The high quality fps and pixel scale, if the fps stays above the fps value we use it's pixel scale
		/// </summary>

		public float _highFPS = 55.0f;
		public float _highPixelScale = 1.0f;

		/// <summary>
		/// The medium quality fps and pixel scale, if the fps stays above the fps value we use it's pixel scale
		/// </summary>

		public float _mediumFPS = 30.0f;
		public float _mediumPixelScale = 0.5f;

		/// <summary>
		/// The low quality fps and pixel scale, if the fps stays above the fps value we use it's pixel scale
		/// </summary>

		public float _lowFPS = 10.0f;
		public float _lowPixelScale = 0.25f;

		/// <summary>
		/// The interval in seconds to average fps over, we use an average over interval seconds to prevent brief stalls causing quality drops
		/// </summary>

		public float _interval = 5.0f;

		/// <summary>
		/// The number of frames rendered over _interval seconds
		/// </summary>

		int _intervalFrames = 0;

		/// <summary>
		/// The total of fps values captured over interval seconds
		/// </summary>

		float _intervalFpsTotal = 0;

		/// <summary>
		/// The low, medium and high values stashed in an array
		/// </summary>

		float[] _fpsThresholds = null;

		/// <summary>
		/// The low, medium and high values stashed in an array
		/// </summary>

		float[] _pixelScales = null;

		/// <summary>
		/// The current quality level index into the above arrays
		/// </summary>

		int _activeQualityLevel;

		/// <summary>
		/// The previous quality levels fps, use this to see if lowering pixel scale has any effect
		/// </summary>

		float _previousAvgFPS = 60f;

		/// <summary>
		/// Used to track fps
		/// </summary>

		float _deltaTime = 0f;

		/// <summary>
		/// The time we started tracking fps
		/// </summary>

		float _startTick = 0f;


		/// <summary>
		/// Capture our performance brackets and set the start quality level
		/// </summary>

		void Awake()
		{
			// stash the editor values in out arrays
			_fpsThresholds = new float[] { _lowFPS, _mediumFPS, _highFPS };
			_pixelScales = new float[] { _lowPixelScale, _mediumPixelScale, _highPixelScale };
			// reset to the max quality level
			//ResetQuality();
		}
		
		/// <summary>
		/// Keep monitoring fps, if it drops below the active qaulity levels fps threshold then we drop quality level
		/// </summary>

		void Update()
		{
			// calc fps
			_deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
			float msec = _deltaTime * 1000.0f;
			float fps = 1.0f / _deltaTime;

			// increment our total frames and fps values
			_intervalFpsTotal += fps;
			_intervalFrames++;
			
			// wait for the interval to pass before checking fps
			if((Time.time-_startTick) < _interval) return;

			float avgfps = _intervalFpsTotal / _intervalFrames;

			// if the fps has dropped too low then drop quality level
			if(_activeQualityLevel > 0 && avgfps < _fpsThresholds[_activeQualityLevel])
			{
				_previousAvgFPS = avgfps;
				_activeQualityLevel--;
				SetQualityLevel(_activeQualityLevel);
			}
			// if the fps is no better than before raise the quality level
			float fpsdif = avgfps - _previousAvgFPS;
			if(_activeQualityLevel < _fpsThresholds.Length-1 && fpsdif < 0f)
			{
				_previousAvgFPS = avgfps;
				_activeQualityLevel++;
				SetQualityLevel(_activeQualityLevel);
			}

			// reset interval values
			_intervalFpsTotal = 0f;
			_intervalFrames = 0;
			_startTick = Time.time;
		}

		/// <summary>
		/// Resets the quality level to maximum.
		/// </summary>

		public void ResetQuality()
		{
			SetQualityLevel(_fpsThresholds.Length-1);
		}

		/// <summary>
		/// Set pixel scale to specific quality level
		/// </summary>
		/// <param name="aLevel">A quality level.</param>

		public void SetQualityLevel(int aLevel)
		{
			aLevel = Mathf.Clamp(aLevel, 0, _fpsThresholds.Length-1);
			_activeQualityLevel = aLevel;
			DevicePixelRatio.ScaleDevicePixelRatio(_pixelScales[_activeQualityLevel]);

			_startTick = Time.time;
			_deltaTime = 0f;
		}
	}

} // end Hbx WebGL namespace
