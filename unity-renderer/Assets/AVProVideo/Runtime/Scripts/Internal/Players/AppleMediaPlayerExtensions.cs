//-----------------------------------------------------------------------------
// Copyright 2015-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

#if UNITY_2017_2_OR_NEWER && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || (!UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS)))

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RenderHeads.Media.AVProVideo
{
	internal static class AppleMediaPlayerExtensions
	{
		// AVPPlayerStatus

		internal static bool IsReadyToPlay(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.ReadyToPlay) == AppleMediaPlayer.Native.AVPPlayerStatus.ReadyToPlay;
		}

		internal static bool IsPlaying(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Playing) == AppleMediaPlayer.Native.AVPPlayerStatus.Playing;
		}

		internal static bool IsPaused(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Paused) == AppleMediaPlayer.Native.AVPPlayerStatus.Paused;
		}

		internal static bool IsFinished(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Finished) == AppleMediaPlayer.Native.AVPPlayerStatus.Finished;
		}

		internal static bool IsSeeking(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Seeking) == AppleMediaPlayer.Native.AVPPlayerStatus.Seeking;
		}

		internal static bool IsBuffering(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Buffering) == AppleMediaPlayer.Native.AVPPlayerStatus.Buffering;
		}

		internal static bool IsStalled(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Stalled) == AppleMediaPlayer.Native.AVPPlayerStatus.Stalled;
		}

		internal static bool IsExternalPlaybackActive(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.ExternalPlaybackActive) == AppleMediaPlayer.Native.AVPPlayerStatus.ExternalPlaybackActive;
		}

		internal static bool IsCached(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Cached) == AppleMediaPlayer.Native.AVPPlayerStatus.Cached;
		}

		internal static bool HasFinishedSeeking(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.FinishedSeeking) == AppleMediaPlayer.Native.AVPPlayerStatus.FinishedSeeking;
		}

		internal static bool HasUpdatedAssetInfo(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedAssetInfo) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedAssetInfo;
		}

		internal static bool HasUpdatedTexture(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedTexture) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedTexture;
		}

		internal static bool HasUpdatedBufferedTimeRanges(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedBufferedTimeRanges) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedBufferedTimeRanges;
		}

		internal static bool HasUpdatedSeekableTimeRanges(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedSeekableTimeRanges) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedSeekableTimeRanges;
		}

		internal static bool HasUpdatedText(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedText) == AppleMediaPlayer.Native.AVPPlayerStatus.UpdatedText;
		}

		internal static bool HasVideo(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasVideo) == AppleMediaPlayer.Native.AVPPlayerStatus.HasVideo;
		}

		internal static bool HasAudio(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasAudio) == AppleMediaPlayer.Native.AVPPlayerStatus.HasAudio;
		}

		internal static bool HasText(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasText) == AppleMediaPlayer.Native.AVPPlayerStatus.HasText;
		}

		internal static bool HasMetadata(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.HasMetadata) == AppleMediaPlayer.Native.AVPPlayerStatus.HasMetadata;
		}

		internal static bool HasFailed(this AppleMediaPlayer.Native.AVPPlayerStatus status)
		{
			return (status & AppleMediaPlayer.Native.AVPPlayerStatus.Failed) == AppleMediaPlayer.Native.AVPPlayerStatus.Failed;
		}

		// AVPPlayerFlags

		internal static bool IsLooping(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.Looping) == AppleMediaPlayer.Native.AVPPlayerFlags.Looping;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetLooping(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsLooping() ^ b)
			{
				flags = (b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.Looping
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.Looping) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool IsMuted(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.Muted) == AppleMediaPlayer.Native.AVPPlayerFlags.Muted;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetMuted(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsMuted() ^ b)
			{
				flags = (b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.Muted
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.Muted) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool IsExternalPlaybackAllowed(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback) == AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetAllowExternalPlayback(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsExternalPlaybackAllowed() ^ b)
			{
				flags = (b ? flags |  AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.AllowExternalPlayback) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool ResumePlayback(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback) == AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetResumePlayback(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.ResumePlayback() ^ b)
			{
				flags = (b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback
				           : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.ResumePlayback) | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		internal static bool IsDirty(this AppleMediaPlayer.Native.AVPPlayerFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerFlags.Dirty) == AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
		}

		internal static AppleMediaPlayer.Native.AVPPlayerFlags SetDirty(this AppleMediaPlayer.Native.AVPPlayerFlags flags, bool b)
		{
			if (flags.IsDirty() ^ b)
			{
				flags = b ? flags | AppleMediaPlayer.Native.AVPPlayerFlags.Dirty : flags & ~AppleMediaPlayer.Native.AVPPlayerFlags.Dirty;
			}
			return flags;
		}

		// MARK: AVPPlayerAssetFlags

		internal static bool IsCompatibleWithAirPlay(this AppleMediaPlayer.Native.AVPPlayerAssetFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerAssetFlags.CompatibleWithAirPlay) == AppleMediaPlayer.Native.AVPPlayerAssetFlags.CompatibleWithAirPlay;
		}

		// MARK: AVPPlayerTrackFlags

		internal static bool IsDefault(this AppleMediaPlayer.Native.AVPPlayerTrackFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTrackFlags.Default) == AppleMediaPlayer.Native.AVPPlayerTrackFlags.Default;
		}

		// AVPPlayerTextureFlags

		internal static bool IsFlipped(this AppleMediaPlayer.Native.AVPPlayerTextureFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTextureFlags.Flipped) == AppleMediaPlayer.Native.AVPPlayerTextureFlags.Flipped;
		}

		internal static bool IsLinear(this AppleMediaPlayer.Native.AVPPlayerTextureFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTextureFlags.Linear) == AppleMediaPlayer.Native.AVPPlayerTextureFlags.Linear;
		}

		internal static bool IsMipmapped(this AppleMediaPlayer.Native.AVPPlayerTextureFlags flags)
		{
			return (flags & AppleMediaPlayer.Native.AVPPlayerTextureFlags.Mipmapped) == AppleMediaPlayer.Native.AVPPlayerTextureFlags.Mipmapped;
		}
	}
}

#endif
