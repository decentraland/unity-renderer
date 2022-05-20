using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;

public class DCLEntityShould
{
    [Test]
    public void CleanUpEntityComponents()
    {
        DecentralandEntity entity = new DecentralandEntity();
        entity.entityId = 345345;
        
        IParcelScene scene = Substitute.For<IParcelScene>();
        scene.componentsManagerLegacy.Returns(new ECSComponentsManagerLegacy(scene));
        entity.scene = scene;
        
        IEntityComponent component = Substitute.For<IEntityComponent>();
        entity.scene.componentsManagerLegacy.AddComponent(entity, CLASS_ID_COMPONENT.NONE, component);

        entity.Cleanup();

        component.Received().Cleanup();
    }
}