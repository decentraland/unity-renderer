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

        private CharacterPreviewController prefab;

        public ICharacterPreviewController Create(CharacterPreviewMode loadingMode, RenderTexture renderTexture, bool isVisible,
            CharacterPreviewController.CameraFocus cameraFocus = CharacterPreviewController.CameraFocus.DefaultEditing, bool isOrthographic = false)
        {
            var instance = Object.Instantiate(prefab);
            instance.transform.position = COORDS_TO_START + (VECTOR_BETWEEN_INSTANCES * controllersCount);

            var characterPreviewController = instance.gameObject.GetComponent<CharacterPreviewController>();

            characterPreviewController.Initialize(loadingMode, renderTexture);
            characterPreviewController.SetEnabled(isVisible);
            characterPreviewController.SetCameraProjection(isOrthographic);
            characterPreviewController.SetFocus(cameraFocus, null, false);

            controllersCount++;

            return characterPreviewController;
        }

        void IDisposable.Dispose() { }

        void IService.Initialize()
        {
            prefab = Resources.Load<CharacterPreviewController>("CharacterPreview");
        }
    }
}
