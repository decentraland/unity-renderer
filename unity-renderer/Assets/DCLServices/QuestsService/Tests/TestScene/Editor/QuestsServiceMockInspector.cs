using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.QuestsService.Tests.TestScene.Editor
{
    [CustomEditor(typeof(QuestsServiceMock))]
    public class QuestsServiceMockInspector : UnityEditor.Editor
    {
        private VisualElement root;

        private QuestsServiceMock questService;

        private void OnEnable()
        {
            questService = (QuestsServiceMock)serializedObject.targetObject;
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            var defaultGUI = new IMGUIContainer(OnInspectorGUI);
            root.Add(defaultGUI);

            if (Application.isPlaying)
            {
                IntegerField amountField = new IntegerField("Add amount",2);
                var initButton = new Button(() => questService.EnqueueChanges(amountField.value))
                {
                    text = "Enqueue",
                };

                initButton.style.marginLeft = 10;
                var container = new UnityEngine.UIElements.VisualElement();
                container.style.flexDirection = FlexDirection.Row;
                container.Add(amountField);
                container.Add(initButton);
                root.Add(container);
            }

            return root;

        }
    }
}
