using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

public interface IComponentGroups
{
    IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> texturizableGroup { get; }
    IECSReadOnlyComponentsGroup<InternalRenderers, InternalVisibility> visibilityGroup { get; }
}
