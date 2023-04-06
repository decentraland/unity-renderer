using System;
using System.Collections;
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

        [SerializeField] internal Transform container;
        [SerializeField] internal CollapsableListToggleButton toggleButton;
        [SerializeField] internal GameObject emptyStateContainer;
        [SerializeField] private CollapsableSortedListModel model;

        private int filteredCount;
        private bool isLayoutDirty;

        public Dictionary<K, V> Entries => entries;

        public Comparison<V> SortingMethod { get; set; } = (model, model1) => 0;

        public override bool isVisible => gameObject.activeInHierarchy;

        public override void OnEnable()
        {
            base.OnEnable();
            UpdateEmptyState();
            UpdateLayout();
        }

        public void Update()
        {
            if (isLayoutDirty)
                Utils.ForceRebuildLayoutImmediate((RectTransform) container);
            isLayoutDirty = false;
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

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            gameObject.SetActive(true);
            model.isVisible = true;
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            gameObject.SetActive(false);
            model.isVisible = false;
        }

        public virtual V Get(K key) => entries.TryGetValue(key, out var value) ? value : null;

        public virtual void Add(K key, V value)
        {
            if (entries.ContainsKey(key)) return;
            entries.Add(key, value);
            sortedEntries.Add(key);
            var entryTransform = value.transform;
            entryTransform.SetParent(container, false);
            entryTransform.localScale = Vector3.one;
            UpdateEmptyState();
            UpdateLayout();
        }

        public virtual V Remove(K key)
        {
            if (!entries.ContainsKey(key)) return default;
            var entry = entries[key];
            entries.Remove(key);
            sortedEntries.RemoveAll(k => k.Equals(key));
            UpdateEmptyState();
            UpdateLayout();
            return entry;
        }

        public virtual void Clear()
        {
            foreach (var key in entries.Keys.ToList())
                Remove(key);

            entries.Clear();
            sortedEntries.Clear();
            UpdateEmptyState();
            UpdateLayout();
        }

        public virtual void Filter(Func<V, bool> comparision)
        {
            filteredCount = 0;

            foreach (var entry in entries)
            {
                var isMatch = comparision.Invoke(entry.Value);
                entry.Value.gameObject.SetActive(isMatch);

                if (!isMatch)
                    filteredCount++;
            }

            UpdateLayout();
            UpdateEmptyState();
        }

        public IEnumerator FilterAsync(Func<V, bool> comparision, int throttlingBudget = 10)
        {
            filteredCount = 0;
            var iterations = 0;

            foreach (var entry in entries)
            {
                iterations++;
                if (iterations % throttlingBudget == 0)
                    yield return null;

                var isMatch = comparision.Invoke(entry.Value);
                entry.Value.gameObject.SetActive(isMatch);

                if (!isMatch)
                    filteredCount++;
            }

            UpdateLayout();
            UpdateEmptyState();
        }

        public virtual void Sort()
        {
            sortedEntries.Sort((k, y) => SortingMethod.Invoke(entries[k], entries[y]));
            for (var i = 0; i < sortedEntries.Count; i++)
            {
                var key = sortedEntries[i];
                entries[key].transform.SetAsLastSibling();
            }
        }

        protected virtual void UpdateEmptyState()
        {
            if (emptyStateContainer == null) return;
            emptyStateContainer.SetActive(Count() == 0);
        }

        private void UpdateLayout() => isLayoutDirty = true;
    }
}
