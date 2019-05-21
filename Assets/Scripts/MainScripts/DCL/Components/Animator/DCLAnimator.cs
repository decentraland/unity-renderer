using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using DCL.Helpers;
using DCL.Models;

namespace DCL.Components
{
    public class DCLAnimator : BaseComponent
    {
        [System.Serializable]
        public class Model
        {
            [System.Serializable]
            public class DCLAnimationState
            {
                public string name;
                public string clip;
                public AnimationClip clipReference;
                public bool playing;
                [Range(0, 1)]
                public float weight = 1f;
                public float speed = 1f;
                public bool looping = true;
                public bool shouldReset = false;

                public DCLAnimationState Clone()
                {
                    return (DCLAnimationState)this.MemberwiseClone();
                }

                public bool Equals(DCLAnimationState other)
                {
                    return name == other.name &&
                           clip == other.clip &&
                           playing == other.playing &&
                           weight == other.weight &&
                           looping == other.looping &&
                           speed == other.speed &&
                           shouldReset == other.shouldReset;
                }
            }

            public DCLAnimationState[] states;
        }

        public Model model = new Model();

        Model.DCLAnimationState[] previousState;
        Dictionary<string, AnimationClip> clipNameToClip = new Dictionary<string, AnimationClip>();
        Dictionary<AnimationClip, AnimationState> clipToState = new Dictionary<AnimationClip, AnimationState>();
        Animation animComponent = null;

        private void Start()
        {
            entity.OnShapeUpdated += OnComponentUpdated;

            if (entity.meshGameObject && entity.meshGameObject.GetComponentInChildren<Animation>() != null)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);
            UpdateAnimationState();
            return null;
        }

        private void OnComponentUpdated(DecentralandEntity e)
        {
            if (entity.meshGameObject && entity.meshGameObject.GetComponentInChildren<Animation>() != null)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (entity == null || entity.meshGameObject == null)
                return;

            if (previousState != null && !CheckIfDirty(model.states, previousState))
                return;

            if (model.states == null || model.states.Length == 0)
                return;


            //NOTE(Brian): fetch all the AnimationClips in Animation component.
            if (animComponent == null)
            {
                animComponent = entity.meshGameObject.GetComponentInChildren<Animation>(true);

                if (animComponent == null)
                    return;

                clipNameToClip.Clear();
                clipToState.Clear();
                int layerIndex = 0;

                animComponent.playAutomatically = false;
                animComponent.enabled = true;

                foreach (AnimationState unityState in animComponent)
                {
                    clipNameToClip[unityState.clip.name] = unityState.clip;
                    
                    unityState.clip.wrapMode = WrapMode.Loop;
                    unityState.layer = layerIndex;
                    unityState.blendMode = AnimationBlendMode.Blend;
                    layerIndex++;
                }
            }

            UpdateAnimationState();
        }

        void UpdateAnimationState()
        {
            if (clipNameToClip.Count == 0 || animComponent == null)
                return;

            animComponent.playAutomatically = false;
            animComponent.enabled = true;

            foreach (Model.DCLAnimationState state in model.states)
            {
                if (clipNameToClip.ContainsKey(state.clip))
                {
                    AnimationState unityState = animComponent[state.clip];
                    unityState.weight = state.weight;
                    unityState.wrapMode = state.looping ? WrapMode.Loop : WrapMode.ClampForever;
                    unityState.speed = state.speed;

                    state.clipReference = unityState.clip;

                    if (state.shouldReset)
                    {
                        ResetAnimation(state);
                    }

                    if (state.playing)
                    {
                        if (!animComponent.IsPlaying(state.clip))
                        {
                            animComponent.Play(state.clip);
                        }
                    }
                    else
                    {
                        if (animComponent.IsPlaying(state.clip))
                        {
                            animComponent.Stop(state.clip);
                        }
                    }
                }
            }
        }

        public void ResetAnimation(Model.DCLAnimationState state)
        {
            animComponent.Stop(state.clip);
            animComponent.Play(state.clip);
        }

        public Model.DCLAnimationState GetStateByString(string stateName)
        {
            for (var i = 0; i < model.states.Length; i++)
            {
                if (model.states[i].name == stateName)
                {
                    return model.states[i];
                }
            }

            return null;
        }


        bool CheckIfDirty(Model.DCLAnimationState[] states1, Model.DCLAnimationState[] states2)
        {
            if (states1 == null || states2 == null)
                return true;

            if (states1.Length != states2.Length)
                return true;

            //TODO(Brian): bad performance, fix later?
            for (int i = 0; i < states2.Length; i++)
            {
                for (int j = 0; j < states1.Length; j++)
                {
                    if (states1[j].name == states2[i].name && !states1[j].Equals(states2[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }

}
