using System.Collections;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class ECSVisibilityComponentShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSVisibilityComponentHandler handler;
        private GameObject gameObject;
        private IInternalECSComponent<InternalVisibility> internalVisibilityComponent;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            internalVisibilityComponent = Substitute.For<IInternalECSComponent<InternalVisibility>>();

            handler = new ECSVisibilityComponentHandler(internalVisibilityComponent);

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
            handler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            handler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void PutInternalVisibilityOnUpdate()
        {
            PBVisibilityComponent model = new PBVisibilityComponent() { Visible = true };

            handler.OnComponentModelUpdated(scene, entity, model);
            
            internalVisibilityComponent.Received(1)
                                       .PutFor(scene, entity,
                                           Arg.Is<InternalVisibility>(x => x.visible == true && x.dirty));

            internalVisibilityComponent.ClearReceivedCalls();

            PBVisibilityComponent model2 = new PBVisibilityComponent() { Visible = false };
            handler.OnComponentModelUpdated(scene, entity, model2);

            internalVisibilityComponent.Received(1)
                                       .PutFor(scene, entity,
                                           Arg.Is<InternalVisibility>(x => x.visible == false && x.dirty));
        }
    }
}
