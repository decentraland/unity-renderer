using Cysharp.Threading.Tasks;
using DCL;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    [CustomEditor(typeof(MapRendererTestScene))]
    public class MapRendererTestSceneInspector : Editor
    {
        private MapRendererTestScene testScene;

        private static IReadOnlyList<IMapRendererTestSceneElementProvider> elementProviders;

        private VisualElement root;

        private void OnEnable()
        {
            testScene = (MapRendererTestScene)serializedObject.targetObject;
        }

        private async UniTaskVoid Initialize()
        {
            if (testScene.initialized)
                return;

            var (serviceLocator, elementsProviders) =
                MapRendererTestSceneServiceLocatorFactory.Create(testScene.container, testScene.parcelSize, testScene.atlasChunkSize);

            elementProviders = elementsProviders;

            testScene.initialized = true;

            await Environment.SetupAsync(serviceLocator);

            DrawControls();
        }

        private void DrawControls()
        {
            foreach (IMapRendererTestSceneElementProvider elementProvider in elementProviders)
            {
                var group = new VisualElement();
                group.AddToClassList(MapRendererTestSceneStyles.FUNCTION_GROUP);

                var title = new Label
                {
                    text = elementProvider.GetType().Name
                };

                title.AddToClassList(MapRendererTestSceneStyles.GROUP_TITLE);

                group.Add(title);

                group.Add(elementProvider.GetElement());
                root.Add(group);
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            var defaultGUI = new IMGUIContainer(OnInspectorGUI);
            root.Add(defaultGUI);

            MapRendererTestSceneStyles.SetStyleSheet(root);

            if (Application.isPlaying)
            {
                if (testScene.initialized)
                {
                    DrawControls();
                }
                else
                {
                    var initButton = new Button(() => Initialize().Forget()) { text = "Initialize" };
                    root.Add(initButton);
                }
            }

            return root;
        }
    }
}
