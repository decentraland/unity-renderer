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
        private IDCLEntity entity;
        private PBAnimator model;
        internal Animation animComponent;
        internal bool isShapeLoaded = false;
        
        internal Dictionary<string, AnimationClip> clipNameToClip = new Dictionary<string, AnimationClip>();
        
        public AnimatorComponentHandler(DataStore_ECS7 dataStoreEcs7) { dataStore = dataStoreEcs7; }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            dataStore.shapesReady.OnAdded -= ShapeReadyAdded;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAnimator model)
        {
            this.entity = entity;
            this.model = model;
            
            // If shape is loaded we just update the state of the animation with the model info
            if (isShapeLoaded)
            {
                UpdateAnimationState(model);
            }
            else
            {
                // If shape is not loaded, we check if it is already loaded, if not we subscribe to when it is ready
                if (dataStore.shapesReady.TryGetValue(entity.entityId, out GameObject gameObject))
                {
                    ShapeReadyAdded(entity.entityId, gameObject);
                }
                else
                {
                    dataStore.shapesReady.OnAdded -= ShapeReadyAdded;
                    dataStore.shapesReady.OnAdded += ShapeReadyAdded;
                }
            }
        }

        private void ShapeReadyAdded(long entityId, GameObject gameObject)
        {
            if (entityId != entity.entityId)
                return;

            Initialize(entity);
            isShapeLoaded = true;

            UpdateAnimationState(model);
        }

        internal void Initialize(IDCLEntity entity)
        {
            if (entity == null)
                return;

            //NOTE(Brian): fetch all the AnimationClips in Animation component.
            animComponent = entity.gameObject.GetComponentInChildren<Animation>(true);

            if (animComponent == null)
                return;

            clipNameToClip.Clear();
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

        internal void UpdateAnimationState(PBAnimator model)
        {
            if (clipNameToClip.Count == 0 || animComponent == null)
                return;

            if (model.States.Count == 0)
                return;
            
            for (int i = 0; i < model.States.Count; i++)
            {
                if (clipNameToClip.ContainsKey(model.States[i].Clip))
                {
                    AnimationState unityState = animComponent[model.States[i].Clip];
                    unityState.weight = model.States[i].GetWeight();

                    unityState.wrapMode = model.States[i].GetLoop() ? WrapMode.Loop : WrapMode.Default;

                    unityState.clip.wrapMode = unityState.wrapMode;
                    unityState.speed = model.States[i].GetSpeed();
                    unityState.enabled = model.States[i].Playing;

                    if (model.States[i].ShouldReset)
                        ResetAnimation(model.States[i], unityState.clip);

                    if (model.States[i].Playing && !animComponent.IsPlaying(model.States[i].Clip))
                        animComponent.Play(model.States[i].Clip);
                }
            }
        }
        
        private void ResetAnimation(PBAnimationState state, AnimationClip clipReference)
        {
            if (state == null || clipReference == null)
            {
                Debug.LogError("Clip not found");
                return;
            }

            animComponent.Stop(state.Clip);

            //Manually sample the animation. If the reset is not played again the frame 0 wont be applied
            clipReference.SampleAnimation(animComponent.gameObject, 0);
        }
    }
}