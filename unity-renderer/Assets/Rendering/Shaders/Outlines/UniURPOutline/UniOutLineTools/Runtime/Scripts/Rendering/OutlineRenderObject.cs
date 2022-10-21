using System;

using System.Collections;
using System.Collections.Generic;

using System.Text;

using UnityEngine;


namespace UniOutline.Outline
{
	
	// A single outline object + its outline settings.
	
	public readonly struct OutlineRenderObject : IEquatable<OutlineRenderObject>
	{
		#region data

		private readonly string _tag;
		private readonly IReadOnlyList<Renderer> _renderers;
		private readonly IOutlineSettings _outlineSettings;

		#endregion

		#region interface

		
		// Gets the object tag name.
		
		public string Tag => _tag;

		
		// Gets renderers for the object.
		
		public IReadOnlyList<Renderer> Renderers => _renderers;

		
		// Gets outline settings for this object.
		
		public IOutlineSettings OutlineSettings => _outlineSettings;

		
		// Initializes a new instance of the <see cref="OutlineRenderObject"/> struct.
		
		public OutlineRenderObject(IReadOnlyList<Renderer> renderers, IOutlineSettings outlineSettings, string tag = null)
		{
			_renderers = renderers;
			_outlineSettings = outlineSettings;
			_tag = tag;
		}

		#endregion

		#region IEquatable

		
		public bool Equals(OutlineRenderObject other)
		{
			return string.CompareOrdinal(_tag, other._tag) == 0 && _renderers == other._renderers && _outlineSettings == other._outlineSettings;
		}

		#endregion
	}
}
