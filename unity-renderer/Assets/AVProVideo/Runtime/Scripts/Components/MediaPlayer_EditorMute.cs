using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2015-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProVideo
{
	public partial class MediaPlayer : MonoBehaviour
	{
#region Audio Mute Support for Unity Editor
#if UNITY_EDITOR
		private bool _unityAudioMasterMute = false;
		private void CheckEditorAudioMute()
		{
			// Detect a change
			if (UnityEditor.EditorUtility.audioMasterMute != _unityAudioMasterMute)
			{
				if (_controlInterface != null)
				{
					_unityAudioMasterMute = UnityEditor.EditorUtility.audioMasterMute;
					_controlInterface.MuteAudio(_audioMuted || _unityAudioMasterMute);
				}
			}
		}
#endif
#endregion // Audio Mute Support for Unity Editor
	}
}