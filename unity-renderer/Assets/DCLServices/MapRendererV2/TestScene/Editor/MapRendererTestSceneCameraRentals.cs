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
            var zoom = new FloatField("Zoom") { value = 1 };
            var texRes = new Vector2IntField("Texture Resolution") { value = new Vector2Int(512, 512) };
            var zoomThreshold = new Vector2IntField("Zoom thresholds") { value = new Vector2Int(100, 500) };

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

                AddMapCameraControllerControls(rent, mapCameraControllers.Count - 1);
            }

            var submitButton = new Button(CreateNewRent) { text = "Rent" };

            root.Add(enabledLayers);
            root.Add(position);
            root.Add(zoom);
            root.Add(texRes);
            root.Add(zoomThreshold);
            root.Add(submitButton);

            return root;
        }

        private void AddMapCameraControllerControls(IMapCameraController cameraController, int index)
        {
            // TODO add more controls

            var controls = new VisualElement();
            controls.AddToClassList(MapRendererTestSceneStyles.FUNCTION_GROUP);

            var name = new Label($"#{index} {cameraController.EnabledLayers}");
            name.AddToClassList(MapRendererTestSceneStyles.GROUP_TITLE);
            controls.Add(name);

            var texPreview = new Image { image = cameraController.GetRenderTexture() };
            texPreview.AddToClassList(MapRendererTestSceneStyles.TEXTURE_PREVIEW);
            controls.Add(texPreview);

            var zoom = new FloatField("Zoom") { value = cameraController.Zoom};
            zoom.RegisterValueChangedCallback(eve => cameraController.SetZoom(eve.newValue));
            controls.Add(zoom);

            var position = new Vector2Field("Position") { value = cameraController.Position };
            position.RegisterValueChangedCallback(evt => cameraController.SetPosition(evt.newValue));
            controls.Add(position);

            controls.Add(new Button(() =>
            {
                cameraController.Release();
                rents.Remove(controls);
                mapCameraControllers.Remove(cameraController);
            }) { text = "Release" });

            rents.Add(controls);
        }

        private VisualElement CreateExistingRentsControls()
        {
            rents = new VisualElement();
            rents.Add(new Label("Rents:") { style = { fontSize = 10, unityFontStyleAndWeight = FontStyle.Bold } });

            for (var index = 0; index < mapCameraControllers.Count; index++)
            {
                IMapCameraController controller = mapCameraControllers[index];
                AddMapCameraControllerControls(controller, index);
            }

            return rents;
        }
    }
}
