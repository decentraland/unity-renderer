using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;

namespace UIComponents.CollapsableSortedList
{
    public class CollapsableSortedListComponentView<K, V> : BaseComponentView
        where V : Component
    {
        private readonly Dictionary<K, V> entries = new Dictionary<K, V>();
        private readonly List<K> sortedEntries = new List<K>();

        [SerializeField] private Transform container;
        [SerializeField] private CollapsableListToggleButton toggleButton;
        [SerializeField] private GameObject emptyStateContainer;
        [SerializeField] private CollapsableSortedListModel model;

        private int filteredCount;

        public Dictionary<K, V> Entries => entries;

        public Comparison<V> SortingMethod { get; set; } = (model, model1) => 0;

        public override void OnEnable()
        {
            base.OnEnable();
            UpdateEmptyState();
        }

        public override void RefreshControl()
        {
            Clear();
            
            if (model.isExpanded)
                Expand();
            else
                Collapse();

            if (model.isVisible)
                Show();
            else
                Hide();
        }

        public virtual bool Contains(K key) => entries.ContainsKey(key);

        public virtual void Expand()
        {
            toggleButton.Toggle(true);
            model.isExpanded = true;
        }

        public virtual void Collapse()
        {
            toggleButton.Toggle(false);
            model.isExpanded = false;
        }

        public virtual int Count() => entries.Count - filteredCount;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            model.isVisible = true;
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            model.isVisible = false;
        }

        public virtual V Get(K key) => entries[key];

        public virtual void Add(K key, V value)
        {
            if (entries.ContainsKey(key)) return;
            entries.Add(key, value);
            sortedEntries.Add(key);
            var entryTransform = value.transform;
            entryTransform.SetParent(container, false);
            entryTransform.localScale = Vector3.one;
            Sort();
            UpdateEmptyState();
            ((RectTransform) container).ForceUpdateLayout();
        }

        public virtual V Remove(K key)
        {
            if (!entries.ContainsKey(key)) return default;
            var entry = entries[key];
            entries.Remove(key);
            sortedEntries.RemoveAll(model => model.Equals(entry));
            UpdateEmptyState();
            ((RectTransform) container).ForceUpdateLayout();
            return entry;
        }

        public virtual void Clear()
        {
            foreach (var key in entries.Keys.ToList())
                Remove(key);

            entries.Clear();
            sortedEntries.Clear();
            UpdateEmptyState();
            ((RectTransform) container).ForceUpdateLayout();
        }

        public void Filter(Func<V, bool> comparision)
        {
            filteredCount = 0;

            foreach (var entry in entries)
            {
                var isMatch = comparision.Invoke(entry.Value);
                entry.Value.gameObject.SetActive(isMatch);

                if (!isMatch)
                    filteredCount++;
            }

            ((RectTransform) container).ForceUpdateLayout();
        }

        public virtual void Sort()
        {
            sortedEntries.Sort((k, y) => SortingMethod.Invoke(entries[k], entries[y]));
            foreach (var key in sortedEntries)
                entries[key].transform.SetAsLastSibling();
        }

        private void UpdateEmptyState() => emptyStateContainer.SetActive(Count() == 0);
    }
}