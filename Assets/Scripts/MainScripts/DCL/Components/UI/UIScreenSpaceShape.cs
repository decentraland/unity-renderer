using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;
using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class UIScreenSpaceShape : UIShape
    {
        public override string componentName => "UIScreenSpaceShape";

        Model model = new Model();
        Transform characterTransform;

        public UIScreenSpaceShape(ParcelScene scene) : base(scene)
        {
            DCLCharacterController.OnCharacterMoved += OnCharacterMoved;
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            Debug.LogError("Aborted UIScreenShape attachment to an entity. UIShapes shouldn't be attached to entities.");
        }

        public override void DetachFrom(DecentralandEntity entity)
        {
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);

            scene.EnsureScreenSpaceCanvasGameObject(componentName);
            transform = scene.uiScreenSpaceCanvas.GetComponent<RectTransform>();

            if (characterTransform == null)
                characterTransform = GameObject.FindObjectOfType<CharacterController>().transform;

            OnCharacterMoved(characterTransform.position);

            return null;
        }

        public override void Dispose()
        {
            DCLCharacterController.OnCharacterMoved -= OnCharacterMoved;

            base.Dispose();
        }

        void OnCharacterMoved(Vector3 newCharacterPosition)
        {
            scene.uiScreenSpaceCanvas.enabled = scene.IsInsideSceneBoundaries(newCharacterPosition) && model.visible;
        }
    }
}