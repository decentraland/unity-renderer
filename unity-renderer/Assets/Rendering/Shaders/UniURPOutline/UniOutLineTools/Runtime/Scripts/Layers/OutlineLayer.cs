using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace UniOutline.Outline
{
	
	[Serializable]
	public sealed class OutlineLayer : ICollection<GameObject>, IReadOnlyCollection<GameObject>, IOutlineSettings
	{
		#region data

		[SerializeField, HideInInspector]
		private OutlineSettingsInstance _settings = new OutlineSettingsInstance();
		[SerializeField, HideInInspector]
		private string _name;
		[SerializeField, HideInInspector]
		private bool _enabled = true;
		[SerializeField, HideInInspector]
		private bool _mergeLayerObjects;

		private OutlineLayerCollection _parentCollection;
		private Dictionary<GameObject, OutlineRendererCollection> _outlineObjects = new Dictionary<GameObject, OutlineRendererCollection>();
		private List<Renderer> _mergedRenderers;

		#endregion

		#region interface

		
		public string Name
		{
			get
			{
				if (string.IsNullOrEmpty(_name))
				{
					return "OutlineLayer #" + Index.ToString();
				}

				return _name;
			}
		}

		
		public bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		
		public bool MergeLayerObjects
		{
			get
			{
				if (_mergeLayerObjects)
				{
					return true;
				}
				return false;
			}
			set
			{
				_mergeLayerObjects = value;
			}
		}

		
		public int Index
		{
			get
			{
				if (_parentCollection != null)
				{
					return _parentCollection.IndexOf(this);
				}

				return -1;
			}
		}

		
		public OutlineSettings OutlineSettings
		{
			get
			{
				return _settings.OutlineSettings;
			}
			set
			{
				_settings.OutlineSettings = value;
			}
		}


		private OutlineLayer()
		{
		}

		public static OutlineLayer CreateInstance()
		{
			return new OutlineLayer();
		}


		internal OutlineLayer(OutlineLayerCollection parentCollection)
		{
			_parentCollection = parentCollection;
		}

		
		public OutlineLayer(string name)
		{
			_name = name;
		}

		
		public OutlineLayer(OutlineSettings settings)
		{
			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_settings.OutlineSettings = settings;
		}

		
		public OutlineLayer(string name, OutlineSettings settings)
		{
			if (settings is null)
			{
				throw new ArgumentNullException(nameof(settings));
			}

			_name = name;
			_settings.OutlineSettings = settings;
		}

		
		public bool TryGetRenderers(GameObject go, out ICollection<Renderer> renderers)
		{
			if (go is null)
			{
				throw new ArgumentNullException(nameof(go));
			}

			if (_outlineObjects.TryGetValue(go, out var result))
			{
				renderers = result;
				return true;
			}

			renderers = null;
			return false;
		}

		
		// Gets the objects for rendering.
		
		public void GetRenderObjects(IList<OutlineRenderObject> renderObjects)
		{
			if (_enabled)
			{
				if (_mergeLayerObjects)
				{
					renderObjects.Add(new OutlineRenderObject(GetRenderers(), this, Name));
				}
				else
				{
					foreach (var kvp in _outlineObjects)
					{
						var go = kvp.Key;

						if (go && go.activeInHierarchy)
						{
							renderObjects.Add(new OutlineRenderObject(kvp.Value.GetList(), _settings, go.name));
						}
					}
				}
			}
		}

		
		// Gets all layer renderers.
		
		public IReadOnlyList<Renderer> GetRenderers()
		{
			if (_enabled)
			{
				if (_mergedRenderers != null)
				{
					_mergedRenderers.Clear();
				}
				else
				{
					_mergedRenderers = new List<Renderer>();
				}

				foreach (var kvp in _outlineObjects)
				{
					var go = kvp.Key;

					if (go && go.activeInHierarchy)
					{
						var rl = kvp.Value.GetList();

						for (var i = 0; i < rl.Count; i++)
						{
							_mergedRenderers.Add(rl[i]);
						}
					}
				}

				return _mergedRenderers;
			}

			return Array.Empty<Renderer>();
		}

		#endregion

		#region internals

		public string NameTag
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		internal OutlineLayerCollection ParentCollection => _parentCollection;

		internal void UpdateRenderers(int ignoreLayers)
		{
			foreach (var renderers in _outlineObjects.Values)
			{
				renderers.Reset(false, ignoreLayers);
			}
		}

		internal void Reset()
		{
			_outlineObjects.Clear();
		}

		internal void SetCollection(OutlineLayerCollection collection)
		{
			if (_parentCollection == null || collection == null || _parentCollection == collection)
			{
				_parentCollection = collection;
			}
			else
			{
				throw new InvalidOperationException("OutlineLayer can only belong to a single OutlineLayerCollection.");
			}
		}

		#endregion

		#region IOutlineSettings

		
		public Color OutlineColor
		{
			get
			{
				return _settings.OutlineColor;
			}
			set
			{
				_settings.OutlineColor = value;
			}
		}

		
		public int OutlineWidth
		{
			get
			{
				return _settings.OutlineWidth;
			}
			set
			{
				_settings.OutlineWidth = value;
			}
		}

		
		public float OutlineIntensity
		{
			get
			{
				return _settings.OutlineIntensity;
			}
			set
			{
				_settings.OutlineIntensity = value;
			}
		}

		
		public float OutlineAlphaCutoff
		{
			get
			{
				return _settings.OutlineAlphaCutoff;
			}
			set
			{
				_settings.OutlineAlphaCutoff = value;
			}
		}

		
		public OutlineRenderFlags OutlineRenderMode
		{
			get
			{
				return _settings.OutlineRenderMode;
			}
			set
			{
				_settings.OutlineRenderMode = value;
			}
		}

		#endregion

		#region ICollection

		
		public int Count => _outlineObjects.Count;

		
		public bool IsReadOnly => false;

		
		public void Add(GameObject go)
		{
			if (go is null)
			{
				throw new ArgumentNullException(nameof(go));
			}

			if (!_outlineObjects.ContainsKey(go))
			{
				var renderers = new OutlineRendererCollection(go);
				renderers.Reset(false, _parentCollection.IgnoreLayerMask);
				_outlineObjects.Add(go, renderers);
			}
		}

		
		public bool Remove(GameObject go)
		{
			if (go is null)
			{
				return false;
			}

			return _outlineObjects.Remove(go);
		}

		
		public bool Contains(GameObject go)
		{
			if (go is null)
			{
				return false;
			}

			return _outlineObjects.ContainsKey(go);
		}

		
		public void Clear()
		{
			_outlineObjects.Clear();
		}

		
		public void CopyTo(GameObject[] array, int arrayIndex)
		{
			_outlineObjects.Keys.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		
		public IEnumerator<GameObject> GetEnumerator()
		{
			return _outlineObjects.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _outlineObjects.Keys.GetEnumerator();
		}

		#endregion

		#region IEquatable

		
		public bool Equals(IOutlineSettings other)
		{
			return OutlineSettings.Equals(this, other);
		}

		#endregion

		#region Object

		
		public override string ToString()
		{
			var text = new StringBuilder();

			if (string.IsNullOrEmpty(_name))
			{
				text.Append("OutlineLayer");
			}
			else
			{
				text.Append(_name);
			}

			if (_parentCollection != null)
			{
				text.Append(" #");
				text.Append(_parentCollection.IndexOf(this));
			}

			if (_outlineObjects.Count > 0)
			{
				text.Append(" (");

				foreach (var go in _outlineObjects.Keys)
				{
					text.Append(go.name);
					text.Append(", ");
				}

				text.Remove(text.Length - 2, 2);
				text.Append(")");
			}

			return string.Format("{0}", text);
		}

		
		public override bool Equals(object other)
		{
			return OutlineSettings.Equals(this, other as IOutlineSettings);
		}

		
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#region implementation
		#endregion
	}
}
