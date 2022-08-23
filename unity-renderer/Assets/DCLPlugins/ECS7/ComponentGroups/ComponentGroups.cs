using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

public readonly struct ComponentGroups : IComponentGroups
{
    public readonly IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> texturizableGroup { get; }

    public ComponentGroups(ECSComponentsManager componentsManager)
    {
        texturizableGroup = componentsManager.CreateComponentGroup<InternalMaterial, InternalTexturizable>
            ((int)InternalECSComponentsId.MATERIAL, (int)InternalECSComponentsId.TEXTURIZABLE);
    }
}