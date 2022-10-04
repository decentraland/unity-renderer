using System;
using DCL.ECS7.InternalComponents;

public interface IInternalECSComponents
{
    Action writeInternalComponentsSystem { get; }
    IInternalECSComponent<InternalTexturizable> texturizableComponent { get; }
    IInternalECSComponent<InternalMaterial> materialComponent { get; }
    IInternalECSComponent<InternalColliders> onPointerColliderComponent { get; }
    IInternalECSComponent<InternalColliders> physicColliderComponent { get; }
    IInternalECSComponent<InternalRenderers> renderersComponent { get; }
    IInternalECSComponent<InternalVisibility> visibilityComponent { get; }
    IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent { get; }
}