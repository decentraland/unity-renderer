using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapRendererTestSceneCameraRentals : IMapRendererTestSceneElementProvider
    {
        private readonly MapRenderer mapRenderer;

        private VisualElement rents;
        private readonly List<IMapCameraController> mapCameraControllers = new ();

        public MapRendererTestSceneCameraRentals(MapRenderer mapRenderer)
        {
            this.mapRenderer = mapRenderer;
        }

        public VisualElement GetElement()
        {
            var group = new VisualElement();

            group.Add(CreateAddRent());
            group.Add(CreateExistingRentsControls());

            return group;
        }

        private VisualElement CreateAddRent()
        {
            var root = new VisualElement();

            var enabledLayers = new EnumFlagsField("Enabled Layers", MapLayer.Atlas);
            var position = new Vector2IntField("Position");
            var zoom = new FloatField("Zoom");
            var texRes = new Vector2IntField("Texture Resolution");
            var zoomThreshold = new Vector2IntField("Zoom thresholds");

            void CreateNewRent()
            {
                var rent = mapRenderer.RentCamera(
                    new MapCameraInput(
                        (MapLayer)enabledLayers.value,
                        position.value,
                        zoom.value,
                        texRes.value,
                        zoomThreshold.value));

                mapCameraControllers.Add(rent);

                AddMapCameraControllerControls(rent);
            }

            var submitButton = new Button(CreateNewRent) {text = "Rent"};

            root.Add(enabledLayers);
            root.Add(position);
            root.Add(zoom);
            root.Add(texRes);
            root.Add(zoomThreshold);
            root.Add(submitButton);

            return root;
        }

        private void AddMapCameraControllerControls(IMapCameraController cameraController)
        {
            // TODO add more controls

            var controls = new VisualElement();
            controls.AddToClassList(MapRendererTestSceneStyles.FUNCTION_GROUP);

            controls.Add(new Button(() =>
            {
                cameraController.Release();
                rents.Remove(controls);
                mapCameraControllers.Remove(cameraController);
            }) {text = "Release"});

            rents.Add(controls);
        }

        private VisualElement CreateExistingRentsControls()
        {
            rents = new VisualElement();
            rents.Add(new Label("Rents:") { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            foreach (IMapCameraController controller in mapCameraControllers)
                AddMapCameraControllerControls(controller);

            return rents;
        }
    }
}
