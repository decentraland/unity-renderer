using DCL.ECS7.InternalComponents;

public interface IInternalECSComponents
{
    IInternalECSComponent<InternalTexturizable> texturizableComponent { get; }
    IInternalECSComponent<InternalMaterial> materialComponent { get; }
    IInternalECSComponent<InternalColliders> onPointerColliderComponent { get; }
    IInternalECSComponent<InternalColliders> physicColliderComponent { get; }
}