using DCL.Helpers;
using System;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;

namespace DCL.Backpack
{
    public class NftBreadcrumbComponentView : BaseComponentView<NftBreadcrumbModel>
    {
        [SerializeField] internal NftSubCategoryFilterComponentView prefab;
        [SerializeField] internal GameObject separatorPrefab;
        [SerializeField] internal RectTransform container;
        [SerializeField] internal RectTransform layoutContainer;
        [SerializeField] internal NFTTypeIconsAndColors iconsByCategory;

        private readonly Dictionary<NftSubCategoryFilterComponentView, PoolableObject> pooledObjects = new ();
        private readonly Dictionary<GameObject, PoolableObject> pooledSeparators = new ();
        private Pool pool;
        private Pool separatorsPool;
        private bool isLayoutDirty;

        internal NftBreadcrumbModel Model => model;

        public event Action<string> OnFilterSelected;
        public event Action<string> OnFilterRemoved;

        public override void Awake()
        {
            base.Awake();

            pool = PoolManager.i.AddPool(
                $"NftBreadcrumbComponentView_{GetInstanceID()}",
                Instantiate(prefab).gameObject,
                maxPrewarmCount: 5,
                isPersistent: true);

            separatorsPool = PoolManager.i.AddPool(
                $"NftBreadcrumbSeparator_{GetInstanceID()}",
                Instantiate(separatorPrefab),
                maxPrewarmCount: 5,
                isPersistent: true);
        }

        public override void RefreshControl()
        {
            ClearInstances();

            for (var i = 0; i < model.Path.Length; i++)
            {
                (string Filter, string Name, string Type, bool Removable) subCategory = model.Path[i];
                bool isLastItem = i == model.Path.Length - 1;
                bool isSelected = model.Current == i;

                CreateSubCategory(subCategory, false, isSelected);

                if (!isLastItem)
                    CreateSeparator();
            }

            isLayoutDirty = true;
        }

        private void Update()
        {
            if (!isLayoutDirty) return;
            isLayoutDirty = false;
            layoutContainer.ForceUpdateLayout(false);
        }

        private void CreateSeparator()
        {
            PoolableObject poolObj = separatorsPool.Get();
            GameObject separator = poolObj.gameObject;
            separator.transform.SetParent(container, false);
            pooledSeparators[separator] = poolObj;
        }

        private NftSubCategoryFilterComponentView CreateSubCategory((string Filter, string Name, string Type, bool Removable) subCategory,
            bool showResultCount, bool isSelected)
        {
            PoolableObject poolObj = pool.Get();
            NftSubCategoryFilterComponentView view = poolObj.gameObject.GetComponent<NftSubCategoryFilterComponentView>();
            Sprite icon = iconsByCategory.GetTypeImage(subCategory.Type);

            view.SetModel(new NftSubCategoryFilterModel
            {
                Name = subCategory.Name.Replace("_", " "),
                Filter = subCategory.Filter,
                ResultCount = model.ResultCount,
                ShowResultCount = showResultCount,
                Icon = icon,
                IsSelected = isSelected,
                ShowRemoveButton = subCategory.Removable,
            });

            view.OnNavigate += ApplyFilter;
            view.OnExit += RemoveFilter;
            view.transform.SetParent(container, false);

            pooledObjects[view] = poolObj;

            return view;
        }

        private void ClearInstances()
        {
            foreach ((NftSubCategoryFilterComponentView view, PoolableObject poolObj) in pooledObjects)
            {
                poolObj.Release();
                view.OnNavigate -= ApplyFilter;
                view.OnExit -= RemoveFilter;
            }

            pooledObjects.Clear();

            foreach ((GameObject go, PoolableObject poolObj) in pooledSeparators)
                poolObj.Release();

            pooledSeparators.Clear();
        }

        private void RemoveFilter(NftSubCategoryFilterModel model) =>
            OnFilterRemoved?.Invoke(model.Filter);

        private void ApplyFilter(NftSubCategoryFilterModel model) =>
            OnFilterSelected?.Invoke(model.Filter);
    }
}
