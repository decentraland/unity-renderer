using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    public class ECSTweenHandlerShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private ECSTweenHandler componentHandler;
        private InternalECSComponents internalComponents;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1000);

            componentHandler = new ECSTweenHandler(internalComponents.TweenComponent, internalComponents.sceneBoundsCheckComponent);
        }

        [TearDown]
        public void TearDown()
        {
            componentHandler.OnComponentRemoved(scene, entity);
            internalComponents.TweenComponent.RemoveFor(scene, entity);
            internalComponents.sceneBoundsCheckComponent.RemoveFor(scene, entity);
            testUtils.Dispose();
        }

        [Test]
        public void CreateInternalSBCComponentCorrectly()
        {
            var internalSBC = internalComponents.sceneBoundsCheckComponent.GetFor(scene, entity);
            Assert.IsNull(internalSBC);

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

            internalSBC = internalComponents.sceneBoundsCheckComponent.GetFor(scene, entity);
            Assert.IsNotNull(internalSBC);
        }

        [Test]
        public void CreateInternalComponentCorrectlyForMoveMode()
        {
            var internalTween = internalComponents.TweenComponent.GetFor(scene, entity);
            Assert.IsNull(internalTween);

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

            internalTween = internalComponents.TweenComponent.GetFor(scene, entity);
            Assert.IsNotNull(internalTween);
            Assert.AreEqual(entity.gameObject.transform, internalTween.Value.model.transform);
            Assert.AreEqual(PBTween.ModeOneofCase.Move, internalTween.Value.model.tweenMode);
        }

        [Test]
        public void CreateInternalComponentCorrectlyForRotationMode()
        {
            var internalTween = internalComponents.TweenComponent.GetFor(scene, entity);
            Assert.IsNull(internalTween);

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

            internalTween = internalComponents.TweenComponent.GetFor(scene, entity);
            Assert.IsNotNull(internalTween);
            Assert.AreEqual(entity.gameObject.transform, internalTween.Value.model.transform);
            Assert.AreEqual(PBTween.ModeOneofCase.Rotate, internalTween.Value.model.tweenMode);
        }

        [Test]
        public void CreateInternalComponentCorrectlyForScaleMode()
        {
            var internalTween = internalComponents.TweenComponent.GetFor(scene, entity);
            Assert.IsNull(internalTween);

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

            internalTween = internalComponents.TweenComponent.GetFor(scene, entity);
            Assert.IsNotNull(internalTween);
            Assert.AreEqual(entity.gameObject.transform, internalTween.Value.model.transform);
            Assert.AreEqual(PBTween.ModeOneofCase.Scale, internalTween.Value.model.tweenMode);
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
            Vector3 startPosition = Vector3.one;
            Vector3 endPosition = new Vector3(57f, 41f, 5f);
            Transform entityTransform = entity.gameObject.transform;

            var model = new PBTween()
            {
                Duration = 3000,
                Move = new Move()
                {
                    Start = new Decentraland.Common.Vector3() { X = startPosition.x, Y = startPosition.y, Z = startPosition.z },
                    End = new Decentraland.Common.Vector3() { X = endPosition.x, Y = endPosition.y, Z = endPosition.z },
                    FaceDirection = true
                }
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check transform direction is correct
            Assert.AreEqual((endPosition - startPosition).normalized.ToString(), entityTransform.forward.ToString());
        }

        [Test]
        public void ResetTweenIfNewOneIsPutOnSameEntity()
        {
            // Setup first Tween component
            Vector3 startPosition1 = Vector3.one;
            Vector3 endPosition1 = new Vector3(5f, 5f, 5f);

            Transform entityTransform = entity.gameObject.transform;
            entityTransform.localPosition = new Vector3(50f, 50f, 50f);

            // Confirm there's no previous tween running for entity transform
            var transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNull(transformTweens);

            var model1 = new PBTween()
            {
                Duration = 3000,
                Move = new Move()
                {
                    Start = new Decentraland.Common.Vector3() { X = startPosition1.x, Y = startPosition1.y, Z = startPosition1.z },
                    End = new Decentraland.Common.Vector3() { X = endPosition1.x, Y = endPosition1.y, Z = endPosition1.z }
                }
            };
            componentHandler.OnComponentModelUpdated(scene, entity, model1);

            // Check transform is positioned at start-position
            Assert.AreEqual(entityTransform.localPosition, startPosition1);

            // Setup second Tween component
            Vector3 startPosition2 = new Vector3(37f, 22f, 11f);
            Vector3 endPosition2 = new Vector3(68f, 99f, 33f);
            var model2 = new PBTween()
            {
                Duration = 1500,
                Move = new Move()
                {
                    Start = new Decentraland.Common.Vector3() { X = startPosition2.x, Y = startPosition2.y, Z = startPosition2.z },
                    End = new Decentraland.Common.Vector3() { X = endPosition2.x, Y = endPosition2.y, Z = endPosition2.z }
                }
            };
            componentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Check transform is positioned at start-position
            Assert.AreEqual(entityTransform.localPosition, startPosition2);

            // Check transform DOTween is playing
            transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNotNull(transformTweens);
            Assert.IsTrue(transformTweens[0].IsPlaying());

            // Move tween to the end to check end-position is correct
            // (DOTween doesn't have any way to get the end value of the tween...)
            transformTweens[0].Goto(model2.Duration);
            Assert.AreEqual(endPosition2, entityTransform.localPosition);
        }

        [Test]
        public void BePlayingByDefault()
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

            // Check transform DOTween is playing by default...
            transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNotNull(transformTweens);
            Assert.IsTrue(transformTweens[0].IsPlaying());
        }

        [Test]
        public void HandlePausing()
        {
            Vector3 startPosition = Vector3.one;
            Vector3 endPosition = new Vector3(5f, 5f, 5f);
            Transform entityTransform = entity.gameObject.transform;

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

            // Check transform DOTween is playing
            var transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.IsNotNull(transformTweens);
            Assert.IsTrue(transformTweens[0].IsPlaying());

            // Pause it
            model.Playing = false;
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check it's not playing
            Assert.IsFalse(transformTweens[0].IsPlaying());

            // Unpause it
            model.Playing = true;
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check it's playing again
            Assert.IsTrue(transformTweens[0].IsPlaying());
        }

        [Test]
        public void HandleCustomInitialCurrentTime()
        {
            Vector3 startScale = Vector3.one;
            Vector3 endScale = new Vector3(5f, 5f, 5f);
            float initialTime = 0.67f;
            Transform entityTransform = entity.gameObject.transform;

            var model = new PBTween()
            {
                Duration = 3000,
                Scale = new Scale()
                {
                    Start = new Decentraland.Common.Vector3() { X = startScale.x, Y = startScale.y, Z = startScale.z },
                    End = new Decentraland.Common.Vector3() { X = endScale.x, Y = endScale.y, Z = endScale.z }
                },
                CurrentTime = initialTime
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check tween time is the custom initial one
            var transformTweens = DOTween.TweensByTarget(entityTransform, true);
            Assert.AreEqual(initialTime, transformTweens[0].ElapsedPercentage());
        }

        [Test]
        public void KillTweenerImmediatelyOnComponentRemove()
        {
            Vector3 startScale = Vector3.one;
            Vector3 endScale = new Vector3(5f, 5f, 5f);

            var model = new PBTween()
            {
                Duration = 3000,
                Scale = new Scale()
                {
                    Start = new Decentraland.Common.Vector3() { X = startScale.x, Y = startScale.y, Z = startScale.z },
                    End = new Decentraland.Common.Vector3() { X = endScale.x, Y = endScale.y, Z = endScale.z }
                },
                Playing = true
            };

            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Check tween is running
            var tweens = DOTween.PlayingTweens();
            Assert.AreEqual(1, tweens.Count);

            componentHandler.OnComponentRemoved(scene, entity);
            tweens = DOTween.PlayingTweens();
            Assert.IsNull(tweens);
        }
    }
}
