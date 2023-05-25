using DCL.Helpers;
using System;
using System.Collections.Generic;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

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
        private NftSubCategoryFilterComponentView[] categoriesByIndex = Array.Empty<NftSubCategoryFilterComponentView>();
        private readonly List<GameObject> separators = new ();
        private Pool pool;
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

            pool.ForcePrewarm();
        }

        public override void RefreshControl()
        {
            ClearInstances();

            categoriesByIndex = new NftSubCategoryFilterComponentView[model.Path.Length];

            for (var i = 0; i < model.Path.Length; i++)
            {
                (string Filter, string Name, string Type, bool Removable) subCategory = model.Path[i];
                bool isLastItem = i == model.Path.Length - 1;
                bool isSelected = model.Current == i;

                NftSubCategoryFilterComponentView subCategoryView =
                    CreateSubCategory(subCategory, isLastItem, isSelected);
                categoriesByIndex[i] = subCategoryView;

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
            GameObject separator = Instantiate(separatorPrefab, container, false);
            separators.Add(separator);
        }

        private NftSubCategoryFilterComponentView CreateSubCategory((string Filter, string Name, string Type, bool Removable) subCategory,
            bool isLastItem, bool isSelected)
        {
            PoolableObject poolObj = pool.Get();
            NftSubCategoryFilterComponentView view = poolObj.gameObject.GetComponent<NftSubCategoryFilterComponentView>();
            Sprite icon = iconsByCategory.GetTypeImage(subCategory.Type);

            view.SetModel(new NftSubCategoryFilterModel
            {
                Name = subCategory.Name.Replace("_", " "),
                Filter = subCategory.Filter,
                ResultCount = model.ResultCount,
                ShowResultCount = isLastItem,
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

            foreach (GameObject separator in separators)
                Destroy(separator);
            separators.Clear();
        }

        private void RemoveFilter(NftSubCategoryFilterModel model) =>
            OnFilterRemoved?.Invoke(model.Filter);

        private void ApplyFilter(NftSubCategoryFilterModel model) =>
            OnFilterSelected?.Invoke(model.Filter);
    }
}
