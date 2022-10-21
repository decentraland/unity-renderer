using System;

using System.ComponentModel;

using System.Runtime.CompilerServices;

using UnityEngine;


namespace UniOutline.Outline
{
	
	// Extension methods for the Renderer class.
	
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class OutlineSettingsExtensions
	{
		
		// Gets a value indicating whether outline should use alpha testing.
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlphaTestingEnabled(this IOutlineSettings settings)
		{
			return (settings.OutlineRenderMode & OutlineRenderFlags.EnableAlphaTesting) != 0;
		}

		
		// Gets a value indicating whether outline should use depth testing.
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDepthTestingEnabled(this IOutlineSettings settings)
		{
			return (settings.OutlineRenderMode & OutlineRenderFlags.EnableDepthTesting) != 0;
		}

		
		// Gets a value indicating whether outline frame should be blurred.
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsBlurEnabled(this IOutlineSettings settings)
		{
			return (settings.OutlineRenderMode & OutlineRenderFlags.Blurred) != 0;
		}
	}
}
