using DCL;
using DCL.Components;
using DCL.Models;
using UnityEngine;

public class UIComponentsPlugin : IPlugin
{
    private readonly UIShapePool inputTextPool;
    private readonly UIShapePool fullScreenPool;
    private readonly UIShapePool screenSpacePool;
    private readonly UIShapePool containerRectPool;
    private readonly UIShapePool scrollRectPool;
    private readonly UIShapePool containerStackPool;
    private readonly UIShapePool imagePool;
    private readonly UIShapePool textPool;

    private readonly Transform uiPoolsRoot;

    public UIComponentsPlugin()
    {
        uiPoolsRoot = new GameObject("_SDK6_UIShapes_Pools").transform;

        inputTextPool = new (uiPoolsRoot, prefabPath: "UIInputText", true, 5);
        containerRectPool = new (uiPoolsRoot, prefabPath: "UIContainerRect", true, 30);
        scrollRectPool = new (uiPoolsRoot, prefabPath: "UIScrollRect");
        containerStackPool = new (uiPoolsRoot, prefabPath: "UIContainerRect", true);
        imagePool = new (uiPoolsRoot, prefabPath: "UIImage", true, 200);
        textPool = new (uiPoolsRoot, prefabPath: "UIText", true, 100);

        // TODO: introduce pooling there instead of dynamicaly assebling the object each time
        fullScreenPool = new UIShapePool(uiPoolsRoot, prefabPath: "", capacity: 1);
        screenSpacePool = new UIShapePool(uiPoolsRoot, prefabPath: "", capacity: 1);

        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;

        // UI
        factory.RegisterBuilder((int)CLASS_ID.UI_INPUT_TEXT_SHAPE, () => new UIInputText(inputTextPool));
        factory.RegisterBuilder((int) CLASS_ID.UI_FULLSCREEN_SHAPE, () => new UIScreenSpace(fullScreenPool) );
        factory.RegisterBuilder((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE, () => new UIScreenSpace(screenSpacePool) );
        factory.RegisterBuilder((int) CLASS_ID.UI_CONTAINER_RECT, () => new UIContainerRect(containerRectPool) );
        factory.RegisterBuilder((int) CLASS_ID.UI_SLIDER_SHAPE, () => new UIScrollRect(scrollRectPool) );
        factory.RegisterBuilder((int) CLASS_ID.UI_CONTAINER_STACK, () => new UIContainerStack(containerStackPool) );
        factory.RegisterBuilder((int) CLASS_ID.UI_IMAGE_SHAPE, () => new UIImage(imagePool));
        factory.RegisterBuilder((int) CLASS_ID.UI_TEXT_SHAPE, () => new UIText(textPool));

        factory.createConditions.Add((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE, CanCreateScreenShape);
        factory.createConditions.Add((int) CLASS_ID.UI_FULLSCREEN_SHAPE, CanCreateScreenShape);
    }

    private static bool CanCreateScreenShape(int sceneNumber, int _) =>
        Environment.i.world.state.GetScene(sceneNumber)
                   .componentsManagerLegacy.GetSceneSharedComponent<UIScreenSpace>() == null;

    public void Dispose()
    {
        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;

        factory.UnregisterBuilder((int) CLASS_ID.UI_INPUT_TEXT_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_FULLSCREEN_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_CONTAINER_RECT);
        factory.UnregisterBuilder((int) CLASS_ID.UI_SLIDER_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_CONTAINER_STACK);
        factory.UnregisterBuilder((int) CLASS_ID.UI_IMAGE_SHAPE);
        factory.UnregisterBuilder((int) CLASS_ID.UI_TEXT_SHAPE);

        factory.createConditions.Remove((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE);
        factory.createConditions.Remove((int) CLASS_ID.UI_FULLSCREEN_SHAPE);
    }
}
