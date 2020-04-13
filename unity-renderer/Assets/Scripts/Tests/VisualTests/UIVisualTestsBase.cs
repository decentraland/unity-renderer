using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

public class UIVisualTestsBase : VisualTestsBase
{
    protected string screenSpaceId;

    protected override IEnumerator SetUp()
    {
        yield break;
    }

    protected IEnumerator InitUIVisualTestScene(string testName)
    {
        yield return InitScene();

        yield return VisualTestHelpers.InitVisualTestsScene(testName);

        // Create UIScreenSpace
        UIScreenSpace screenSpace = TestHelpers.SharedComponentCreate<UIScreenSpace, UIScreenSpace.Model>(scene, CLASS_ID.UI_SCREEN_SPACE_SHAPE);
        yield return screenSpace.routine;

        screenSpaceId = screenSpace.id;

        // The canvas has to be in ScreenSpaceCamera mode to be able to render the UI correctly for the snapshots
        screenSpace.canvas.renderMode = RenderMode.ScreenSpaceCamera;
        screenSpace.canvas.worldCamera = VisualTestController.i.camera;
        screenSpace.canvas.planeDistance = 1;

        // The camera should only render UI to decrease conflict chance with future ground changes, etc.
        VisualTestController.i.camera.cullingMask = 1 << LayerMask.NameToLayer("UI");

        int id = GameViewUtils.AddOrGetCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, UnityEditor.GameViewSizeGroupType.Standalone, 1280, 720, "Test Resolution");
        GameViewUtils.SetSize(id);
    }

    protected IEnumerator CreateUIComponent<SharedComponentType, SharedComponentModel>(CLASS_ID classId, SharedComponentModel model, string componentId)
        where SharedComponentType : BaseDisposable
        where SharedComponentModel : class, new()
    {
        if (model == null)
            model = new SharedComponentModel();

        // Creation
        var component = scene.SharedComponentCreate(
            componentId,
            "material",
            (int)classId
        ) as SharedComponentType;
        yield return component.routine;

        // "fake" update (triggers 1st ApplyChanges() call)
        scene.SharedComponentUpdate(componentId, JsonUtility.ToJson(new SharedComponentModel()));
        yield return component.routine;

        // "real" update
        scene.SharedComponentUpdate(componentId, JsonUtility.ToJson(model));
        yield return component.routine;
    }
}
