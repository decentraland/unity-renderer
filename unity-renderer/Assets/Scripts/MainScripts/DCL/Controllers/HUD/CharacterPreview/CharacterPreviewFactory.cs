using DCL;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MainScripts.DCL.Controllers.HUD.CharacterPreview
{
    public class CharacterPreviewFactory : ICharacterPreviewFactory
    {
        private static readonly Vector3 COORDS_TO_START = new (0, 50, 0);
        private static readonly Vector3 VECTOR_BETWEEN_INSTANCES = new (3, 0, 3);

        private int controllersCount = 0;

        private global::MainScripts.DCL.Controllers.HUD.CharacterPreview.CharacterPreviewController prefab;

        public ICharacterPreviewController Create(CharacterPreviewMode loadingMode, RenderTexture renderTexture, bool isVisible,
            global::MainScripts.DCL.Controllers.HUD.CharacterPreview.CharacterPreviewController.CameraFocus cameraFocus = global::MainScripts.DCL.Controllers.HUD.CharacterPreview.CharacterPreviewController.CameraFocus.DefaultEditing)
        {
            var instance = Object.Instantiate(prefab);
            instance.transform.position = COORDS_TO_START + (VECTOR_BETWEEN_INSTANCES * controllersCount);

            var characterPreviewController = instance.gameObject.GetComponent<global::MainScripts.DCL.Controllers.HUD.CharacterPreview.CharacterPreviewController>();

            characterPreviewController.Initialize(loadingMode, renderTexture);
            characterPreviewController.SetEnabled(isVisible);
            characterPreviewController.SetFocus(cameraFocus, false);

            controllersCount++;

            return characterPreviewController;
        }

        void IDisposable.Dispose() { }

        void IService.Initialize()
        {
            prefab = Resources.Load<global::MainScripts.DCL.Controllers.HUD.CharacterPreview.CharacterPreviewController>("CharacterPreview");
        }
    }
}
