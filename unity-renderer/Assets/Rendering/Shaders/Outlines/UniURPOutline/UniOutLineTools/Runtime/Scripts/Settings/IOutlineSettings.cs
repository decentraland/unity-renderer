using System;
using UniOutline.Outline;
using UnityEngine;

namespace UniOutline.Outline
{
	
	//Outline settings.
	
	public interface IOutlineSettings : IEquatable<IOutlineSettings>
	{
		
		// Gets or sets outline color.
		
		Color OutlineColor { get; set; }

		
		// Gets or sets outline width in pixels. 
		int OutlineWidth { get; set; }

		
		/// Gets or sets outline intensity value. 
		/// This is used for blurred oulines only and is ignored for solid outlines.
		
		float OutlineIntensity { get; set; }

		
		/// Gets or sets alpha cutoff value. Allowed range is [0, 1]. 
		
		float OutlineAlphaCutoff { get; set; }

		
		/// Gets or sets outline render mode.
		
		OutlineRenderFlags OutlineRenderMode { get; set; }
	}
}
