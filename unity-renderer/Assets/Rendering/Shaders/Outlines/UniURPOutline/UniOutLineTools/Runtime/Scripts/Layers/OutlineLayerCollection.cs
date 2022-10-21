using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UniOutline.Outline
{
	
	[CreateAssetMenu(fileName = "OutlineLayerCollection", menuName = "/TDTools/Custom/Shaders/Outline/Outline Layer Collection")]
	public sealed class OutlineLayerCollection : ScriptableObject, IList<OutlineLayer>, IReadOnlyList<OutlineLayer>
	{
		#region data

		[SerializeField]
		private List<OutlineLayer> _layers = new List<OutlineLayer>();
		[SerializeField]
		private int _ignoreLayerMask;

		#endregion

		#region interface

		// ignore layers
		public int IgnoreLayerMask
		{
			get
			{
				return _ignoreLayerMask;
			}
			set
			{
				if (_ignoreLayerMask != value)
				{
					_ignoreLayerMask = value;

					foreach (var layer in _layers)
					{
						layer.UpdateRenderers(value);
					}
				}
			}
		}

		// gameobjects in layers
		public int NumberOfObjects
		{
			get
			{
				var result = 0;

				foreach (var layer in _layers)
				{
					result += layer.Count;
				}

				return result;
			}
		}

		// indexed layers
		public OutlineLayer GetOrAddLayer(int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			while (index >= _layers.Count)
			{
				_layers.Add(new OutlineLayer(this));
			}

			return _layers[index];
		}

		// add a clean layer
		public OutlineLayer AddLayer()
		{
			var layer = new OutlineLayer(this);
			_layers.Add(layer);
			return layer;
		}

		// rendering objects GET
		public void GetRenderObjects(IList<OutlineRenderObject> renderObjects)
		{
			foreach (var layer in _layers)
			{
				layer.GetRenderObjects(renderObjects);
			}
		}

		// remove layers
		public void Remove(GameObject go)
		{
			foreach (var layer in _layers)
			{
				if (layer.Remove(go))
				{
					break;
				}
			}
		}

		// remove objects in layers
		public void ClearLayerContent()
		{
			foreach (var layer in _layers)
			{
				layer.Clear();
			}
		}

		#endregion

		#region secondary

		internal void Reset()
		{
			foreach (var layer in _layers)
			{
				layer.Reset();
			}
		}

		#endregion

		#region ScriptableObject

		private void OnEnable()
		{
			foreach (var layer in _layers)
			{
				layer.Clear();
				layer.SetCollection(this);
			}
		}

		#endregion

		#region IList

		// outline layer
		public OutlineLayer this[int layerIndex]
		{
			get
			{
				return _layers[layerIndex];
			}
			set
			{
				if (value is null)
				{
					throw new ArgumentNullException("layer");
				}

				if (layerIndex < 0 || layerIndex >= _layers.Count)
				{
					throw new ArgumentOutOfRangeException(nameof(layerIndex));
				}

				if (_layers[layerIndex] != value)
				{
					value.SetCollection(this);

					_layers[layerIndex].SetCollection(null);
					_layers[layerIndex] = value;
				}
			}
		}

		// indexofLayers
		public int IndexOf(OutlineLayer layer)
		{
			if (layer != null)
			{
				return _layers.IndexOf(layer);
			}

			return -1;
		}

		// inset a layer
		public void Insert(int index, OutlineLayer layer)
		{
			if (layer is null)
			{
				throw new ArgumentNullException(nameof(layer));
			}

			if (layer.ParentCollection != this)
			{
				layer.SetCollection(this);
				_layers.Insert(index, layer);
			}
		}

		// remove a layer
		public void RemoveAt(int index)
		{
			if (index >= 0 && index < _layers.Count)
			{
				_layers[index].SetCollection(null);
				_layers.RemoveAt(index);
			}
		}

		#endregion

		#region Collections ICollection

		// collections
		public int Count => _layers.Count;

		
		public bool IsReadOnly => false;

		
		public void Add(OutlineLayer layer)
		{
			if (layer is null)
			{
				throw new ArgumentNullException(nameof(layer));
			}

			if (layer.ParentCollection != this)
			{
				layer.SetCollection(this);
				_layers.Add(layer);
			}
		}

		
		public bool Remove(OutlineLayer layer)
		{
			if (_layers.Remove(layer))
			{
				layer.SetCollection(null);
				return true;
			}

			return false;
		}

		
		public void Clear()
		{
			if (_layers.Count > 0)
			{
				foreach (var layer in _layers)
				{
					layer.SetCollection(null);
				}

				_layers.Clear();
			}
		}

		
		public bool Contains(OutlineLayer layer)
		{
			if (layer is null)
			{
				return false;
			}

			return _layers.Contains(layer);
		}

		
		public void CopyTo(OutlineLayer[] array, int arrayIndex)
		{
			_layers.CopyTo(array, arrayIndex);
		}

		#endregion

		#region IEnumerable

		
		public IEnumerator<OutlineLayer> GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _layers.GetEnumerator();
		}

		#endregion

		#region implementation
		#endregion
	}
}
