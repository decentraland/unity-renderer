using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace DCL.Components
{
    public class UIShapePool
    {
        private readonly string prefabPath;
        private readonly ObjectPool<UIReferencesContainer> pool;

        public UIShapePool(string prefabPath, int capacity = 3)
        {
            this.prefabPath = prefabPath;

            pool = new ObjectPool<UIReferencesContainer>(CreateUIShape, OnTakeShapeFromPool, OnReturnShapeToPool, OnDestroyShape, true, capacity);

            Debug.Log("VV::  PooL Created for " + prefabPath);
            List<UIReferencesContainer> prewarmList = new List<UIReferencesContainer>(capacity);
            for (var i = 0; i < capacity; i++)
                prewarmList.Add(pool.Get());

            for (var i = 0; i < capacity; i++)
                pool.Release(prewarmList[i]);
        }

        public UIReferencesContainer TakeUIShape() =>
            pool.Get();

        public void ReleaseUIShape(UIReferencesContainer uiShape) =>
            pool.Release(uiShape);

        private UIReferencesContainer CreateUIShape() =>
            Object.Instantiate(
                Resources.Load<UIReferencesContainer>(prefabPath), null, false);

        private void OnTakeShapeFromPool(UIReferencesContainer uiShape)
        {
            uiShape.gameObject.SetActive(true);
        }

        private void OnReturnShapeToPool(UIReferencesContainer uiShape)
        {
            uiShape.transform.SetParent(null, false);
            uiShape.gameObject.SetActive(false);
        }

        private void OnDestroyShape(UIReferencesContainer uiShape)
        {
            Object.Destroy(uiShape.gameObject);
        }
    }
}
