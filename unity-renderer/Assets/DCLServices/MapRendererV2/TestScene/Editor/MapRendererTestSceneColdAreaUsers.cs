using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneColdAreaUsers : IMapRendererTestSceneElementProvider
    {
        private readonly BaseVariable<string> realmName;
        private readonly Vector2IntVariable userPosition;
        private readonly MapRendererTestSceneHotScenesController hotScenesController;

        public MapRendererTestSceneColdAreaUsers(
            BaseVariable<string> realmName,
            Vector2IntVariable userPosition,
            MapRendererTestSceneHotScenesController hotScenesController)
        {
            this.realmName = realmName;
            this.userPosition = userPosition;
            this.hotScenesController = hotScenesController;
        }

        public VisualElement GetElement()
        {
            var root = new VisualElement();
            root.Add(CreateRealName());
            root.Add(CreateUserPosition());
            root.Add(CreateHotScenes());
            return root;
        }

        private VisualElement CreateRealName()
        {
            var field = new TextField("Realm name");
            field.binding = new BaseVariableBinding<string>(realmName, field);
            return field;
        }

        private VisualElement CreateUserPosition()
        {
            var field = new Vector2IntField("User coords");
            field.binding = new BaseVariableAssetBinding<Vector2Int>(userPosition, field);
            return field;
        }

        private VisualElement CreateHotScenes()
        {
            var root = new VisualElement();

            var so = new SerializedObject(hotScenesController);
            var prop = new PropertyField(so.FindProperty("sceneInfos"));
            prop.Bind(so);

            root.Add(prop);
            root.Add(new Button(() => hotScenesController.EmitScenesInfo()) {text = "Emit Changes"});
            return root;
        }
    }
}
