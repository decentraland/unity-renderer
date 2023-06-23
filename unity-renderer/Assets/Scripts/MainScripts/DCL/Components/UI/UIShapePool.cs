using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace DCL.Components
{
    public class UIShapePool
    {
        private readonly string prefabPath;
        private readonly ObjectPool<UIReferencesContainer> pool;

        public UIShapePool(string prefabPath, int capacity = 100)
        {
            this.prefabPath = prefabPath;
            Debug.Log("VV::  PooL Created for " + prefabPath);

            pool = new ObjectPool<UIReferencesContainer>(CreateUIShape, OnTakeShapeFromPool, OnReturnShapeToPool, OnDestroyShape, true, capacity);

            for (int i = 0; i < 100; i++)
            {
                pool.Get();
            }
            // for (int i = 0; i < 100; i++) pool.Release();
        }

        public UIReferencesContainer TakeUIShape() =>
            pool.Get();

        public void ReleaseUIShape(UIReferencesContainer uiShape) =>
            pool.Release(uiShape);

        private UIReferencesContainer CreateUIShape()
        {
            Debug.Log("VV::  PooL Instantiating");
            var obj = Object.Instantiate(Resources.Load<UIReferencesContainer>(prefabPath));
            obj.transform.parent = null;
            return obj;
        }

        private void OnTakeShapeFromPool(UIReferencesContainer uiShape)
        {
            Debug.Log("VV:: Pool taking", uiShape.gameObject);

            uiShape.gameObject.SetActive(true);
        }

        private void OnReturnShapeToPool(UIReferencesContainer uiShape)
        {
            uiShape.transform.parent = null;
            uiShape.gameObject.SetActive(false);
            Debug.Log("VV:: Pool released", uiShape.gameObject);
        }

        private void OnDestroyShape(UIReferencesContainer uiShape)
        {
            Object.Destroy(uiShape.gameObject);
        }
    }
}
