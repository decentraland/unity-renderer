using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using DCL.Controllers;

namespace DCL.Components
{
    public class DCLAnimator : BaseComponent
    {
        [System.Serializable]
        public class Model : BaseModel
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
                    return (DCLAnimationState) this.MemberwiseClone();
                }
            }

            public DCLAnimationState[] states;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        [System.NonSerialized]
        public Animation animComponent = null;
        
        Dictionary<string, AnimationClip> clipNameToClip = new Dictionary<string, AnimationClip>();
        Dictionary<AnimationClip, AnimationState> clipToState = new Dictionary<AnimationClip, AnimationState>();

        private void Awake()
        {
            model = new Model();
        }

        private void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;
        }

        public override IEnumerator ApplyChanges(BaseModel model)
        {
            entity.OnShapeUpdated -= OnComponentUpdated;
            entity.OnShapeUpdated += OnComponentUpdated;

            UpdateAnimationState();

            return null;
        }

        new public Model GetModel()
        {
            return (Model) model;
        }

        private void OnComponentUpdated(IDCLEntity e)
        {
            UpdateAnimationState();
        }

        private void Initialize()
        {
            if (entity == null || animComponent != null) return;

            //NOTE(Brian): fetch all the AnimationClips in Animation component.
            animComponent = transform.parent.GetComponentInChildren<Animation>(true);

            if (animComponent == null) return;

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

        void UpdateAnimationState()
        {
            Initialize();

            if (clipNameToClip.Count == 0 || animComponent == null)
                return;

            Model model = (Model) this.model;

            if (model.states == null || model.states.Length == 0)
                return;

            for (int i = 0; i < model.states.Length; i++)
            {
                Model.DCLAnimationState state = model.states[i];

                if (clipNameToClip.ContainsKey(state.clip))
                {
                    AnimationState unityState = animComponent[state.clip];
                    unityState.weight = state.weight;
                    unityState.wrapMode = state.looping ? WrapMode.Loop : WrapMode.ClampForever;
                    unityState.clip.wrapMode = unityState.wrapMode;
                    unityState.speed = state.speed;

                    state.clipReference = unityState.clip;

                    if (state.shouldReset)
                        ResetAnimation(state);

                    if (state.playing)
                    {
                        if (!animComponent.IsPlaying(state.clip))
                            animComponent.Play(state.clip);
                    }
                    else
                    {
                        if (animComponent.IsPlaying(state.clip))
                            animComponent.Stop(state.clip);
                    }
                }
            }
        }

        public void ResetAnimation(Model.DCLAnimationState state)
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

        public Model.DCLAnimationState GetStateByString(string stateName)
        {
            Model model = (Model) this.model;

            for (var i = 0; i < model.states.Length; i++)
            {
                if (model.states[i].name == stateName)
                {
                    return model.states[i];
                }
            }

            return null;
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID_COMPONENT.ANIMATOR;
        }
    }
}