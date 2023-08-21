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
    private readonly UIShapePool containerStackChildPool;
    private readonly UIShapePool imagePool;
    private readonly UIShapePool textPool;

    private readonly Transform uiPoolsRoot;

    public UIComponentsPlugin()
    {
        uiPoolsRoot = new GameObject("_SDK6_UIShapes_Pools").transform;

        inputTextPool = new (uiPoolsRoot, prefabPath: "UIInputText", capacity: 2);
        containerRectPool = new (uiPoolsRoot, prefabPath: "UIContainerRect", true, 100);
        scrollRectPool = new (uiPoolsRoot, prefabPath: "UIScrollRect", capacity: 5);
        containerStackPool = new (uiPoolsRoot, prefabPath: "UIContainerRect", true, 100);
        containerStackChildPool = new (uiPoolsRoot, prefabPath: "UIContainerStackChild", true, 5);
        imagePool = new (uiPoolsRoot, prefabPath: "UIImage", true, 700);
        textPool = new (uiPoolsRoot, prefabPath: "UIText", true, 600);

        // TODO: introduced partial pooling there instead of dynamically assembling the object each time
        // this needs to be fully converted to pooling when (if) we shift this part to Addressables
        fullScreenPool = new UIShapePool(uiPoolsRoot, prefabPath: "UIScreenSpace", true, capacity: 3);
        screenSpacePool = new UIShapePool(uiPoolsRoot, prefabPath: "UIScreenSpace", true, capacity: 3);

        IRuntimeComponentFactory factory = Environment.i.world.componentFactory;

        // UI
        factory.RegisterBuilder((int) CLASS_ID.UI_INPUT_TEXT_SHAPE, () => new UIInputText(inputTextPool));
        factory.RegisterBuilder((int) CLASS_ID.UI_FULLSCREEN_SHAPE, () => new UIScreenSpace(fullScreenPool));
        factory.RegisterBuilder((int) CLASS_ID.UI_SCREEN_SPACE_SHAPE, () => new UIScreenSpace(screenSpacePool));
        factory.RegisterBuilder((int) CLASS_ID.UI_CONTAINER_RECT, () => new UIContainerRect(containerRectPool));
        factory.RegisterBuilder((int) CLASS_ID.UI_SLIDER_SHAPE, () => new UIScrollRect(scrollRectPool));
        factory.RegisterBuilder((int) CLASS_ID.UI_CONTAINER_STACK, () => new UIContainerStack(containerStackPool, containerStackChildPool));
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
