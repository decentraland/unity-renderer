using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class InputTextTestsController : MonoBehaviour
    {
        protected ISceneController sceneController;
        protected ParcelScene scene;

        protected IEnumerator InitScene(bool spawnCharController = true)
        {
            sceneController = Environment.i.world.sceneController;

            yield return new WaitForSeconds(0.01f);

            scene = sceneController.CreateTestScene();

            yield return new WaitForSeconds(0.01f);

            if (spawnCharController)
            {
                if (DCLCharacterController.i == null)
                {
                    GameObject.Instantiate(Resources.Load("Prefabs/CharacterController"));
                }
            }
        }

        public IEnumerator Start()
        {
            yield return InitScene(spawnCharController: false);

            DCLCharacterController.i.gravity = 0;

            UIScreenSpace ssshape = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(
                scene,
                DCL.Models.CLASS_ID.UI_SCREEN_SPACE_SHAPE);

            yield return ssshape.routine;

            UIInputText text = TestHelpers.SharedComponentCreate<UIInputText, UIInputText.Model>(
                scene,
                Models.CLASS_ID.UI_INPUT_TEXT_SHAPE,
                new UIInputText.Model()
                {
                    textModel = new DCL.Components.TextShape.Model()
                    {
                        color = Color.white,
                        opacity = 1,
                    },

                    placeholder = "Chat here!",
                    placeholderColor = Color.grey,
                    focusedBackground = Color.black,
                    parentComponent = ssshape.id,
                    positionX = new UIValue(200, UIValue.Unit.PIXELS),
                    positionY = new UIValue(200, UIValue.Unit.PIXELS),
                    height = new UIValue(200, UIValue.Unit.PIXELS),
                    width = new UIValue(200, UIValue.Unit.PIXELS),
                    hAlign = "left",
                    vAlign = "top",
                });

            yield return text.routine;
        }
    }
}