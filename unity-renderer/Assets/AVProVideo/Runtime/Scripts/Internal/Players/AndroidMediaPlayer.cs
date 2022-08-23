﻿// NOTE: We only allow this script to compile in editor so we can easily check for compilation issues
#if (UNITY_EDITOR || UNITY_ANDROID)

#define AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
#define DLL_METHODS

using UnityEngine;
using System;
using System.Text;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2015-2021 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	/// <summary>
	/// Android implementation of BaseMediaPlayer
	/// </summary>
	// TODO: seal this class
	public class AndroidMediaPlayer : BaseMediaPlayer
	{
		protected static AndroidJavaObject	s_ActivityContext	= null;
		protected static AndroidJavaObject  s_Interface			= null;
		protected static bool				s_bInitialised		= false;

		private static string				s_Version = "Plug-in not yet initialised";

		private static System.IntPtr 		_nativeFunction_RenderEvent = System.IntPtr.Zero;

		protected AndroidJavaObject			m_Video;
		private Texture2D					m_Texture;
		private int                         m_TextureHandle;
		private bool						m_UseFastOesPath;

//		private string						m_AuthToken;

		private double						m_Duration			= 0.0;
		private int							m_Width				= 0;
		private int							m_Height			= 0;

		protected int 						m_iPlayerIndex		= -1;

		private Android.VideoApi			m_API;
		private bool						m_HeadRotationEnabled = false;
		private bool						m_FocusEnabled = false;
		private System.IntPtr 				m_Method_Update;
		private System.IntPtr 				m_Method_SetHeadRotation;
		private System.IntPtr				m_Method_GetCurrentTimeS;
		private System.IntPtr				m_Method_GetSourceVideoFrameRate;
		private System.IntPtr				m_Method_IsPlaying;
		private System.IntPtr				m_Method_IsPaused;
		private System.IntPtr				m_Method_IsFinished;
		private System.IntPtr				m_Method_IsSeeking;
		private System.IntPtr				m_Method_IsBuffering;
		private System.IntPtr				m_Method_IsLooping;
		private System.IntPtr				m_Method_HasVideo;
		private System.IntPtr				m_Method_HasAudio;
		private System.IntPtr               m_Method_HasMetaData;
		private System.IntPtr				m_Method_SetFocusProps;
		private System.IntPtr				m_Method_SetFocusEnabled;
		private System.IntPtr				m_Method_SetFocusRotation;
		private jvalue[]					m_Value0 = new jvalue[0];
		private jvalue[]					m_Value1 = new jvalue[1];
		private jvalue[]					m_Value2 = new jvalue[2];
		private jvalue[]					m_Value4 = new jvalue[4];

		private MediaPlayer.OptionsAndroid	m_Options;

		private enum NativeStereoPacking : int
		{
			Unknown = -1,		// Unknown
			Monoscopic = 0,		// Monoscopic
			TopBottom = 1,		// Top is the left eye, bottom is the right eye
			LeftRight = 2,		// Left is the left eye, right is the right eye
			Mesh = 3,			// Use the mesh UV to unpack, uv0=left eye, uv1=right eye
		}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
		private int _textureQuality = QualitySettings.masterTextureLimit;
#endif
		static private System.Threading.Thread		m_MainThread;

		public static bool InitialisePlatform()
		{
			m_MainThread = System.Threading.Thread.CurrentThread;

			// Get the activity context
			if (s_ActivityContext == null)
			{
				AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				if (activityClass != null)
				{
					s_ActivityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
				}
			}

			if (!s_bInitialised)
			{
				s_Interface = new AndroidJavaObject("com.renderheads.AVPro.Video.Manager");
				if (s_Interface != null)
				{
					s_Version = s_Interface.Call<string>("GetPluginVersion");
					s_Interface.Call("SetContext", s_ActivityContext);

					// Calling this native function cause the .SO library to become loaded
					// This is important for Unity < 5.2.0 where GL.IssuePluginEvent works differently
					_nativeFunction_RenderEvent = Native.GetRenderEventFunc();
					if (_nativeFunction_RenderEvent != IntPtr.Zero)
					{
						s_bInitialised = true;
					}
				}
			}

			return s_bInitialised;
		}

		public static void DeinitPlatform()
		{
			if (s_bInitialised)
			{
				if (s_Interface != null)
				{
					s_Interface.CallStatic("Deinitialise");
					s_Interface = null;
				}
				s_ActivityContext = null;
				s_bInitialised = false;
			}
		}

		private static void IssuePluginEvent(Native.AVPPluginEvent type, int param)
		{
			// Build eventId from the type and param.
			int eventId = 0x5d5ac000 | ((int)type << 8);

			switch (type)
			{
				case Native.AVPPluginEvent.PlayerSetup:
				case Native.AVPPluginEvent.PlayerUpdate:
				case Native.AVPPluginEvent.PlayerDestroy:
				case Native.AVPPluginEvent.ExtractFrame:
					{
						eventId |= param & 0xff;
					}
					break;
			}

			GL.IssuePluginEvent(_nativeFunction_RenderEvent, eventId);
		}

		private System.IntPtr GetMethod(string methodName, string signature)
		{
			Debug.Assert(m_Video != null);
			System.IntPtr result = AndroidJNIHelper.GetMethodID(m_Video.GetRawClass(), methodName, signature, false);

			Debug.Assert(result != System.IntPtr.Zero);
			if (result == System.IntPtr.Zero)
			{
				Debug.LogError("[AVProVideo] Unable to get method " + methodName + " " + signature);
				throw new System.Exception("[AVProVideo] Unable to get method " + methodName + " " + signature);
			}

			return result;
		}

		public AndroidMediaPlayer(MediaPlayer.OptionsAndroid options) : base()
		{
			Debug.Assert(s_Interface != null);
			Debug.Assert(s_bInitialised);

			m_Options = options;

			m_API = options.videoApi;

			if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Vulkan)
			{
				Debug.LogWarning("[AVProVideo] Vulkan graphics API is not supported.  Please select OpenGL ES 2.0 and 3.0 are supported on Android.");
			}

#if UNITY_2017_2_OR_NEWER
			Vector2 vPreferredVideo = GetPreferredVideoResolution(options.preferredMaximumResolution, options.customPreferredMaximumResolution);
#else
			Vector2 vPreferredVideo = GetPreferredVideoResolution(options.preferredMaximumResolution, new Vector2(0.0f, 0.0f));
#endif

			// Create a java-size video class up front
			Debug.Log("s_Interface " + s_Interface);
			m_Video = s_Interface.Call<AndroidJavaObject>( "CreatePlayer", (int)(m_API), options.useFastOesPath, options.preferSoftwareDecoder, (int)(options.audioOutput), (int)(options.audio360ChannelMode), Helper.GetUnityAudioSampleRate(), 
																		   options.StartWithHighestBandwidth(), options.minBufferMs, options.maxBufferMs, options.bufferForPlaybackMs, options.bufferForPlaybackAfterRebufferMs, 
																		   (int)(options.GetPreferredPeakBitRateInBitsPerSecond()), (int)(vPreferredVideo.x), (int)(vPreferredVideo.y), (int)(options.blitTextureFiltering) );
			Debug.Log("m_Video " + m_Video);

			if (m_Video != null)
			{
				m_Method_Update = GetMethod("Update", "()V");
				m_Method_SetHeadRotation = GetMethod("SetHeadRotation", "(FFFF)V");
				m_Method_SetFocusProps = GetMethod("SetFocusProps", "(FF)V");
				m_Method_SetFocusEnabled = GetMethod("SetFocusEnabled", "(Z)V");
				m_Method_SetFocusRotation = GetMethod("SetFocusRotation", "(FFFF)V");
				m_Method_GetCurrentTimeS = GetMethod("GetCurrentTimeS", "()D");
				m_Method_GetSourceVideoFrameRate = GetMethod("GetSourceVideoFrameRate", "()F");
				m_Method_IsPlaying = GetMethod("IsPlaying", "()Z");
				m_Method_IsPaused = GetMethod("IsPaused", "()Z");
				m_Method_IsFinished = GetMethod("IsFinished", "()Z");
				m_Method_IsSeeking = GetMethod("IsSeeking", "()Z");
				m_Method_IsBuffering = GetMethod("IsBuffering", "()Z");
				m_Method_IsLooping = GetMethod("IsLooping", "()Z");
				m_Method_HasVideo = GetMethod("HasVideo", "()Z");
				m_Method_HasAudio = GetMethod("HasAudio", "()Z");
				m_Method_HasMetaData = GetMethod("HasMetaData", "()Z");

				m_iPlayerIndex = m_Video.Call<int>("GetPlayerIndex");
				Helper.LogInfo("Creating player " + m_iPlayerIndex);
				SetOptions(options.useFastOesPath, options.showPosterFrame);

				// Initialise renderer, on the render thread
				AndroidMediaPlayer.IssuePluginEvent(Native.AVPPluginEvent.PlayerSetup, m_iPlayerIndex);
			}
			else
			{
				Debug.LogError("[AVProVideo] Failed to create player instance");
			}
		}

		public void SetOptions(bool useFastOesPath, bool showPosterFrame)
		{
			m_UseFastOesPath = useFastOesPath;

			if (m_Video != null)
			{
				// Show poster frame is only needed when using the MediaPlayer API
				showPosterFrame = (m_API == Android.VideoApi.MediaPlayer) ? showPosterFrame:false;

				m_Video.Call("SetPlayerOptions", m_UseFastOesPath, showPosterFrame);
			}
		}

		public override long GetEstimatedTotalBandwidthUsed()
		{
			long result = -1;
			if (s_Interface != null)
			{
				result = m_Video.Call<long>("GetEstimatedBandwidthUsed");
			}
			return result;
		}


		public override string GetVersion()
		{
			return s_Version;
		}

		public override string GetExpectedVersion()
		{
			return Helper.ExpectedPluginVersion.Android;
		}

		public override bool OpenMedia(string path, long offset, string httpHeader, MediaHints mediaHints, int forceFileFormat = 0, bool startWithHighestBitrate = false)
		{
			bool bReturn = false;

			if (m_Video != null)
			{
				_mediaHints = mediaHints;

				Debug.Assert(m_Width == 0 && m_Height == 0 && m_Duration == 0.0);
				bReturn = m_Video.Call<bool>("OpenVideoFromFile", path, offset, httpHeader, forceFileFormat, (int)(m_Options.audioOutput), (int)(m_Options.audio360ChannelMode));
				if (!bReturn)
				{
					DisplayLoadFailureSuggestion(path);
				}
			}
			else
			{
				Debug.LogError("[AVProVideo] m_Video is null!");
			}

			return bReturn;
		}

		public override void SetKeyServerAuthToken(string token)
		{
			if (m_Video != null)
			{
				m_Video.Call("SetKeyServerAuthToken", token);
			}
		}

		public override void SetOverrideDecryptionKey(byte[] key)
		{
			if( m_Video != null )
			{
				m_Video.Call("SetOverrideDecryptionKey", key);
			}
		}

		private void DisplayLoadFailureSuggestion(string path)
		{
			if (path.ToLower().Contains("http://"))
			{
				Debug.LogError("Android 8 and above require HTTPS by default, change to HTTPS or enable ClearText in the AndroidManifest.xml");
			}
		}

		public override void CloseMedia()
		{
			if (m_Texture != null)
			{
				Texture2D.Destroy(m_Texture);
				m_Texture = null;
			}
			m_TextureHandle = 0;

			m_Duration = 0.0;
			m_Width = 0;
			m_Height = 0;

			if (m_Video != null)
			{
				m_Video.Call("CloseVideo");
			}

			base.CloseMedia();
		}

		public override void SetLooping( bool bLooping )
		{
			if( m_Video != null )
			{
				m_Video.Call("SetLooping", bLooping);
			}
		}

		public override bool IsLooping()
		{
			bool result = false;
			if( m_Video != null )
			{
				if (m_Method_IsLooping != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsLooping, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsLooping");
				}
			}
			return result;
		}

		public override bool HasVideo()
		{
			bool result = false;
			if( m_Video != null )
			{
				if (m_Method_HasVideo != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_HasVideo, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("HasVideo");
				}
			}
			return result;
		}

		public override bool HasAudio()
		{
			bool result = false;
			if( m_Video != null )
			{
				if (m_Method_HasAudio != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_HasAudio, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("HasAudio");
				}
			}
			return result;
		}

		public override bool HasMetaData()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_HasMetaData != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_HasMetaData, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("HasMetaData");
				}
			}
			return result;
		}

		public override bool CanPlay()
		{
			bool result = false;
#if DLL_METHODS
			result = Native._CanPlay( m_iPlayerIndex );
#else
			if (m_Video != null)
			{
				result = m_Video.Call<bool>("CanPlay");
			}
#endif
			return result;
		}

		public override void Play()
		{
			if (m_Video != null)
			{
				m_Video.Call("Play");
			}
		}

		public override void Pause()
		{
			if (m_Video != null)
			{
				m_Video.Call("Pause");
			}
		}

		public override void Stop()
		{
			if (m_Video != null)
			{
				// On Android we never need to actually Stop the playback, pausing is fine
				m_Video.Call("Pause");
			}
		}

		public override void Seek(double time)
		{
			if (m_Video != null)
			{
				// time is in seconds
				m_Video.Call("Seek", time);
			}
		}

		public override void SeekFast(double time)
		{
			if (m_Video != null)
			{
				// time is in seconds
				m_Video.Call("SeekFast", time);
			}
		}

		public override double GetCurrentTime()
		{
			double result = 0.0;
			if (m_Video != null)
			{
				if (m_Method_GetCurrentTimeS != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallDoubleMethod(m_Video.GetRawObject(), m_Method_GetCurrentTimeS, m_Value0);
				}
				else
				{
					result = (double)m_Video.Call<double>("GetCurrentTimeS");
				}
			}
			return result;
		}

		public override void SetPlaybackRate(float rate)
		{
			if (m_Video != null)
			{
				m_Video.Call("SetPlaybackRate", rate);
			}
		}

		public override float GetPlaybackRate()
		{
			float result = 0.0f;
			if (m_Video != null)
			{
				result = m_Video.Call<float>("GetPlaybackRate");
			}
			return result;
		}

		public override void SetAudioHeadRotation(Quaternion q)
		{
			if (m_Video != null)
			{
				if (!m_HeadRotationEnabled)
				{
					m_Video.Call("SetPositionTrackingEnabled", true);
					m_HeadRotationEnabled = true;
				}

				if (m_Method_SetHeadRotation != System.IntPtr.Zero)
				{
					m_Value4[0].f = q.x;
					m_Value4[1].f = q.y;
					m_Value4[2].f = q.z;
					m_Value4[3].f = q.w;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetHeadRotation, m_Value4);
				}
				else
				{
					m_Video.Call("SetHeadRotation", q.x, q.y, q.z, q.w);
				}
			}
		}

		public override void ResetAudioHeadRotation()
		{
			if (m_Video != null && m_HeadRotationEnabled)
			{
				m_Video.Call("SetPositionTrackingEnabled", false);
				m_HeadRotationEnabled = false;
			}
		}

		public override void SetAudioFocusEnabled(bool enabled)
		{
			if (m_Video != null && enabled != m_FocusEnabled)
			{
				if (m_Method_SetFocusEnabled != System.IntPtr.Zero)
				{
					m_Value1[0].z = enabled;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusEnabled, m_Value1);
				}
				else
				{
					m_Video.Call("SetFocusEnabled", enabled);
				}
				m_FocusEnabled = enabled;
			}
		}

		public override void SetAudioFocusProperties(float offFocusLevel, float widthDegrees)
		{
			if(m_Video != null && m_FocusEnabled)
			{
				if (m_Method_SetFocusProps != System.IntPtr.Zero)
				{
					m_Value2[0].f = offFocusLevel;
					m_Value2[1].f = widthDegrees;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusProps, m_Value2);
				}
				else
				{
					m_Video.Call("SetFocusProps", offFocusLevel, widthDegrees);
				}
			}
		}

		public override void SetAudioFocusRotation(Quaternion q)
		{
			if (m_Video != null && m_FocusEnabled)
			{
				if (m_Method_SetFocusRotation != System.IntPtr.Zero)
				{
					m_Value4[0].f = q.x;
					m_Value4[1].f = q.y;
					m_Value4[2].f = q.z;
					m_Value4[3].f = q.w;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusRotation, m_Value4);
				}
				else
				{
					m_Video.Call("SetFocusRotation", q.x, q.y, q.z, q.w);
				}
			}
		}

		public override void ResetAudioFocus()
		{
			if (m_Video != null)
			{
				if (m_Method_SetFocusProps != System.IntPtr.Zero &&
					m_Method_SetFocusEnabled != System.IntPtr.Zero &&
					m_Method_SetFocusRotation != System.IntPtr.Zero)
				{
					m_Value2[0].f = 0f;
					m_Value2[1].f = 90f;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusProps, m_Value2);
					m_Value1[0].z = false;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusEnabled, m_Value1);
					m_Value4[0].f = 0f;
					m_Value4[1].f = 0f;
					m_Value4[2].f = 0f;
					m_Value4[3].f = 1f;
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_SetFocusRotation, m_Value4);
				}
				else
				{
					m_Video.Call("SetFocusProps", 0f, 90f);
					m_Video.Call("SetFocusEnabled", false);
					m_Video.Call("SetFocusRotation", 0f, 0f, 0f, 1f);
				}
			}
		}

		public override double GetDuration()
		{
			return m_Duration;
		}

		public override int GetVideoWidth()
		{
			return m_Width;
		}
			
		public override int GetVideoHeight()
		{
			return m_Height;
		}

		public override float GetVideoFrameRate()
		{
			float result = 0.0f;
			if( m_Video != null )
			{
				if (m_Method_GetSourceVideoFrameRate != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallFloatMethod(m_Video.GetRawObject(), m_Method_GetSourceVideoFrameRate, m_Value0);
				}
				else
				{
					result = m_Video.Call<float>("GetSourceVideoFrameRate");
				}
			}
			return result;
		}

		public override float GetVideoDisplayRate()
		{
			float result = 0.0f;
#if DLL_METHODS
			result = Native._GetVideoDisplayRate( m_iPlayerIndex );
#else
			if (m_Video != null)
			{
				result = m_Video.Call<float>("GetDisplayRate");
			}
#endif
			return result;
		}

		public override bool IsSeeking()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsSeeking != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsSeeking, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsSeeking");
				}
			}
			return result;
		}

		public override bool IsPlaying()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsPlaying != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsPlaying, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsPlaying");
				}
			}
			return result;
		}

		public override bool IsPaused()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsPaused != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsPaused, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsPaused");
				}
			}
			return result;
		}

		public override bool IsFinished()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsFinished != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsFinished, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsFinished");
				}
			}
			return result;
		}

		public override bool IsBuffering()
		{
			bool result = false;
			if (m_Video != null)
			{
				if (m_Method_IsBuffering != System.IntPtr.Zero)
				{
					result = AndroidJNI.CallBooleanMethod(m_Video.GetRawObject(), m_Method_IsBuffering, m_Value0);
				}
				else
				{
					result = m_Video.Call<bool>("IsBuffering");
				}
			}
			return result;
		}

		public override Texture GetTexture( int index )
		{
			Texture result = null;
			if (GetTextureFrameCount() > 0)
			{
				result = m_Texture;
			}
			return result;
		}

		public override int GetTextureFrameCount()
		{
			int result = 0;
			if( m_Texture != null )
			{
#if DLL_METHODS
				result = Native._GetFrameCount( m_iPlayerIndex );
#else
				if (m_Video != null)
				{
					result = m_Video.Call<int>("GetFrameCount");
				}
#endif
			}
			return result;
		}

		internal override StereoPacking InternalGetTextureStereoPacking()
		{
			StereoPacking result = StereoPacking.Unknown;
			if (m_Video != null)
			{
				NativeStereoPacking internalStereoMode = (NativeStereoPacking)( m_Video.Call<int>("GetCurrentVideoTrackStereoMode") );
				switch( internalStereoMode )
				{
					case NativeStereoPacking.Monoscopic:	result = StereoPacking.None;		break;
					case NativeStereoPacking.TopBottom:		result = StereoPacking.TopBottom;	break;
					case NativeStereoPacking.LeftRight:		result = StereoPacking.LeftRight;	break;
					case NativeStereoPacking.Mesh:			result = StereoPacking.Unknown;		break;
				}
			}
			return result;
		}

		public override bool RequiresVerticalFlip()
		{
			return false;
		}

		public override void MuteAudio(bool bMuted)
		{
			if (m_Video != null)
			{
				m_Video.Call("MuteAudio", bMuted);
			}
		}

		public override bool IsMuted()
		{
			bool result = false;
			if( m_Video != null )
			{
				result = m_Video.Call<bool>("IsMuted");
			}
			return result;
		}

		public override void SetVolume(float volume)
		{
			if (m_Video != null)
			{
				m_Video.Call("SetVolume", volume);
			}
		}

		public override float GetVolume()
		{
			float result = 0.0f;
			if( m_Video != null )
			{
				result = m_Video.Call<float>("GetVolume");
			}
			return result;
		}

		public override void SetBalance(float balance)
		{
			if( m_Video != null )
			{
				m_Video.Call("SetAudioPan", balance);
			}
		}

		public override float GetBalance()
		{
			float result = 0.0f;
			if( m_Video != null )
			{
				result = m_Video.Call<float>("GetAudioPan");
			}
			return result;
		}

		public override bool WaitForNextFrame(Camera dummyCamera, int previousFrameCount)
		{
			// Mark as extracting
			bool isMultiThreaded = m_Video.Call<bool>("StartExtractFrame");

			// In single threaded Android this method won't work, so just return
			if (isMultiThreaded)
			{
				// Queue up render thread event to wait for the new frame
				IssuePluginEvent(Native.AVPPluginEvent.ExtractFrame, m_iPlayerIndex);

				// Force render thread to run
				dummyCamera.Render();

				// Wait for the frame to change
				m_Video.Call("WaitForExtract");

				// Return whether the frame changed
				return (previousFrameCount != GetTextureFrameCount());
			}
			return false;	
		}

		public override long GetTextureTimeStamp()
		{
			long timeStamp = long.MinValue;
			if (m_Video != null)
			{
				timeStamp = m_Video.Call<long>("GetTextureTimeStamp");
			}
			return timeStamp;
		}

		public override void Render()
		{
			if (m_Video != null)
			{
				if (m_UseFastOesPath)
				{
					// This is needed for at least Unity 5.5.0, otherwise it just renders black in OES mode
					GL.InvalidateState();
				}
				AndroidMediaPlayer.IssuePluginEvent( Native.AVPPluginEvent.PlayerUpdate, m_iPlayerIndex );
				if (m_UseFastOesPath)
				{
					GL.InvalidateState();
				}
			}
		}

		public override void OnEnable()
		{
			base.OnEnable();

#if DLL_METHODS
			int textureHandle = Native._GetTextureHandle(m_iPlayerIndex);
#else
			int textureHandle = m_Video.Call<int>("GetTextureHandle");
#endif

			if (m_Texture != null && textureHandle > 0 && m_Texture.GetNativeTexturePtr() == System.IntPtr.Zero)
			{
				//Debug.Log("RECREATING");
				m_Texture.UpdateExternalTexture(new System.IntPtr(textureHandle));
			}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
			_textureQuality = QualitySettings.masterTextureLimit;
#endif
		}

		public override System.DateTime GetProgramDateTime()
		{
			System.DateTime result;
			if (m_Video != null)
			{
				double seconds = m_Video.Call<double>("GetCurrentAbsoluteTimestamp");
				result = Helper.ConvertSecondsSince1970ToDateTime(seconds);
			}
			else
			{
				result = base.GetProgramDateTime();
			}
			return result;
		}

		public override void Update()
		{
			if (m_Video != null)
			{
				if (m_Method_Update != System.IntPtr.Zero)
				{
					AndroidJNI.CallVoidMethod(m_Video.GetRawObject(), m_Method_Update, m_Value0);
				}
				else
				{
					m_Video.Call("Update");
				}

//				_lastError = (ErrorCode)( m_Video.Call<int>("GetLastErrorCode") );
				_lastError = (ErrorCode)( Native._GetLastErrorCode( m_iPlayerIndex ) );
			}

			if( m_Options.HasChanged( MediaPlayer.OptionsAndroid.ChangeFlags.All, true ) )
			{
				if (m_Video != null)
				{
#if UNITY_2017_2_OR_NEWER
					Vector2 vPreferredVideo = GetPreferredVideoResolution(m_Options.preferredMaximumResolution, m_Options.customPreferredMaximumResolution);
#else
					Vector2 vPreferredVideo = GetPreferredVideoResolution(m_Options.preferredMaximumResolution, new Vector2(0.0f, 0.0f));
#endif

					int iNewBitrate = (int)(m_Options.GetPreferredPeakBitRateInBitsPerSecond());
					/*bool bSetMaxResolutionAndBitrate =*/ m_Video.Call<bool>("SetPreferredVideoResolutionAndBitrate", (int)(vPreferredVideo.x), (int)(vPreferredVideo.y), iNewBitrate);
				}
			}

/*
			m_fTestTime += Time.deltaTime;
			if( m_fTestTime > 4.0f )
			{
				m_fTestTime = 0.0f;

				int iNumStreams = InternalGetAdaptiveStreamCount( TrackType.Video );
				int iNewStreamIndex = UnityEngine.Random.Range( -1, iNumStreams );
				SetVideoAdaptiveStreamIndex( TrackType.Video, iNewStreamIndex );
			}
*/

			// Call before the native update call
			UpdateTracks();
			UpdateTextCue();

			UpdateSubtitles();

			UpdateTimeRanges();

			UpdateDisplayFrameRate();

			if (Mathf.Approximately((float)m_Duration, 0f))
			{
#if DLL_METHODS
				m_Duration = (double)( Native._GetDuration( m_iPlayerIndex ) );
#else
				m_Duration = m_Video.Call<double>("GetDurationS");
#endif

//				if( m_DurationMs > 0.0f ) { Helper.LogInfo("Duration: " + m_DurationMs); }
			}

			// Check if we can create the texture
			// Scan for a change in resolution
			int newWidth = -1;
			int newHeight = -1;
			if (m_Texture != null)
			{
#if DLL_METHODS
				newWidth = Native._GetWidth(m_iPlayerIndex);
				newHeight = Native._GetHeight(m_iPlayerIndex);
#else
				newWidth = m_Video.Call<int>("GetWidth");
				newHeight = m_Video.Call<int>("GetHeight");
#endif
				if (newWidth != m_Width || newHeight != m_Height)
				{
					m_Texture = null;
					m_TextureHandle = 0;
				}
			}
#if DLL_METHODS
			int textureHandle = Native._GetTextureHandle(m_iPlayerIndex);
#else
				int textureHandle = m_Video.Call<int>("GetTextureHandle");
#endif
			if (textureHandle != 0 && textureHandle != m_TextureHandle)
			{
				// Already got? (from above)
				if (newWidth == -1 || newHeight == -1)
				{
#if DLL_METHODS
					newWidth = Native._GetWidth(m_iPlayerIndex);
					newHeight = Native._GetHeight(m_iPlayerIndex);
#else
					newWidth = m_Video.Call<int>("GetWidth");
					newHeight = m_Video.Call<int>("GetHeight");
#endif
				}

				if (Mathf.Max(newWidth, newHeight) > SystemInfo.maxTextureSize)
				{
					m_Width = newWidth;
					m_Height = newHeight;
					m_TextureHandle = textureHandle;
					Debug.LogError("[AVProVideo] Video dimensions larger than maxTextureSize");
				}
				else if (newWidth > 0 && newHeight > 0)
				{
					m_Width = newWidth;
					m_Height = newHeight;
					m_TextureHandle = textureHandle;

					switch (m_API)
					{
						case Android.VideoApi.MediaPlayer:
							_playerDescription = "MediaPlayer";
							break;
						case Android.VideoApi.ExoPlayer:
							_playerDescription = "ExoPlayer";
							break;
						default:
							_playerDescription = "UnknownPlayer";
							break;
					}

					if (m_UseFastOesPath)
					{
						_playerDescription += " OES";
					}
					else
					{
						_playerDescription += " NonOES";
					}

					Helper.LogInfo("Using playback path: " + _playerDescription + " (" + m_Width + "x" + m_Height + "@" + GetVideoFrameRate().ToString("F2") + ")");

					// NOTE: From Unity 5.4.x when using OES textures, an error "OPENGL NATIVE PLUG-IN ERROR: GL_INVALID_OPERATION: Operation illegal in current state" will be logged.
					// We assume this is because we're passing in TextureFormat.RGBA32 which isn't the true texture format.  This error should be safe to ignore.
					m_Texture = Texture2D.CreateExternalTexture(m_Width, m_Height, TextureFormat.RGBA32, false, false, new System.IntPtr(textureHandle));
					if (m_Texture != null)
					{
						ApplyTextureProperties(m_Texture);
					}
					Helper.LogInfo("Texture ID: " + textureHandle);
				}
			}

#if AVPROVIDEO_FIXREGRESSION_TEXTUREQUALITY_UNITY542
			// In Unity 5.4.2 and above the video texture turns black when changing the TextureQuality in the Quality Settings
			// The code below gets around this issue.  A bug report has been sent to Unity.  So far we have tested and replicated the
			// "bug" in Windows only, but a user has reported it in Android too.  
			// Texture.GetNativeTexturePtr() must sync with the rendering thread, so this is a large performance hit!
			if (_textureQuality != QualitySettings.masterTextureLimit)
			{
				if (m_Texture != null && textureHandle > 0 && m_Texture.GetNativeTexturePtr() == System.IntPtr.Zero)
				{
					//Debug.Log("RECREATING");
					m_Texture.UpdateExternalTexture(new System.IntPtr(textureHandle));
				}

				_textureQuality = QualitySettings.masterTextureLimit;
			}
#endif
		}

		protected override void ApplyTextureProperties(Texture texture)
		{
			// NOTE: According to OES_EGL_image_external: For external textures, the default min filter is GL_LINEAR and the default S and T wrap modes are GL_CLAMP_TO_EDGE
			// See https://www.khronos.org/registry/gles/extensions/OES/OES_EGL_image_external_essl3.txt
			// But there is a new extension that allows some wrap modes:
			// https://www.khronos.org/registry/OpenGL/extensions/EXT/EXT_EGL_image_external_wrap_modes.txt
			// So really we need to detect whether these extensions exist when m_UseFastOesPath is true
			//if (!m_UseFastOesPath)
			{
				base.ApplyTextureProperties(texture);
			}
		}

		public override bool PlayerSupportsLinearColorSpace()
		{
			return false;
		}

		public override float[] GetTextureTransform()
		{
			float[] transform = null;
			if (m_Video != null)
			{
				transform = m_Video.Call<float[]>("GetTextureTransform");
				/*if (transform != null)
				{
					Debug.Log("xform: " + transform[0] + " " + transform[1] + " " + transform[2] + " " + transform[3] + " " + transform[4] + " " + transform[5]);
				}*/
			}
			return transform;
		}

		public override void Dispose()
		{
			//Debug.LogError("DISPOSE");

			if (m_Video != null)
			{
				m_Video.Call("SetDeinitialiseFlagged");

				m_Video.Dispose();
				m_Video = null;
			}

			if (s_Interface != null)
			{
				s_Interface.Call("DestroyPlayer", m_iPlayerIndex);
			}

			if (m_Texture != null)
			{
				Texture2D.Destroy(m_Texture);
				m_Texture = null;
			}

			// Deinitialise player (replaces call directly as GL textures are involved)
			AndroidMediaPlayer.IssuePluginEvent( Native.AVPPluginEvent.PlayerDestroy, m_iPlayerIndex );
		}

		private void UpdateTimeRanges()
		{
			if( m_Video != null )
			{
				// Seekable time ranges
				AndroidJavaObject seekableReturnObject = m_Video.Call<AndroidJavaObject>("GetSeekableTimeRanges");
				if( seekableReturnObject.GetRawObject().ToInt32() != 0 )
				{
					double[] aSeekableRanges = AndroidJNIHelper.ConvertFromJNIArray<double[]>( seekableReturnObject.GetRawObject() );
					if( aSeekableRanges.Length > 1 )
					{
						int iNumRanges = aSeekableRanges.Length / 2;
						_seekableTimes._ranges = new TimeRange[ iNumRanges ];

						int iRangeIndex = 0;
						for( int iRange = 0; iRange < iNumRanges; ++iRange )
						{
							_seekableTimes._ranges[ iRange ] = new TimeRange( aSeekableRanges[ iRangeIndex ], aSeekableRanges[ iRangeIndex + 1 ] );
							iRangeIndex += 2;
						}
						_seekableTimes.CalculateRange();
					}

					seekableReturnObject.Dispose();
				}

				// Buffered time ranges
				AndroidJavaObject bufferedReturnObject = m_Video.Call<AndroidJavaObject>("GetBufferedTimeRanges");
				if( bufferedReturnObject.GetRawObject().ToInt32() != 0 )
				{
					double[] aBufferedRanges = AndroidJNIHelper.ConvertFromJNIArray<double[]>( bufferedReturnObject.GetRawObject() );
					if( aBufferedRanges.Length > 1 )
					{
						int iNumRanges = aBufferedRanges.Length / 2;
						_bufferedTimes._ranges = new TimeRange[ iNumRanges ];

						int iRangeIndex = 0;
						for( int iRange = 0; iRange < iNumRanges; ++iRange )
						{
							_bufferedTimes._ranges[iRange] = new TimeRange( aBufferedRanges[iRangeIndex], aBufferedRanges[iRangeIndex + 1] );
							iRangeIndex += 2;
						}
						_bufferedTimes.CalculateRange();
					}

					bufferedReturnObject.Dispose();
				}
			}
		}

		bool isMainThread()
		{
			return m_MainThread.Equals(System.Threading.Thread.CurrentThread);
		}

		public override int GetAudioChannelCount()
		{
			return Native._GetCurrentAudioTrackNumChannels(m_iPlayerIndex);
		}
/*
		public override AudioChannelMaskFlags GetAudioChannelMask()
		{
			return (AudioChannelMaskFlags)Native.GetAudioChannelMask(_instance);
		}
*/

		public override int GrabAudio(float[] buffer, int sampleCount, int channelCount)
		{
			int iReturn = 0;

			// Get audio data
			iReturn = Native._GrabAudioNative( buffer, m_iPlayerIndex, sampleCount, channelCount );

			return iReturn;
		}

		public override int GetAudioBufferedSampleCount()
		{
			int iBufferedSampleCount = 0;

			if (m_Video != null)
			{
				// Get audio data
				iBufferedSampleCount = m_Video.Call<int>("GetAudioBufferedSampleCount");
			}

			return iBufferedSampleCount;
		}

		internal override bool InternalIsChangedTextCue()
		{
			// Has the text cue changed since the last frame 'tick'
			if( m_Video != null )
			{
				return m_Video.Call<bool>("GetTextCueDirty");
			}

			return false;
		}

		internal override string InternalGetCurrentTextCue()
		{
			// Return a pointer to the current text cue string in UTF16 format
			if ( m_Video != null )
			{
				return m_Video.Call<string>("GetCurrentTextCue");
			}

			return string.Empty;
		}

		internal override bool InternalIsChangedTracks(TrackType trackType)
		{
			// Has it changed since the last frame 'tick'
			bool result = false;
			switch (trackType)
			{
				case TrackType.Video:
					{
						result = ( m_Video != null ) ? m_Video.Call<bool>("GetVideoTracksDirty") : false;
						break;
					}
				case TrackType.Audio:
					{
						result = ( m_Video != null ) ? m_Video.Call<bool>("GetAudioTracksDirty") : false;
						break;
					}
				case TrackType.Text:
					{
						result = ( m_Video != null ) ? m_Video.Call<bool>("GetTextTracksDirty") : false;
						break;
					}
			}
			return result;
		}

		internal override int InternalGetTrackCount(TrackType trackType)
		{
			int result = 0;
			switch (trackType)
			{
				case TrackType.Video:
					{
						result = ( m_Video != null ) ? m_Video.Call<int>("GetNumberVideoTracks") : 0;
						break;
					}
				case TrackType.Audio:
					{
						result = ( m_Video != null ) ? m_Video.Call<int>("GetNumberAudioTracks") : 0;
						break;
					}
				case TrackType.Text:
					{
						result = ( m_Video != null ) ? m_Video.Call<int>("GetNumberTextTracks") : 0;
						break;
					}
			}
			return result;
		}

		internal override bool InternalSetActiveTrack(TrackType trackType, int trackUid)
		{
			bool result = false;
			switch (trackType)
			{
				case TrackType.Video:
					{
						result = ( m_Video != null ) ? m_Video.Call<bool>("SetVideoTrack", trackUid) : false;
						break;
					}
				case TrackType.Audio:
					{
						result = ( m_Video != null ) ? m_Video.Call<bool>("SetAudioTrack", trackUid) : false;
						break;
					}
				case TrackType.Text:
					{
						result = ( m_Video != null ) ? m_Video.Call<bool>("SetTextTrack", trackUid) : false;
						break;
					}
			}
			return result;
		}

		internal override TrackBase InternalGetTrackInfo(TrackType trackType, int trackIndex, ref bool isActiveTrack)
		{
			TrackBase result = null;
			switch (trackType)
			{
				case TrackType.Video:
					{
						if (m_Video != null)
						{
							AndroidJavaObject returnObject = m_Video.Call<AndroidJavaObject>("GetVideoTrackInfo", trackIndex);
							if (returnObject.GetRawObject().ToInt32() != 0)
							{
								String[] aReturn = AndroidJNIHelper.ConvertFromJNIArray<String[]>(returnObject.GetRawObject());
								bool bReturn = (aReturn.Length > 0) ? (int.Parse(aReturn[0]) == 1) : false;

								if (bReturn)
								{
									result = new VideoTrack(trackIndex, aReturn[1], aReturn[2], (aReturn[3] == "1"));

									isActiveTrack = (m_Video != null && m_Video.Call<int>("GetCurrentVideoTrackIndex") == trackIndex);
								}

								returnObject.Dispose();
							}
						}
					}
					break;

				case TrackType.Audio:
					{
						if (m_Video != null)
						{
							AndroidJavaObject returnObject = m_Video.Call<AndroidJavaObject>("GetAudioTrackInfo", trackIndex);
							if (returnObject.GetRawObject().ToInt32() != 0)
							{
								String[] aReturn = AndroidJNIHelper.ConvertFromJNIArray<String[]>( returnObject.GetRawObject() );
								bool bReturn = ( aReturn.Length > 0 ) ? ( int.Parse( aReturn[ 0 ]) == 1 ) : false;

								if (bReturn)
								{
									int iBitrate = 0;
									int.TryParse( aReturn[ 4 ], out iBitrate );

									int iChannelCount = 0;
									int.TryParse(aReturn[ 5 ], out iChannelCount);

									result = new AudioTrack( trackIndex, aReturn[ 1 ], aReturn[ 2 ], (aReturn[ 3 ] == "1") );

									isActiveTrack = (m_Video != null && m_Video.Call<int>("GetCurrentAudioTrackIndex") == trackIndex);
								}

								returnObject.Dispose();
							}
						}
					}
					break;
				
				case TrackType.Text:
					{
						if (m_Video != null)
						{
							AndroidJavaObject returnObject = m_Video.Call<AndroidJavaObject>( "GetTextTrackInfo", trackIndex );
							if (returnObject.GetRawObject().ToInt32() != 0)
							{
								String[] aReturn = AndroidJNIHelper.ConvertFromJNIArray<String[]>( returnObject.GetRawObject() );
								bool bReturn = ( aReturn.Length > 0 ) ? ( int.Parse(aReturn[ 0 ] ) == 1 ) : false;

								int uid = -1;

								if( bReturn )
								{
									int.TryParse(aReturn[1], out uid);

									result = new TextTrack(uid, aReturn[ 2 ], aReturn[ 3 ], false);

									isActiveTrack = (m_Video != null && m_Video.Call<int>("GetCurrentTextTrackIndex") == trackIndex);
								}

								returnObject.Dispose();
							}
						}
					}
					break;
			}
			return result;
		}

		internal /*override*/ int InternalGetAdaptiveStreamCount(TrackType trackType, int trackIndex = -1)
		{
			int result = 0;
			switch (trackType)
			{
				case TrackType.Video:
					{
						result = (m_Video != null) ? m_Video.Call<int>("GetNumberVideoAdaptiveStreams", trackIndex) : 0;

						Debug.Log("[AVProVideo]: InternalGetAdaptiveStreamCount return = " + result);
						break;
					}
				case TrackType.Audio:
					{
						break;
					}
				case TrackType.Text:
					{
						break;
					}
			}
			return result;
		}

		internal /*override*/ void InternalGetAdaptiveStreamInfo(TrackType trackType, int trackIndex = -1)
		{
			switch( trackType )
			{
				case TrackType.Video:
					{
						if( m_Video != null )
						{
							AndroidJavaObject returnObject = m_Video.Call<AndroidJavaObject>("GetVideoAdaptiveStreamInfo", trackIndex);
							if( returnObject.GetRawObject().ToInt32() != 0 )
							{
								String[] aReturn = AndroidJNIHelper.ConvertFromJNIArray<String[]>(returnObject.GetRawObject());
								bool bReturn = (aReturn.Length > 0) ? (int.Parse(aReturn[0]) == 1) : false;

								string toLog = "";
								foreach( string str in aReturn )	{ toLog += str + ", "; }
								Debug.Log( "[AVProVideo]: InternalGetAdaptiveStreamInfo return = " + toLog );

								if ( bReturn )
								{
								}

								returnObject.Dispose();
							}
						}
					}
					break;

				case TrackType.Audio:
					{
					}
					break;

				case TrackType.Text:
					{
					}
					break;
			}
		}

		internal /*override*/ int SetVideoAdaptiveStreamIndex(TrackType trackType, int streamIndex)
		{
			int result = 0;
			switch( trackType )
			{
				case TrackType.Video:
					{
						Debug.Log("[AVProVideo]: SetVideoAdaptiveStreamIndex : streamIndex = " + streamIndex);

						result = (m_Video != null) ? m_Video.Call<int>("SetVideoAdaptiveStreams", streamIndex) : 0;
						break;
					}
				case TrackType.Audio:
					{
						break;
					}
				case TrackType.Text:
					{
						break;
					}
			}
			return result;
		}

		private Vector2 GetPreferredVideoResolution(MediaPlayer.OptionsAndroid.Resolution preferredMaximumResolution, Vector2 customPreferredMaximumResolution)
		{
			Vector2 vResolution = new Vector2( 0.0f, 0.0f );

			switch( preferredMaximumResolution )
			{
				case MediaPlayer.OptionsAndroid.Resolution.NoPreference:
					break;
				case MediaPlayer.OptionsAndroid.Resolution._480p:
					vResolution.x = 640;
					vResolution.y = 480;
					break;
				case MediaPlayer.OptionsAndroid.Resolution._720p:
					vResolution.x = 1280;
					vResolution.y = 720;
					break;
				case MediaPlayer.OptionsAndroid.Resolution._1080p:
					vResolution.x = 1920;
					vResolution.y = 1080;
					break;
				case MediaPlayer.OptionsAndroid.Resolution._2160p:
					vResolution.x = 3840;
					vResolution.y = 2160;
					break;
				case MediaPlayer.OptionsAndroid.Resolution.Custom:
#if UNITY_2017_2_OR_NEWER
					vResolution.x = customPreferredMaximumResolution.x;
					vResolution.y = customPreferredMaximumResolution.y;
#endif
					break;
			}

			return vResolution;
		}

		public override bool IsMediaCachingSupported()
		{
			if( m_Video != null )
			{
				return m_Video.Call<bool>("IsMediaCachingSupported");
			}

			return true;
		}

		public override void AddMediaToCache(string url, string headers, MediaCachingOptions options)
		{
			if (m_Video != null)
			{
				double dMinBitrate = -1.0f;
				int iMinWidth = -1;
				int iMinHeight = -1;

				double dMaxBitrate = -1.0f;
				int iMaxWidth = -1;
				int iMaxHeight = -1;
				
				if (options != null )
				{
					dMinBitrate = options.minimumRequiredBitRate;
					iMinWidth = (int)( options.minimumRequiredResolution.x );
					iMinHeight = (int)( options.minimumRequiredResolution.y );

					dMaxBitrate = options.maximumRequiredBitRate;
					iMaxWidth = (int)(options.maximumRequiredResolution.x);
					iMaxHeight = (int)(options.maximumRequiredResolution.y);
				}
				m_Video.Call("AddMediaToCache", url, headers, dMinBitrate, iMinWidth, iMinHeight, dMaxBitrate, iMaxWidth, iMaxHeight);
			}
		}

		public override void RemoveMediaFromCache(string url)
		{
			if(m_Video != null)
			{
				m_Video.Call("RemoveMediaFromCache", url);
			}
		}

		public override void CancelDownloadOfMediaToCache(string url)
		{
			if (m_Video != null)
			{
				m_Video.Call("CancelDownloadOfMediaToCache", url);
			}
		}

		public override CachedMediaStatus GetCachedMediaStatus(string url, ref float progress)
		{
			CachedMediaStatus eStatus = CachedMediaStatus.NotCached;

			if (m_Video != null)
			{
				float[] afReturn = m_Video.Call<float[]>("GetCachedMediaStatus", url);
				eStatus = (CachedMediaStatus)( afReturn[ 0 ] );
				progress = afReturn[ 1 ] * 0.01f;

//				if( eStatus != CachedMediaStatus.NotCached && progress < 1.0f )
//				{
//					Debug.Log("AVProVideo: GetCachedMediaStatus | url = " + url + " | eStatus = " + eStatus + " | progress = " + progress);
//				}
			}

			return eStatus;
		}


		private struct Native
		{
			internal const string LibraryName = "AVProVideo2Native";

			[DllImport (LibraryName)]
			public static extern IntPtr GetRenderEventFunc();

			[DllImport(LibraryName)]
			public static extern bool JNIAttachCurrentThread();

			[DllImport(LibraryName)]
			public static extern void JNIDetachCurrentThread();

			[DllImport(LibraryName)]
			public static extern int _GetCurrentAudioTrackNumChannels( int iPlayerIndex );

			[DllImport(LibraryName)]
//			unsafe public static extern int _GrabAudioNative(float* pBuffer, int iPlayerIndex, int sampleCount, int channelCount);
			public static extern int _GrabAudioNative(float[] pBuffer, int iPlayerIndex, int sampleCount, int channelCount);

			[DllImport (LibraryName)]
			public static extern int _GetWidth( int iPlayerIndex );

			[DllImport (LibraryName)]
			public static extern int _GetHeight( int iPlayerIndex );
			
			[DllImport (LibraryName)]
			public static extern int _GetTextureHandle( int iPlayerIndex );

			[DllImport (LibraryName)]
			public static extern double _GetDuration( int iPlayerIndex );

			[DllImport (LibraryName)]
			public static extern int _GetLastErrorCode( int iPlayerIndex );

			[DllImport (LibraryName)]
			public static extern int _GetFrameCount( int iPlayerIndex );
		
			[DllImport (LibraryName)]
			public static extern float _GetVideoDisplayRate( int iPlayerIndex );

			[DllImport (LibraryName)]
			public static extern bool _CanPlay( int iPlayerIndex );
			
			public enum AVPPluginEvent
			{
				Nop,
				PlayerSetup,
				PlayerUpdate,
				PlayerDestroy,
				ExtractFrame,
			}
		}
	}
}
#endif
	  