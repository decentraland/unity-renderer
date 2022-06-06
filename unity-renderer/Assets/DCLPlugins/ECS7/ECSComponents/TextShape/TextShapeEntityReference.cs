using DCL.Controllers;
using DCL.Models;
using TMPro;
using UnityEngine;

namespace DCL.ECS7.TextShape
{
    public class TextShapeEntityReference
    {
        private const string COMPONENT_NAME = "TextShape";

        internal GameObject textGameObject;
        internal TextMeshPro textComponent;
        internal RectTransform rectTransform;
        internal AssetPromise_Font promise;
        
        public TextShapeEntityReference(IParcelScene scene, IDCLEntity entity)
        {
            textGameObject = new GameObject(COMPONENT_NAME);
            textGameObject.AddComponent<MeshRenderer>();
            rectTransform = textGameObject.AddComponent<RectTransform>();
            textComponent = textGameObject.AddComponent<TextMeshPro>();
            textGameObject.transform.SetParent(scene.GetSceneTransform());
        }

        public void Dispose()
        {
            if (promise != null)
                fontPromiseKeeper.Forget(promise);
            GameObject.Destroy(textGameObject);
            GameObject.Destroy(rectTransform);
            GameObject.Destroy(textComponent);
        
            textGameObject = null;
            textComponent = null;
            rectTransform = null;
            currentModel = null;
        }
    }
}