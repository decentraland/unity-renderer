using DCL;
using DCL.Components;
using DCL.Models;

public class UIComponentsPlugin : IPlugin
{
    private readonly UIShapePool inputTextPool = new ("UIInputText");
    private readonly UIShapePool fullScreenPool = new UIShapePool("");
    private readonly UIShapePool screenSpacePool = new UIShapePool("");
    private readonly UIShapePool containerRectPool = new ("UIContainerRect");
    private readonly UIShapePool scrollRectPool = new ("UIScrollRect");
    private readonly UIShapePool containerStackPool = new ( "UIContainerRect");
    private readonly UIShapePool imagePool = new ("UIImage", 3);
    private readonly UIShapePool textPool = new ("UIText", 3);

    public UIComponentsPlugin()
    {
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
