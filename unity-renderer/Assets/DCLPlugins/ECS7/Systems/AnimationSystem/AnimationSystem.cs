using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.AnimationSystem
{
    public class AnimationSystem
    {
        private IECSReadOnlyComponentsGroup<InternalAnimationPlayer, InternalAnimation> animationGroup;
        private readonly IInternalECSComponent<InternalAnimation> animationComponent;

        public AnimationSystem(IECSReadOnlyComponentsGroup<InternalAnimationPlayer, InternalAnimation> animationGroup,
            IInternalECSComponent<InternalAnimation> animationComponent)
        {
            this.animationGroup = animationGroup;
            this.animationComponent = animationComponent;
        }

        public void Update()
        {
            var group = animationGroup.group;

            for (int i = 0; i < group.Count; i++)
            {
                var elementData = group[i];
                var animationPlayerModel = elementData.componentData1.model;
                var animationModel = elementData.componentData2.model;

                if (!animationPlayerModel.dirty && !animationModel.dirty)
                    continue;

                if (!animationModel.IsInitialized)
                {
                    SetupAnimation(animationModel.Animation);
                    animationModel.IsInitialized = true;
                    animationComponent.PutFor(elementData.scene, elementData.entity, animationModel);
                }

                SetAnimationState(animationPlayerModel.States, animationModel.Animation);
            }
        }

        private static void SetupAnimation(Animation animation)
        {
            int layerIndex = 0;

            animation.playAutomatically = true;
            animation.enabled = true;
            animation.Stop();

            //putting the component in play state if playAutomatically was true at that point.
            if (animation.clip)
                animation.clip.SampleAnimation(animation.gameObject, 0);

            foreach (AnimationState unityState in animation)
            {
                unityState.clip.wrapMode = WrapMode.Loop;
                unityState.layer = layerIndex;
                unityState.blendMode = AnimationBlendMode.Blend;
                layerIndex++;
            }
        }

        private static void SetAnimationState(IList<InternalAnimationPlayer.State> animationState, Animation animation)
        {
            if (animationState.Count == 0)
                return;

            for (int i = 0; i < animationState.Count; i++)
            {
                var state = animationState[i];
                AnimationState unityState = animation[state.Clip];

                if (!unityState)
                    continue;

                unityState.weight = state.Weight;

                unityState.wrapMode = state.Loop ? WrapMode.Loop : WrapMode.Default;

                unityState.clip.wrapMode = unityState.wrapMode;
                unityState.speed = state.Speed;
                unityState.enabled = state.Playing;

                if (state.ShouldReset && animation.IsPlaying(state.Clip))
                {
                    animation.Stop(state.Clip);

                    //Manually sample the animation. If the reset is not played again the frame 0 wont be applied
                    unityState.clip.SampleAnimation(animation.gameObject, 0);
                }

                if (state.Playing && !animation.IsPlaying(state.Clip))
                    animation.Play(state.Clip);
            }
        }
    }
}
