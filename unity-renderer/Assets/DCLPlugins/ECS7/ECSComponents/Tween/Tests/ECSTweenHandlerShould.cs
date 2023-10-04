using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DG.Tweening;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;
using Vector3 = UnityEngine.Vector3;

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
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
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
        public void CreateInternalComponentCorrectlyForMoveMode()
        {
            Transform entityTransform = entity.gameObject.transform;
            entityTransform.position = new Vector3(50f, 50f, 50f);

            var model = new PBTween()
            {
                Duration = 3000,
                Move = new Move()
                {
                    Start = new Decentraland.Common.Vector3() { X = 1f, Y = 1f, Z = 1f },
                    End = new Decentraland.Common.Vector3() { X = 5f, Y = 5f, Z = 5f }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            internalTweenComponent.Received(1).PutFor(scene, entity, Arg.Is<InternalTween>(comp =>
                comp.transform == entityTransform
                && comp.tweenMode == PBTween.ModeOneofCase.Move));
        }

        [Test]
        public void CreateInternalComponentCorrectlyForRotationMode()
        {
            var model = new PBTween()
            {
                Duration = 3000,
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
        public void CreateInternalComponentCorrectlyForScaleMode()
        {
            var model = new PBTween()
            {
                Duration = 3000,
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
        public void CreateTweenerCorrectlyForMoveMode()
        {
            Vector3 startPosition = Vector3.one;
            Vector3 endPosition = new Vector3(5f, 5f, 5f);

            Transform entityTransform = entity.gameObject.transform;
            entityTransform.localPosition = new Vector3(50f, 50f, 50f);

            // Confirm there's no previous tween running for entity transform
            var transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNull(transformTweens);

            var model = new PBTween()
            {
                Duration = 3000,
                Move = new Move()
                {
                    Start = new Decentraland.Common.Vector3() { X = startPosition.x, Y = startPosition.y, Z = startPosition.z },
                    End = new Decentraland.Common.Vector3() { X = endPosition.x, Y = endPosition.y, Z = endPosition.z }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check transform is positioned at start-position
            Assert.AreEqual(entityTransform.localPosition, startPosition);

            // Check transform DOTween is playing
            transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNotNull(transformTweens);
            Assert.IsTrue(transformTweens[0].IsPlaying());

            // Move tween to the end to check end-position is correct
            // (DOTween doesn't have any way to get the end value of the tween...)
            transformTweens[0].Goto(model.Duration);
            Assert.AreEqual(endPosition, entityTransform.localPosition);
        }

        [Test]
        public void CreateTweenerCorrectlyForRotationMode()
        {
            Quaternion startRotation = Quaternion.Euler(15f, 30f, 45f);
            Quaternion endRotation = Quaternion.Euler(0f, 90f, 0f);

            Transform entityTransform = entity.gameObject.transform;
            entityTransform.localRotation = Quaternion.identity;

            // Confirm there's no previous tween running for entity transform
            var transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNull(transformTweens);

            var model = new PBTween()
            {
                Duration = 3000,
                Rotate = new Rotate()
                {
                    Start = new Decentraland.Common.Quaternion()
                    {
                        X = startRotation.x,
                        Y = startRotation.y,
                        Z = startRotation.z,
                        W = startRotation.w
                    },
                    End = new Decentraland.Common.Quaternion()
                    {
                        X = endRotation.x,
                        Y = endRotation.y,
                        Z = endRotation.z,
                        W = endRotation.w
                    },
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check transform is positioned at start-rotation
            Assert.AreEqual(entityTransform.localRotation.ToString(), startRotation.ToString());

            // Check transform DOTween is playing
            transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNotNull(transformTweens);
            Assert.IsTrue(transformTweens[0].IsPlaying());

            // Move tween to the end to check end-rotation is correct
            // (DOTween doesn't have any way to get the end value of the tween...)
            transformTweens[0].Goto(model.Duration);
            Assert.AreEqual(endRotation.ToString(), entityTransform.localRotation.ToString());
        }

        [Test]
        public void CreateTweenerCorrectlyForScaleMode()
        {
            Vector3 startScale = Vector3.one;
            Vector3 endScale = new Vector3(5f, 5f, 5f);

            Transform entityTransform = entity.gameObject.transform;
            entityTransform.localScale = new Vector3(50f, 50f, 50f);

            // Confirm there's no previous tween running for entity transform
            var transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNull(transformTweens);

            var model = new PBTween()
            {
                Duration = 3000,
                Scale = new Scale()
                {
                    Start = new Decentraland.Common.Vector3() { X = startScale.x, Y = startScale.y, Z = startScale.z },
                    End = new Decentraland.Common.Vector3() { X = endScale.x, Y = endScale.y, Z = endScale.z }
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check transform is scaled with start-scale
            Assert.AreEqual(entityTransform.localScale, startScale);

            // Check transform DOTween is playing
            transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNotNull(transformTweens);
            Assert.IsTrue(transformTweens[0].IsPlaying());

            // Move tween to the end to check end-scale is correct
            // (DOTween doesn't have any way to get the end value of the tween...)
            transformTweens[0].Goto(model.Duration);
            Assert.AreEqual(endScale, entityTransform.localScale);
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

        [Test]
        public void HandlePausing()
        {

        }
    }
}
