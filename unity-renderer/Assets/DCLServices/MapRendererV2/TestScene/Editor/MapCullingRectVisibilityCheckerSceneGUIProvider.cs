using DCLServices.MapRendererV2.ComponentsFactory;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DCLServices.MapRendererV2.TestScene
{
    public class MapCullingRectVisibilityCheckerSceneGUIProvider : IMapRendererTestSceneGUIProvider
    {
        private static readonly Color OBJECT_COLOR = new (146 / 255f, 153 / 255f, 78 / 255f);
        private static readonly Color CAMERA_COLOR = new (133 / 255f, 44 / 255f, 91 / 255f);

        private readonly MapRenderer mapRenderer;
        private readonly float size;

        private MapRendererConfiguration configuration;

        public MapCullingRectVisibilityCheckerSceneGUIProvider(
            MapRenderer mapRenderer,
            float size)
        {
            this.mapRenderer = mapRenderer;
            this.size = size;
        }

        public void OnSceneGUI()
        {
            Handles.color = OBJECT_COLOR;

            if (!configuration)
                configuration = SceneManager.GetActiveScene().GetRootGameObjects()
                                            .Select(ro => ro.GetComponent<MapRendererConfiguration>())
                                            .FirstOrDefault(x => x);

            if (!configuration)
                return;

            var configurationPosition = configuration.transform.position;

            foreach (var positionProvider in mapRenderer.cullingController.TrackedObjects.Keys)
            {
                var position = positionProvider.CurrentPosition + configurationPosition;

                Handles.DrawLines(new[]
                {
                    position - (new Vector3(-1, -1, 0) * (size / 2f)),
                    position - (new Vector3(-1, 1, 0) * (size / 2f)),
                    position - (new Vector3(-1, 1, 0) * (size / 2f)),
                    position - (new Vector3(1, 1, 0) * (size / 2f)),
                    position - (new Vector3(1, 1, 0) * (size / 2f)),
                    position - (new Vector3(1, -1, 0) * (size / 2f)),
                    position - (new Vector3(1, -1, 0) * (size / 2f)),
                    position - (new Vector3(-1, -1, 0) * (size / 2f)),
                });
            }

            Handles.color = CAMERA_COLOR;

            var camerasRoot = configuration.MapCamerasRoot;
            var z = camerasRoot.transform.position.z;

            foreach (var cameraState in mapRenderer.cullingController.CameraStates)
            {
                var rect = cameraState.Rect;
                var worldRect = new Rect(camerasRoot.TransformPoint(rect.position), camerasRoot.TransformVector(rect.size));

                Handles.DrawLines(new[]
                {
                    new Vector3(worldRect.xMin, worldRect.yMin, z),
                    new Vector3(worldRect.xMin, worldRect.yMax, z),
                    new Vector3(worldRect.xMin, worldRect.yMax, z),
                    new Vector3(worldRect.xMax, worldRect.yMax, z),
                    new Vector3(worldRect.xMax, worldRect.yMax, z),
                    new Vector3(worldRect.xMax, worldRect.yMin, z),
                    new Vector3(worldRect.xMax, worldRect.yMin, z),
                    new Vector3(worldRect.xMin, worldRect.yMin, z),
                });
            }
        }
    }
}
