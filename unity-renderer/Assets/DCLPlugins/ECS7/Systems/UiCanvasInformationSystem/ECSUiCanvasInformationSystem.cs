using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using Decentraland.Common;
using System;
using System.Collections.Generic;
using Screen = UnityEngine.Device.Screen;

namespace ECSSystems.UiCanvasInformationSystem
{
    public class ECSUiCanvasInformationSystem : IDisposable
    {
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBUiCanvasInformation>> componentPool;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly BorderRect interactableArea;

        private float lastDevicePixelRatio = -1;
        private int lastViewportResolutionWidth = -1;
        private int lastScreenRealResolutionWidth = -1;

        public ECSUiCanvasInformationSystem(
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBUiCanvasInformation>> componentPool,
            BaseList<IParcelScene> loadedScenes)
        {
            this.loadedScenes = loadedScenes;
            this.componentsWriter = componentsWriter;
            this.componentPool = componentPool;

            loadedScenes.OnAdded += OnSceneAdded;

            interactableArea = new BorderRect()
            {
                Bottom = 0,
                Left = 0,
                Right = 0,
                Top = 0
            };
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= OnSceneAdded;
        }

        public void Update()
        {
            if (lastViewportResolutionWidth == Screen.width && lastScreenRealResolutionWidth == Screen.currentResolution.width)
                return;

            // 'Screen.mainWindowDisplayInfo' cannot be used in WebGL due to 'Screen.GetDisplayInfoForPoint' unsupported error
            lastScreenRealResolutionWidth = Screen.currentResolution.width;
            lastViewportResolutionWidth = Screen.width;
            lastDevicePixelRatio = CalculateDevicePixelRatio(lastViewportResolutionWidth, lastScreenRealResolutionWidth);

            int scenesCount = loadedScenes.Count;
            for (int i = 0; i < scenesCount; i++)
            {
                UpdateUiCanvasInformationComponent(loadedScenes[i].sceneData.sceneNumber);
            }
        }

        /*
         * Calculates what web browsers call "device pixel ratio": a ratio between the application virtual resolution
         * and the screen resolution. It represents how many physical/real pixels are needed to draw the virtual pixels
         * of the application. The smaller the application resolution is, compared to the real screen resolution, the
         * higher the device pixel ratio is. Normally the result can be between 1~3.
         *
         * Note: Explorer's 'rendering scale' doesn't affect this, as that is only for 3D rendering, so it doesn't affect
         * the whole application resolution (e.g. UIs are unaffected).
        */
        private static float CalculateDevicePixelRatio(int viewportPixelsWidth, int realScreenPixelsWidth) => realScreenPixelsWidth / (float)viewportPixelsWidth;

        private void UpdateUiCanvasInformationComponent(int sceneNumber)
        {
            if (!componentsWriter.TryGetValue(sceneNumber, out var writer))
                return;

            var componentPooled = componentPool.Get();
            var componentModel = componentPooled.WrappedComponent.Model;
            componentModel.InteractableArea = interactableArea;
            componentModel.Width = Screen.width;
            componentModel.Height = Screen.height;
            componentModel.DevicePixelRatio = lastDevicePixelRatio;

            writer.Put(SpecialEntityId.SCENE_ROOT_ENTITY, ComponentID.UI_CANVAS_INFORMATION, componentPooled);
        }

        private void OnSceneAdded(IParcelScene scene)
        {
            UpdateUiCanvasInformationComponent(scene.sceneData.sceneNumber);
        }
    }
}
