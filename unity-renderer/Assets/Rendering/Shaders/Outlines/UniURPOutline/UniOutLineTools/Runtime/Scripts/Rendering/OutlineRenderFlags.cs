using System;

namespace UniOutline.Outline
{
	
	// Enumerates outline render modes.
	
	[Flags]
	public enum OutlineRenderFlags
	{
		// Outline frame is a solid line.
		Standard = 0,
		
		// Outline frame is blurred.
		Blurred = 1,
		
		// Enables depth testing when rendering object outlines. Only visible parts of objects are outlined.
		Occluded = 2,
		
		// Enabled alpha testing when rendering outlines.
		Alphas = 4,
		EnableDepthTesting,
		EnableAlphaTesting,
		None
	}
}
