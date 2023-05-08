using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.QuestsService.Tests.TestScene.Editor
{
    [CustomEditor(typeof(ClientQuestsServiceMock))]
    public class ClientQuestsServiceMockMockInspector : UnityEditor.Editor
    {
        private VisualElement root;

        private ClientQuestsServiceMock clientQuestService;

        private void OnEnable()
        {
            clientQuestService = (ClientQuestsServiceMock)serializedObject.targetObject;
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            var defaultGUI = new IMGUIContainer(OnInspectorGUI);
            root.Add(defaultGUI);

            if (Application.isPlaying)
            {
                IntegerField amountField = new IntegerField("Add amount",2);
                var initButton = new Button(() => clientQuestService.EnqueueChanges(amountField.value))
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
