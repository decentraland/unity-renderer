using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DG.Tweening;
using ECSSystems.TweenSystem;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TestUtils;
using UnityEngine;

namespace Tests
{
    public class ECSTweenSystemShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private InternalECSComponents internalComponents;
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;
        private Action systemUpdate;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();
            var componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessages) }
            };

            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1000);

            var system = new ECSTweenSystem(
                internalComponents.TweenComponent,
                componentsWriter,
                new WrappedComponentPool<IWrappedComponent<PBTweenState>>(0, ()=> new ProtobufWrappedComponent<PBTweenState>(new PBTweenState())),
                new WrappedComponentPool<IWrappedComponent<ECSTransform>>(0, ()=> new TransformWrappedComponent(new ECSTransform())),
                Substitute.For<Vector3Variable>(),
                internalComponents.sceneBoundsCheckComponent
            );
            systemUpdate = () =>
            {
                internalComponents.MarkDirtyComponentsUpdate();
                system.Update();
                internalComponents.ResetDirtyComponentsUpdate();
            };
        }

        [TearDown]
        public void TearDown()
        {
            internalComponents.TweenComponent.RemoveFor(scene, entity);
            internalComponents.sceneBoundsCheckComponent.RemoveFor(scene, entity);
            testUtils.Dispose();
        }

        [Test]
        public void AttachAndUpdateTweenStateComponent()
        {
            float currentTime = 0.666f;
            Transform entityTransform = entity.gameObject.transform;

            // Stuff that the handler does
            Tweener tweener = entityTransform.DOLocalMove(Vector3.up, 9);
            tweener.Goto(9 * currentTime);
            internalComponents.TweenComponent.PutFor(scene, entity, new InternalTween()
            {
                transform = entityTransform,
                tweener = tweener,
                tweenMode = PBTween.ModeOneofCase.Move,
                currentTime = currentTime,
                playing = true,
            });

            systemUpdate();

            outgoingMessages.Put_Called<PBTweenState>(
                entity.entityId,
                ComponentID.TWEEN_STATE,
                componentModel =>
                    componentModel.CurrentTime.Equals(currentTime)
                    && componentModel.State == TweenStateStatus.TsActive
            );
        }

        [Test]
        public void UpdateTransformComponentCorrectly()
        {
            entity.parentId = 6966;
            float currentTime = 0.666f;
            float duration = 9f;
            Transform entityTransform = entity.gameObject.transform;

            // Stuff that the handler does
            Tweener tweener = entityTransform.DOLocalMove(Vector3.up, duration);
            internalComponents.TweenComponent.PutFor(scene, entity, new InternalTween()
            {
                transform = entityTransform,
                tweener = tweener,
                tweenMode = PBTween.ModeOneofCase.Move,
                currentTime = currentTime,
                playing = true,
            });

            tweener.Goto(duration * currentTime);
            systemUpdate();
            outgoingMessages.Put_Called<ECSTransform>(
                entity.entityId,
                ComponentID.TRANSFORM,
                componentModel =>
                    componentModel.position == entityTransform.localPosition
                    && componentModel.parentId == entity.parentId
            );
            Vector3 midPosition = entityTransform.localPosition;

            currentTime = 1f;
            tweener.Goto(duration * currentTime);
            systemUpdate();
            outgoingMessages.Put_Called<ECSTransform>(
                entity.entityId,
                ComponentID.TRANSFORM,
                componentModel =>
                    componentModel.position == entityTransform.localPosition
                    && componentModel.parentId == entity.parentId
            );
            Assert.AreNotEqual(midPosition, entityTransform.localPosition);
        }

        [Test]
        public void UpdateInternalSBCComponent()
        {
            float currentTime = 0.666f;
            float duration = 9f;
            Transform entityTransform = entity.gameObject.transform;

            // Stuff that the handler does
            Tweener tweener = entityTransform.DOLocalMove(Vector3.up, duration);
            internalComponents.TweenComponent.PutFor(scene, entity, new InternalTween()
            {
                transform = entityTransform,
                tweener = tweener,
                tweenMode = PBTween.ModeOneofCase.Move,
                currentTime = currentTime,
                playing = true,
            });

            tweener.Goto(duration * currentTime);
            systemUpdate();
            var sbcModel = internalComponents.sceneBoundsCheckComponent.GetFor(scene, entity).Value.model;
            Assert.AreEqual(entityTransform.position, sbcModel.entityPosition);

            currentTime = 1f;
            tweener.Goto(duration * currentTime);
            systemUpdate();
            sbcModel = internalComponents.sceneBoundsCheckComponent.GetFor(scene, entity).Value.model;
            Assert.AreEqual(entityTransform.position, sbcModel.entityPosition);
        }
    }
}
