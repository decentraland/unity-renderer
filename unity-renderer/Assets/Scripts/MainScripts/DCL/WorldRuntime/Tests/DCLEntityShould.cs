using DCL.Components;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;

public class DCLEntityShould
{
    [Test]
    public void CleanUpEntityComponents()
    {
        DecentralandEntity entity = new DecentralandEntity();
        IEntityComponent component = Substitute.For<IEntityComponent>();
        entity.components.Add(CLASS_ID_COMPONENT.NONE, component);

        entity.Cleanup();

        component.Received().Cleanup();
    }
}