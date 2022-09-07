using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;

public readonly struct ComponentGroups : IComponentGroups
{
    public IECSReadOnlyComponentsGroup<InternalMaterial, InternalTexturizable> texturizableGroup { get; }
    public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerDown> pointerDownGroup { get; }
    public IECSReadOnlyComponentsGroup<InternalColliders, PBOnPointerUp> pointerUpGroup { get; }

    public ComponentGroups(ECSComponentsManager componentsManager)
    {
        texturizableGroup = componentsManager.CreateComponentGroup<InternalMaterial, InternalTexturizable>
            ((int)InternalECSComponentsId.MATERIAL, (int)InternalECSComponentsId.TEXTURIZABLE);

        pointerDownGroup = componentsManager.CreateComponentGroup<InternalColliders, PBOnPointerDown>
            ((int)InternalECSComponentsId.COLLIDER_POINTER, ComponentID.ON_POINTER_DOWN);

        pointerUpGroup = componentsManager.CreateComponentGroup<InternalColliders, PBOnPointerUp>
            ((int)InternalECSComponentsId.COLLIDER_POINTER, ComponentID.ON_POINTER_UP);
    }
}