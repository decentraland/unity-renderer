using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

public readonly struct ComponentGroups : IComponentGroups
{
    public readonly IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> texturizableGroup { get; }
    public readonly IECSReadOnlyComponentsGroup<InternalRenderers, InternalVisibility> visibilityGroup { get; }

    public ComponentGroups(ECSComponentsManager componentsManager)
    {
        texturizableGroup = componentsManager.CreateComponentGroup<InternalMaterial, InternalTexturizable>
            ((int)InternalECSComponentsId.MATERIAL, (int)InternalECSComponentsId.TEXTURIZABLE);
        
        visibilityGroup = componentsManager.CreateComponentGroup<InternalRenderers, InternalVisibility>
            ((int)InternalECSComponentsId.RENDERERS, (int)InternalECSComponentsId.VISIBILITY);
    }
}