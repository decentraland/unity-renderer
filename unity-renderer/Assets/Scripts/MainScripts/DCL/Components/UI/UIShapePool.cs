using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace DCL.Components
{
    public class UIShapePool
    {
        private readonly string prefabPath;
        private readonly ObjectPool<UIReferencesContainer> pool;
        private readonly Transform root;

        public UIShapePool(Transform root, string prefabPath, bool prewarm = false, int capacity = 20)
        {
            this.root = root;
            this.prefabPath = prefabPath;

            pool = new ObjectPool<UIReferencesContainer>(CreateUIShape, OnTakeShapeFromPool, OnReturnShapeToPool, OnDestroyShape, true, capacity);

            if (string.IsNullOrEmpty(prefabPath)) return;

            if (prewarm)
            {
                var prewarmList = new List<UIReferencesContainer>(capacity);

                for (var i = 0; i < capacity; i++)
                    prewarmList.Add(pool.Get());

                for (var i = 0; i < capacity; i++)
                    pool.Release(prewarmList[i]);
            }
        }

        public UIReferencesContainer TakeUIShape()
        {
            var uiShape = pool.Get();
            uiShape.gameObject.SetActive(true);

            return uiShape;
        }

        public UIReferencesContainer TakeUIShapeInsideParent(Transform parent)
        {
            var uiShape = pool.Get();

            uiShape.transform.SetParent(parent, false);
            uiShape.gameObject.SetActive(true);

            return uiShape;
        }

        public void ReleaseUIShape(UIReferencesContainer uiShape) =>
            pool.Release(uiShape);

        private UIReferencesContainer CreateUIShape() =>
            Object.Instantiate(
                Resources.Load<UIReferencesContainer>(prefabPath), root, false);

        private static void OnTakeShapeFromPool(UIReferencesContainer uiShape)
        {
        }

        private void OnReturnShapeToPool(UIReferencesContainer uiShape)
        {
            uiShape.transform.SetParent(root, false);
            uiShape.gameObject.SetActive(false);
        }

        private static void OnDestroyShape(UIReferencesContainer uiShape)
        {
            Object.Destroy(uiShape.gameObject);
        }
    }
}
