using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class AnimatorComponentHandler : IECSComponentHandler<PBAnimator>
    {
        private readonly DataStore_ECS7 dataStore;
        private Animation animComponent;
        
        private Dictionary<string, AnimationClip> clipNameToClip = new Dictionary<string, AnimationClip>();
        private Dictionary<AnimationClip, AnimationState> clipToState = new Dictionary<AnimationClip, AnimationState>();
        
        public AnimatorComponentHandler(DataStore_ECS7 dataStoreEcs7) { dataStore = dataStoreEcs7; }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
           
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAnimator model)
        {
            if (dataStore.animatorShapesReady.ContainsKey(entity.entityId))
            {
                UpdateAnimationState(entity, model);
            }
            else
            {
                dataStore.animatorShapesReady.OnAdded += ( entityId,  gameObject) =>
                {
                    if (entityId == entity.entityId)
                        UpdateAnimationState(entity, model);
                };
            }
        }
        
        private void Initialize(IDCLEntity entity)
        {
            if (entity == null || animComponent != null)
                return;

            //NOTE(Brian): fetch all the AnimationClips in Animation component.
            animComponent = entity.gameObject.transform.parent.GetComponentInChildren<Animation>(true);

            if (animComponent == null)
                return;

            clipNameToClip.Clear();
            clipToState.Clear();
            int layerIndex = 0;

            animComponent.playAutomatically = true;
            animComponent.enabled = true;
            animComponent.Stop(); //NOTE(Brian): When the GLTF is created by GLTFSceneImporter a frame may be elapsed,
            //putting the component in play state if playAutomatically was true at that point.
            animComponent.clip?.SampleAnimation(animComponent.gameObject, 0);

            foreach (AnimationState unityState in animComponent)
            {
                clipNameToClip[unityState.clip.name] = unityState.clip;

                unityState.clip.wrapMode = WrapMode.Loop;
                unityState.layer = layerIndex;
                unityState.blendMode = AnimationBlendMode.Blend;
                layerIndex++;
            }
        }
        
        private void UpdateAnimationState(IDCLEntity entity,PBAnimator model)
        {
            Initialize(entity);
            
            if (clipNameToClip.Count == 0 || animComponent == null)
                return;

            if (model.States.Count == 0)
                return;

            for (int i = 0; i < model.States.Count; i++)
            {
                DCLAnimationState state = new DCLAnimationState(model.States[i]);

                if (clipNameToClip.ContainsKey(state.clip))
                {
                    AnimationState unityState = animComponent[state.clip];
                    unityState.weight = state.weight;
                    unityState.wrapMode = state.loop ? WrapMode.Loop : WrapMode.Default;
                    unityState.clip.wrapMode = unityState.wrapMode;
                    unityState.speed = state.speed;

                    state.clipReference = unityState.clip;

                    unityState.enabled = state.playing;

                    if (state.shouldReset)
                        ResetAnimation(state);

                    
                    if (state.playing && !animComponent.IsPlaying(state.clip))
                    {
                        animComponent.Play(state.clip);
                    }
                }
            }
        }
        
        public void ResetAnimation(DCLAnimationState state)
        {
            if (state == null || state.clipReference == null)
            {
                Debug.LogError("Clip not found");
                return;
            }

            animComponent.Stop(state.clip);

            //Manually sample the animation. If the reset is not played again the frame 0 wont be applied
            state.clipReference.SampleAnimation(animComponent.gameObject, 0);
        }

        internal PBAnimationState GetStateByString(PBAnimator model, string stateName)
        {
            for (var i = 0; i < model.States.Count; i++)
            {
                if (model.States[i].Name == stateName)
                {
                    return model.States[i];
                }
            }

            return null;
        }
    }

    public class DCLAnimationState
    {
        public string name;
        public string clip;
        public AnimationClip clipReference;
        public bool playing;

        [Range(0, 1)]
        public float weight = 1f;

        public float speed = 1f;
        public bool loop = true;
        public bool shouldReset = false;

        public DCLAnimationState(PBAnimationState state)
        {
            name = state.Name;
            clip = state.Clip;
            playing = state.Playing;
            weight = state.Weight;
            speed = state.Speed;
            loop = state.Loop;
            shouldReset = state.ShouldReset;
        }
    }
}