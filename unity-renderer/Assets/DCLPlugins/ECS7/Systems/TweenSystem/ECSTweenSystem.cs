using DCL.Controllers;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.TweenSystem
{
    public class ECSTweenSystem
    {
        private readonly IInternalECSComponent<InternalTween> tweenInternalComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool;
        private readonly WrappedComponentPool<IWrappedComponent<ECSTransform>> transformComponentPool;
        private readonly Vector3Variable worldOffset;
        private readonly IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent;

        public ECSTweenSystem(IInternalECSComponent<InternalTween> tweenInternalComponent,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool,
            WrappedComponentPool<IWrappedComponent<ECSTransform>> transformComponentPool,
            Vector3Variable worldOffset,
            IInternalECSComponent<InternalSceneBoundsCheck> sbcInternalComponent)
        {
            this.tweenInternalComponent = tweenInternalComponent;
            this.componentsWriter = componentsWriter;
            this.tweenStateComponentPool = tweenStateComponentPool;
            this.transformComponentPool = transformComponentPool;
            this.worldOffset = worldOffset;
            this.sbcInternalComponent = sbcInternalComponent;
        }

        public void Update()
        {
            var tweenComponentGroup = tweenInternalComponent.GetForAll();
            int entitiesCount = tweenComponentGroup.Count;

            for (int i = 0; i < entitiesCount; i++)
            {
                var tweenStatePooledComponent = tweenStateComponentPool.Get();
                var tweenStateComponentModel = tweenStatePooledComponent.WrappedComponent.Model;
                IParcelScene scene = tweenComponentGroup[i].key1;
                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                long entityId = tweenComponentGroup[i].key2;
                InternalTween model = tweenComponentGroup[i].value.model;

                if (model.removed)
                {
                    model.tweener.Kill();
                    writer.Remove(entityId, ComponentID.TWEEN_STATE);
                    continue;
                }

                float currentTime = model.tweener.ElapsedPercentage();
                if (currentTime.Equals(1f) && model.currentTime.Equals(1f))
                    continue;

                bool playing = model.playing;

                if (playing)
                {
                    tweenStateComponentModel.CurrentTime = currentTime;
                    tweenStateComponentModel.State = currentTime.Equals(1f) ? TweenStateStatus.TsCompleted : TweenStateStatus.TsActive;

                    scene.entities.TryGetValue(entityId, out IDCLEntity entity);
                    switch (model.tweenMode)
                    {
                        case PBTween.ModeOneofCase.Move:
                            sbcInternalComponent.SetPosition(scene, entity, model.transform.position);
                            break;
                        case PBTween.ModeOneofCase.Rotate:
                        case PBTween.ModeOneofCase.Scale:
                            sbcInternalComponent.OnTransformScaleRotationChanged(scene, entity);
                            break;
                    }
                }
                else
                {
                    tweenStateComponentModel.State = TweenStateStatus.TsPaused;
                }

                writer.Put(entityId, ComponentID.TWEEN_STATE, tweenStatePooledComponent);

                UpdateTransformComponent(scene, entityId, model.transform, writer);

                model.currentTime = currentTime;
                tweenInternalComponent.PutFor(scene, entityId, model);
            }
        }

        private void UpdateTransformComponent(IParcelScene scene, long entity, Transform entityTransform, ComponentWriter writer)
        {
            var transformComponent = transformComponentPool.Get();
            var transformComponentModel = transformComponent.WrappedComponent.Model;
            Vector3 currentWorldOffset = worldOffset.Get();
            var newPosition = entityTransform.localPosition;
            transformComponentModel.position = UtilsScene.GlobalToScenePosition(ref scene.sceneData.basePosition, ref newPosition, ref currentWorldOffset);
            transformComponentModel.rotation = entityTransform.localRotation;
            transformComponentModel.scale = entityTransform.localScale;
            writer.Put(entity, ComponentID.TRANSFORM, transformComponent);
        }
    }
}
