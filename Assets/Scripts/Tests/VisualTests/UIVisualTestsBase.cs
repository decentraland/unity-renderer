using DCL;
using DCL.Models;
using DCL.Components;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UIVisualTestsBase : VisualTestsBase
{
    protected string screenSpaceId;

    protected IEnumerator InitUIVisualTestScene(string testName)
    {
        yield return InitScene();

        yield return VisualTestHelpers.InitVisualTestsScene(testName);

        DCLCharacterController.i.gravity = 0f;
        DCLCharacterController.i.enabled = false;

        // Position character inside parcel (0,0)
        DCLCharacterController.i.SetPosition(JsonConvert.SerializeObject(
        new
        {
            x = 0f,
            y = 2f,
            z = 0f
        }));

        // Create UIScreenSpace
        UIScreenSpace screenSpace = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);
        yield return screenSpace.routine;

        screenSpaceId = screenSpace.id;

        // The canvas has to be in ScreenSpaceCamera mode to be able to render the UI correctly for the snapshots
        screenSpace.canvas.renderMode = RenderMode.ScreenSpaceCamera;
        screenSpace.canvas.worldCamera = Camera.main;
        screenSpace.canvas.planeDistance = 1;

        // UI is rendered on the character's "MainCamera" by default
        VisualTestController.i.camera.gameObject.SetActive(false);
        VisualTestController.i.camera = Camera.main;

        // The camera should only render UI to decrease conflict chance with future ground changes, etc.
        VisualTestController.i.camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
    }

    protected IEnumerator CreateUIComponent<SharedComponentType, SharedComponentModel>(CLASS_ID classId, SharedComponentModel model, string componentId)
        where SharedComponentType : BaseDisposable
        where SharedComponentModel : class, new()
    {
        if (model == null)
            model = new SharedComponentModel();

        // Creation
        var component = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
        {
            classId = (int)classId,
            id = componentId,
            name = "material"
        })) as SharedComponentType;
        yield return component.routine;

        // "fake" update (triggers 1st ApplyChanges() call)
        scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
        {
            id = componentId,
            json = JsonUtility.ToJson(new SharedComponentModel())
        }));
        yield return component.routine;

        // "real" update
        scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
        {
            id = componentId,
            json = JsonUtility.ToJson(model)
        }));
        yield return component.routine;
    }
}
