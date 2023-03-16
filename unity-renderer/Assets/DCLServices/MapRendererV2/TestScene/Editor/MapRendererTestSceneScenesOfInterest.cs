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

        private string name = "fancy scene name";
        private readonly List<Vector2Int> parcels = new ();

        public MapRendererTestSceneScenesOfInterest(MinimapMetadata minimapMetadata)
        {
            this.minimapMetadata = minimapMetadata;
        }

        public VisualElement GetElement()
        {
            var root = new VisualElement();

            var nameField = new TextField(name);
            nameField.RegisterValueChangedCallback(s => name = s.newValue);

            root.Add(nameField);

            var parcels = new VisualElement();
            parcels.AddToClassList(MapRendererTestSceneStyles.FUNCTION_GROUP);

            var title = new Label("Parcels");
            title.AddToClassList(MapRendererTestSceneStyles.GROUP_TITLE);

            parcels.Add(title);

            var parcelsView = new ListView(this.parcels, 30, makeItem: () => new Vector2IntField("parcel"), bindItem:
                (element, i) =>
                {
                    var v2Field = (Vector2IntField)element;
                    v2Field.value = this.parcels[i];
                    v2Field.RegisterValueChangedCallback(evt => this.parcels[i] = evt.newValue);
                });

            parcelsView.showAddRemoveFooter = true;

            parcels.Add(parcelsView);
            root.Add(parcels);

            var addButton = new Button(() =>
            {
                minimapMetadata.AddSceneInfo(new MinimapMetadata.MinimapSceneInfo
                {
                    name = this.name,
                    isPOI = true,
                    parcels = this.parcels.ToList(),
                    description = "",
                    owner = "",
                    previewImageUrl = ""
                });
            }) { text = "Add" };
            root.Add(addButton);

            return root;
        }
    }
}
