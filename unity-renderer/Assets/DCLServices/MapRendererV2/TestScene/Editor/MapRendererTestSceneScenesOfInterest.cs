using DCLServices.MapRendererV2.MapLayers.PointsOfInterest;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneScenesOfInterest : IMapRendererTestSceneElementProvider
    {
        private readonly MinimapMetadata minimapMetadata;
        private readonly MapRenderer mapRenderer;

        private VisualElement existingScenesOfInterest;

        private string name = "fancy scene name";
        private readonly List<ListViewValueTypeContainer<Vector2Int>> parcels = new ();
        private readonly Dictionary<VisualElement, EventCallback<ChangeEvent<Vector2Int>>> callbacks = new ();

        public MapRendererTestSceneScenesOfInterest(MinimapMetadata minimapMetadata, MapRenderer mapRenderer)
        {
            this.minimapMetadata = minimapMetadata;
            this.mapRenderer = mapRenderer;
        }

        public VisualElement GetElement()
        {
            var root = new VisualElement();

            var nameField = new TextField("scene name") {value = name};
            nameField.RegisterValueChangedCallback(s => name = s.newValue);

            root.Add(nameField);

            var parcels = new VisualElement();
            parcels.AddToClassList(MapRendererTestSceneStyles.FUNCTION_GROUP);

            var title = new Label("Parcels");
            title.AddToClassList(MapRendererTestSceneStyles.GROUP_TITLE);

            parcels.Add(title);

            var parcelsView = new ListView(this.parcels, 18, makeItem: () => new Vector2IntField("parcel"), bindItem:
                (element, i) =>
                {
                    // TODO does not work properly, find how to make it work
                    var v2Field = (Vector2IntField)element;
                    v2Field.SetValueWithoutNotify(this.parcels[i]);

                    EventCallback<ChangeEvent<Vector2Int>> callback = evt => this.parcels[i] = evt.newValue;
                    callbacks[v2Field] = callback;
                    v2Field.RegisterValueChangedCallback(callback);
                });

            parcelsView.unbindItem = (element, i) =>
            {
                if (callbacks.TryGetValue(element, out var callback))
                    ((Vector2IntField)element).UnregisterValueChangedCallback(callback);

                callbacks.Remove(element);
            };

            parcelsView.showAddRemoveFooter = true;
            parcelsView.showBoundCollectionSize = true;

            parcels.Add(parcelsView);
            root.Add(parcels);

            var addButton = new Button(() =>
            {
                minimapMetadata.AddSceneInfo(new MinimapMetadata.MinimapSceneInfo
                {
                    name = this.name,
                    isPOI = true,
                    parcels = this.parcels.Select(p => (Vector2Int) p).ToList(),
                    description = "",
                    owner = "",
                    previewImageUrl = ""
                });

                RedrawScenesOfInterest();

            }) { text = "Add" };
            root.Add(addButton);

            RedrawScenesOfInterest();
            root.Add(existingScenesOfInterest);

            return root;
        }

        private void AddSceneOfInterest(ISceneOfInterestMarker marker, int index)
        {
            var e = new VisualElement();
            e.AddToClassList(MapRendererTestSceneStyles.FUNCTION_GROUP);

            var title = new Label($"#{index}");
            title.AddToClassList(MapRendererTestSceneStyles.GROUP_TITLE);

            e.Add(title);

            var posField = new Vector3Field("Position") {value = marker.CurrentPosition};
            posField.binding = new GetterBinding<Vector3>(posField, () => marker.CurrentPosition);

            var isVisibleField = new Toggle("Visible") { value = marker.IsVisible };
            isVisibleField.binding = new GetterBinding<bool>(isVisibleField, () => marker.IsVisible);

            e.Add(posField);
            e.Add(isVisibleField);

            existingScenesOfInterest.Add(e);
        }

        private void RedrawScenesOfInterest()
        {
            if (existingScenesOfInterest == null)
            {
                existingScenesOfInterest = new VisualElement();
                existingScenesOfInterest.AddToClassList(MapRendererTestSceneStyles.FUNCTION_GROUP);

                var label = new Label("List");
                label.AddToClassList(MapRendererTestSceneStyles.GROUP_TITLE);

                existingScenesOfInterest.Add(label);
            }
            else
            {
                existingScenesOfInterest.Clear();
            }

            var i = 0;

            var layer = mapRenderer.layers_Test.OfType<ScenesOfInterestMarkersController>().First();

            foreach (ISceneOfInterestMarker marker in layer.Markers.Values)
            {
                AddSceneOfInterest(marker, i);
                i++;
            }
        }
    }
}
