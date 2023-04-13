using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestScenePlayerMarker : IMapRendererTestSceneElementProvider
    {
        private readonly BaseVariable<Vector3> playerWorldPosition;
        private readonly Vector3Variable playerRotation;

        public MapRendererTestScenePlayerMarker(BaseVariable<Vector3> playerWorldPosition, Vector3Variable playerRotation)
        {
            this.playerWorldPosition = playerWorldPosition;
            this.playerRotation = playerRotation;
        }

        public VisualElement GetElement()
        {
            var root = new VisualElement();

            var worldPosField = new Vector3Field("Player World Position");
            worldPosField.SetValueWithoutNotify(playerWorldPosition.Get());

            playerWorldPosition.OnChange += (c, p) => worldPosField.SetValueWithoutNotify(c);

            worldPosField.RegisterValueChangedCallback(evt => playerWorldPosition.Set(evt.newValue));

            root.Add(worldPosField);

            var rotField = new Vector3Field("Player Rotation");
            rotField.SetValueWithoutNotify(playerRotation.Get());

            playerRotation.OnChange += (c, p) => rotField.SetValueWithoutNotify(c);

            rotField.RegisterValueChangedCallback(evt => playerRotation.Set(evt.newValue));

            root.Add(rotField);

            return root;
        }
    }
}
