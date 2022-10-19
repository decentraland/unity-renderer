//-----------------------------------------------------------------------------
// Copyright 2015-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

#if UNITY_2017_2_OR_NEWER && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || (!UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)))

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RenderHeads.Media.AVProVideo
{
	public sealed partial class AppleMediaPlayer: BaseMediaPlayer
	{
		internal partial struct Native
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			private const string PluginName = "AVProVideo";
#elif UNITY_IOS || UNITY_TVOS
			private const string PluginName = "__Internal";
#endif

			// Video settings

			internal enum AVPPlayerVideoPixelFormat: int
			{
				Invalid,
				Bgra,
				YCbCr420
			}

			[Flags]
			internal enum AVPPlayerVideoOutputSettingsFlags: int
			{
				None             = 0,
				LinearColorSpace = 1 << 0,
				GenerateMipmaps  = 1 << 1,
			}

			// Audio settings

			internal enum AVPPlayerAudioOutputMode : int
			{
				SystemDirect,
				Unity,
				SystemDirectWithCapture,
			}

			// Network settings

			[Flags]
			internal enum AVPPlayerNetworkSettingsFlags: int
			{
				None                 = 0,
				PlayWithoutBuffering = 1 << 0,
				UseSinglePlayerItem  = 1 << 1,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerSettings
			{
				// Video
				internal AVPPlayerVideoPixelFormat pixelFormat;
				internal AVPPlayerVideoOutputSettingsFlags videoFlags;
				internal float preferredMaximumResolution_width;
				internal float preferredMaximumResolution_height;
				internal float maximumPlaybackRate;

				// Audio
				internal AVPPlayerAudioOutputMode audioOutputMode;
				internal int sampleRate;
				internal int bufferLength;
				internal int audioFlags;

				// Network
				internal double preferredPeakBitRate;
				internal double preferredForwardBufferDuration;
				internal AVPPlayerNetworkSettingsFlags networkFlags;
			}

			[Flags]
			internal enum AVPPlayerStatus : int
			{
				Unknown                   = 0,
				ReadyToPlay               = 1 <<  0,
				Playing                   = 1 <<  1,
				Paused                    = 1 <<  2,
				Finished                  = 1 <<  3,
				Seeking                   = 1 <<  4,
				Buffering                 = 1 <<  5,
				Stalled                   = 1 <<  6,
				ExternalPlaybackActive    = 1 <<  7,
				Cached                    = 1 <<  8,
				FinishedSeeking           = 1 <<  9,

				UpdatedAssetInfo          = 1 << 16,
				UpdatedTexture            = 1 << 17,
				UpdatedBufferedTimeRanges = 1 << 18,
				UpdatedSeekableTimeRanges = 1 << 19,
				UpdatedText               = 1 << 20,

				HasVideo                  = 1 << 24,
				HasAudio                  = 1 << 25,
				HasText                   = 1 << 26,
				HasMetadata               = 1 << 27,

				Failed                    = 1 << 31
			}

			[Flags]
			internal enum AVPPlayerFlags : int
			{
				None                  = 0,
				Looping               = 1 <<  0,
				Muted                 = 1 <<  1,
				AllowExternalPlayback = 1 <<  2,
				ResumePlayback        = 1 << 16,	// iOS only, resumes playback after audio session route change
				Dirty                 = 1 << 31
			}

			internal enum AVPPlayerExternalPlaybackVideoGravity : int
			{
				Resize,
				ResizeAspect,
				ResizeAspectFill
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerSize
			{
				internal float width;
				internal float height;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPAffineTransform
			{
				internal float a;
				internal float b;
				internal float c;
				internal float d;
				internal float tx;
				internal float ty;
			}

			[Flags]
			internal enum AVPPlayerAssetFlags : int
			{
				None                  = 0,
				CompatibleWithAirPlay = 1 << 0,
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerAssetInfo
			{
				internal double duration;
				internal AVPPlayerSize dimensions;
				internal float frameRate;
				internal int videoTrackCount;
				internal int audioTrackCount;
				internal int textTrackCount;
				internal AVPPlayerAssetFlags flags;
			}

			[Flags]
			internal enum AVPPlayerTrackFlags: int
			{
				Default = 1 << 0,
			}

			internal enum AVPPlayerVideoTrackStereoMode: int
			{
				Unknown,
				Monoscopic,
				StereoscopicTopBottom,
				StereoscopicLeftRight,
				StereoscopicCustom,
				StereoscopicRightLeft,
			}

			[Flags]
			internal enum AVPPlayerVideoTrackFlags: int
			{
				HasAlpha = 1 << 0,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerVideoTrackInfo
			{
				[MarshalAs(UnmanagedType.LPWStr)] internal string name;
				[MarshalAs(UnmanagedType.LPWStr)] internal string language;
				internal int trackId;
				internal float estimatedDataRate;
				internal uint codecSubtype;
				internal AVPPlayerTrackFlags flags;

				internal AVPPlayerSize dimensions;
				internal float frameRate;
				internal AVPAffineTransform transform;
				internal AVPPlayerVideoTrackStereoMode stereoMode;
				internal int bitsPerComponent;
				internal AVPPlayerVideoTrackFlags videoTrackFlags;

				internal Matrix4x4 yCbCrTransform;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerAudioTrackInfo
			{
				[MarshalAs(UnmanagedType.LPWStr)] internal string name;
				[MarshalAs(UnmanagedType.LPWStr)] internal string language;
				internal int trackId;
				internal float estimatedDataRate;
				internal uint codecSubtype;
				internal AVPPlayerTrackFlags flags;

				internal double sampleRate;
				internal uint channelCount;
				internal uint channelLayoutTag;
				internal AudioChannelMaskFlags channelBitmap;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTextTrackInfo
			{
				[MarshalAs(UnmanagedType.LPWStr)] internal string name;
				[MarshalAs(UnmanagedType.LPWStr)] internal string language;
				internal int trackId;
				internal float estimatedDataRate;
				internal uint codecSubtype;
				internal AVPPlayerTrackFlags flags;
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTimeRange
			{
				internal double start;
				internal double duration;
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerState
			{
				internal AVPPlayerStatus status;
				internal double currentTime;
				internal double currentDate;
				internal int selectedVideoTrack;
				internal int selectedAudioTrack;
				internal int selectedTextTrack;
				internal int bufferedTimeRangesCount;
				internal int seekableTimeRangesCount;
				internal int audioCaptureBufferedSamplesCount;
			}

			internal enum AVPPlayerTextureFormat: int
			{
				Unknown,
				BGRA8,
				R8,
				RG8,
				BC1,
				BC3,
				BC4,
				BC5,
				BC7,
				BGR10A2,
				R16,
				RG16,
				BGR10XR,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTexturePlane
			{
				internal IntPtr plane;
				internal int width;
				internal int height;
				internal AVPPlayerTextureFormat textureFormat;
			}

			[Flags]
			internal enum AVPPlayerTextureFlags: int
			{
				None      = 0,
				Flipped   = 1 << 0,
				Linear    = 1 << 1,
				Mipmapped = 1 << 2,
			}

			internal enum AVPPlayerTextureYCbCrMatrix: int
			{
				Identity,
				ITU_R_601,
				ITU_R_709,
			}

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerTexture
			{
				[MarshalAs(UnmanagedType.ByValArray, SizeConst=2)]
				internal AVPPlayerTexturePlane[] planes;
				internal long itemTime;
				internal int frameCount;
				internal int planeCount;
				internal AVPPlayerTextureFlags flags;
				internal AVPPlayerTextureYCbCrMatrix YCbCrMatrix;
			};

			[StructLayout(LayoutKind.Sequential)]
			internal struct AVPPlayerText
			{
				internal IntPtr buffer;
				internal long itemTime;
				internal int length;
				internal int sequence;
			};

			internal enum AVPPlayerTrackType: int
			{
				Video,
				Audio,
				Text
			};

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)
			[DllImport(PluginName)]
			internal static extern void AVPPluginBootstrap();
#endif

			[DllImport(PluginName)]
			private static extern IntPtr AVPPluginGetVersionStringPointer();

			internal static string GetPluginVersion()
			{
				return System.Runtime.InteropServices.Marshal.PtrToStringAnsi(AVPPluginGetVersionStringPointer());
			}

			[DllImport(PluginName)]
			internal static extern IntPtr AVPPluginMakePlayer(Native.AVPPlayerSettings settings);

			[DllImport(PluginName)]
			internal static extern IntPtr AVPPlayerRelease(IntPtr player);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetState(IntPtr player, ref AVPPlayerState state);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetFlags(IntPtr player, int flags);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetAssetInfo(IntPtr player, ref AVPPlayerAssetInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetVideoTrackInfo(IntPtr player, int index, ref AVPPlayerVideoTrackInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetAudioTrackInfo(IntPtr player, int index, ref AVPPlayerAudioTrackInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetTextTrackInfo(IntPtr player, int index, ref AVPPlayerTextTrackInfo info);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetBufferedTimeRanges(IntPtr player, AVPPlayerTimeRange[] ranges, int count);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetSeekableTimeRanges(IntPtr player, AVPPlayerTimeRange[] ranges, int count);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetTexture(IntPtr player, ref AVPPlayerTexture texture);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerGetText(IntPtr player, ref AVPPlayerText text);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetPlayerSettings(IntPtr player, AVPPlayerSettings settings);
			
			[DllImport(PluginName)]
			[return: MarshalAs(UnmanagedType.U1)]
			internal static extern bool AVPPlayerOpenURL(IntPtr player, string url, string headers);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerClose(IntPtr player);

			[DllImport(PluginName)]
			internal static extern int AVPPlayerGetAudio(IntPtr player, float[] buffer, int length);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetRate(IntPtr player, float rate);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetVolume(IntPtr player, float volume);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetExternalPlaybackVideoGravity(IntPtr player, AVPPlayerExternalPlaybackVideoGravity gravity);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSeek(IntPtr player, double toTime, double toleranceBefore, double toleranceAfter);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetKeyServerAuthToken(IntPtr player, string token);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetKeyServerURL(IntPtr player, string url);

			[DllImport(PluginName)]
			internal static extern void AVPPlayerSetDecryptionKey(IntPtr player, byte[] key, int length);

			[DllImport(PluginName)]
			[return: MarshalAs(UnmanagedType.I1)]
			internal static extern bool AVPPlayerSetTrack(IntPtr player, AVPPlayerTrackType type, int index);

#if !UNITY_EDITOR && UNITY_IOS
			public struct MediaCachingOptions
			{
				public double minimumRequiredBitRate;
				public float  minimumRequiredResolution_width;
				public float  minimumRequiredResolution_height;
				public string title;
				public IntPtr artwork;
				public int    artworkLength;
			}

			[DllImport(PluginName)]
			public static extern void AVPPluginCacheMediaForURL(string url, string headers, MediaCachingOptions options);

			[DllImport(PluginName)]
			public static extern void AVPPluginCancelDownloadOfMediaForURL(string url);

			[DllImport(PluginName)]
			public static extern void AVPPluginRemoveCachedMediaForURL(string url);

			[DllImport(PluginName)]
			public static extern int AVPPluginGetCachedMediaStatusForURL(string url, ref float progress);
#endif
		}
	}
}

#endif
