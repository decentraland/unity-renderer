using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneHomePoint : IMapRendererTestSceneElementProvider
    {
        private readonly BaseVariable<Vector2Int> homePointCoordinates;

        public MapRendererTestSceneHomePoint(BaseVariable<Vector2Int> homePointCoordinates)
        {
            this.homePointCoordinates = homePointCoordinates;
        }

        public VisualElement GetElement()
        {
            var field = new Vector2IntField("Home Point");
            field.SetValueWithoutNotify(homePointCoordinates.Get());

            homePointCoordinates.OnChange += (c, p) => field.SetValueWithoutNotify(c);

            field.RegisterValueChangedCallback(evt => homePointCoordinates.Set(evt.newValue));
            return field;
        }
    }
}
