using System;
using DCL.Components;
using NSubstitute;

namespace Tests
{
    public static class MockedSharedComponentHelper
    {
        public static (ISharedComponent component, Action SetAsReady, string id) Create(string id)
        {
            var componentMock = Substitute.For<ISharedComponent>();
            componentMock.id.Returns(id);
            Action<ISharedComponent> readyCallback = null;
            componentMock.WhenForAnyArgs(x => x.CallWhenReady(Arg.Any<Action<ISharedComponent>>()))
                         .Do(info =>
                         {
                             readyCallback += info.ArgAt<Action<ISharedComponent>>(0);
                         });
            return (component: componentMock,
                SetAsReady: () => readyCallback?.Invoke(componentMock),
                id);
        }
    }
}