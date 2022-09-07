using DCL.ECS7.InternalComponents;

public interface IInternalECSComponents
{
    IInternalECSComponent<InternalTexturizable> texturizableComponent { get; }
    IInternalECSComponent<InternalMaterial> materialComponent { get; }
    IInternalECSComponent<InternalRenderers> renderersComponent { get; }
    IInternalECSComponent<InternalVisibility> visibilityComponent { get; }
}