using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class RemoveOnConditionHandlerShould
    {
        private RemoveOnConditionHandler<InternalTexturizable> handler;
        private IInternalECSComponent<InternalTexturizable> component;
        private IDCLEntity entity;
        private IParcelScene scene;

        [SetUp]
        public void SetUp()
        {
            component = Substitute.For<IInternalECSComponent<InternalTexturizable>>();
            handler = new RemoveOnConditionHandler<InternalTexturizable>(
                () => component, m => m.renderers.Count == 0);
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
        }

        [Test]
        public void RemoveComponentWhenNoRenderers()
        {
            InternalTexturizable model = new InternalTexturizable();
            model.renderers.Clear();
            handler.OnComponentModelUpdated(scene, entity, model);
            component.Received(1).RemoveFor(scene, entity);
        }

        [Test]
        public void NotRemoveComponentWhenNoRenderers()
        {
            GameObject go = new GameObject();
            Renderer renderer = go.AddComponent<MeshRenderer>();

            InternalTexturizable model = new InternalTexturizable();
            model.renderers.Add(renderer);
            handler.OnComponentModelUpdated(scene, entity, model);
            component.DidNotReceive().RemoveFor(Arg.Any<IParcelScene>(), Arg.Any<IDCLEntity>());

            Object.DestroyImmediate(go);
        }
    }
}