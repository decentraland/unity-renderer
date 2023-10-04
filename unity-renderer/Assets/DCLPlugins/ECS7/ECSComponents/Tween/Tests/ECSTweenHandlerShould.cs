using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using Decentraland.Common;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace Tests
{
    public class ECSTweenHandlerShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private ECSTweenHandler componentHandler;
        private IInternalECSComponent<InternalTween> internalTweenComponent;
        private IInternalECSComponent<InternalSceneBoundsCheck> internalSBCComponent;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            // var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1000);

            internalTweenComponent = Substitute.For<IInternalECSComponent<InternalTween>>();
            internalSBCComponent = Substitute.For<IInternalECSComponent<InternalSceneBoundsCheck>>();
            componentHandler = new ECSTweenHandler(internalTweenComponent, internalSBCComponent);

            // Environment.Setup(ServiceLocatorFactory.CreateDefault());
        }

        [TearDown]
        public void TearDown()
        {
            componentHandler.OnComponentRemoved(scene, entity);
            testUtils.Dispose();
            // Environment.Dispose();
        }

        [Test]
        public void CreateInternalComponentCorrectlyForMoveTween()
        {
            var model = new PBTween()
            {
                Duration = 3,
                Move = new Move()
                {
                    Start = new Decentraland.Common.Vector3() { X = 1f, Y = 1f, Z = 1f },
                    End = new Decentraland.Common.Vector3() { X = 5f, Y = 5f, Z = 5f }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            internalTweenComponent.Received(1).PutFor(scene, entity, Arg.Is<InternalTween>(comp =>
                comp.transform == entity.gameObject.transform
                && comp.tweenMode == PBTween.ModeOneofCase.Move));
        }

        [Test]
        public void CreateInternalComponentCorrectlyForRotationTween()
        {
            var model = new PBTween()
            {
                Duration = 3,
                Rotate = new Rotate()
                {
                    Start = new Decentraland.Common.Quaternion() { X = 1f, Y = 1f, Z = 1f, W = 1f },
                    End = new Decentraland.Common.Quaternion() { X = 5f, Y = 5f, Z = 5f, W = 5f }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            internalTweenComponent.Received(1).PutFor(scene, entity, Arg.Is<InternalTween>(comp =>
                comp.transform == entity.gameObject.transform
                && comp.tweenMode == PBTween.ModeOneofCase.Rotate));
        }

        [Test]
        public void CreateInternalComponentCorrectlyForScaleTween()
        {
            var model = new PBTween()
            {
                Duration = 3,
                Scale = new Scale()
                {
                    Start = new Decentraland.Common.Vector3() { X = 1f, Y = 1f, Z = 1f },
                    End = new Decentraland.Common.Vector3() { X = 5f, Y = 5f, Z = 5f }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            internalTweenComponent.Received(1).PutFor(scene, entity, Arg.Is<InternalTween>(comp =>
                comp.transform == entity.gameObject.transform
                && comp.tweenMode == PBTween.ModeOneofCase.Scale));
        }

        [Test]
        public void CreateTweenerCorrectlyForMoveTween()
        {

        }

        [Test]
        public void CreateTweenerCorrectlyForRotationTween()
        {

        }

        [Test]
        public void CreateTweenerCorrectlyForScaleTween()
        {

        }

        [Test]
        public void HandleFaceDirectionCorrectly()
        {

        }

        [Test]
        public void ResetTweenIfNewOneIsPutOnSameEntity()
        {

        }

        [Test]
        public void UseLinearEasingByDefault()
        {

        }

        [Test]
        public void BePlayingByDefault()
        {

        }
    }
}
