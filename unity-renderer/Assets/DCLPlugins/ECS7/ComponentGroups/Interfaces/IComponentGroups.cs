using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;

public interface IComponentGroups
{
    IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> texturizableGroup { get; }
    IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup { get; }
    IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup { get; }
}