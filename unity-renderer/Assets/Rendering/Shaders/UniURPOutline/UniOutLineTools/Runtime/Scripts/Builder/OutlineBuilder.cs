using System;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace UniOutline.Outline
{
	// Token: 0x0200000A RID: 10
	public sealed class OutlineBuilder : MonoBehaviour
	{
		#region data

		[Serializable]
		public class ContentItem
		{
			public GameObject Go;
			public int LayerIndex;
		}

#pragma warning disable 0649

		[SerializeField, Tooltip(OutlineResources.OutlineLayerCollectionTooltip)]
		private OutlineLayerCollection _outlineLayers;
		[SerializeField, HideInInspector]
		private List<ContentItem> _content;

#pragma warning restore 0649

		#endregion

		#region interface

		public List<ContentItem> Content { get => _content; set => _content = value; }

		// Managing layer content
		public OutlineLayerCollection OutlineLayers { get => _outlineLayers; set => _outlineLayers = value; }

		// clear all content
		public void Clear()
		{
			_outlineLayers?.ClearLayerContent();
		}

		#endregion

		#region MonoBehaviour

		private void OnEnable()
		{
			if (_outlineLayers && _content != null)
			{
				foreach (var item in _content)
				{
					if (item.LayerIndex >= 0 && item.LayerIndex < _outlineLayers.Count && item.Go)
					{
						_outlineLayers.GetOrAddLayer(item.LayerIndex).Add(item.Go);
					}
				}
			}
		}

#if UNITY_EDITOR

		private void Reset()
		{
			var effect = GetComponent<OutlineEffect>();

			if (effect)
			{
				_outlineLayers = effect.OutlineLayersInternal;
			}
		}

		private void OnDestroy()
		{
			_outlineLayers?.ClearLayerContent();
		}

#endif

		#endregion
	}
}
