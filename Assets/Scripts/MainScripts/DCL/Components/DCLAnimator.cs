using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using DCL.Helpers;

namespace DCL.Components
{
    public class DCLAnimatorClipPlayableBehaviour : PlayableBehaviour
    {
        public Dictionary<string, AnimationClipPlayable> stateToPlayable;
        public DCLAnimator dclComponent;

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (stateToPlayable != null)
            {
                foreach (var statePlayablePair in stateToPlayable)
                {
                    DCLAnimator.Model.DCLAnimationState animState = dclComponent.GetStateByString(statePlayablePair.Key);

                    if (animState.playing && animState.looping)
                    {
                        if (statePlayablePair.Value.GetPlayState() == PlayState.Playing)
                        {
                            double clipTime = statePlayablePair.Value.GetTime();

                            if (clipTime + info.deltaTime >= animState.clipReference.length)
                            {
                                double error = animState.clipReference.length - clipTime + info.deltaTime;
                                statePlayablePair.Value.SetTime(error);
                                statePlayablePair.Value.Play();
                            }
                        }
                    }
                }
            }

            base.PrepareFrame(playable, info);
        }
    }

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
                public float weight;
                public float speed;
                public bool looping;

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
                      speed == other.speed;
                }
            }

            public DCLAnimationState[] states;
        }

        public Model model = new Model();
        public override string componentName => "animator";

        Model.DCLAnimationState[] previousState;

        PlayableGraph playableGraph;
        AnimationPlayableOutput playableOutput;
        AnimationMixerPlayable mixerPlayable;
        Animator animator;

        Dictionary<string, AnimationClipPlayable> stateToPlayable = new Dictionary<string, AnimationClipPlayable>();
        Dictionary<string, AnimationClip> clipNameToClip = new Dictionary<string, AnimationClip>();

        private void Start()
        {
            entity.OnShapeUpdated += OnComponentUpdated;

            if (GetComponentInChildren<Animation>() != null)
            {
                Initialize();
            }
        }

        private void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;

            if (playableGraph.IsValid())
                playableGraph.Destroy();

            if (mixerPlayable.IsValid())
                mixerPlayable.Destroy();

            if (stateToPlayable != null)
            {
                foreach (var acp in stateToPlayable)
                {
                    acp.Value.Destroy();
                }
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            if (!Helpers.Utils.SafeFromJsonOverwrite(newJson, model))
                yield break;

            Initialize();
        }

        private void OnComponentUpdated()
        {
            if (entity.meshGameObject && entity.meshGameObject.GetComponentInChildren<Animation>() != null)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            if (previousState != null && !CheckIfDirty(model.states, previousState))
            {
                return;
            }

            if (model.states == null || model.states.Length == 0)
                return;

            clipNameToClip.Clear();

            Animation animComponent = GetComponentInChildren<Animation>();

            //NOTE(Brian): fetch all the AnimationClips in Animation component.
            if (animComponent != null)
            {
                foreach (AnimationState s in animComponent)
                {
                    clipNameToClip[s.clip.name] = s.clip;
                    //NOTE(Brian): This is important, legacy clips aren't supported by playables.
                    //             If this is 'true' it gives an assert error.
                    s.clip.legacy = false;
                    s.clip.wrapMode = WrapMode.Loop;
                }

                if (animator == null)
                {
                    animator = animComponent.gameObject.AddComponent<Animator>();
                }
            }

            if (clipNameToClip.Count == 0)
                return;

            //NOTE(Brian): basic playable graph setup.
            if (!playableGraph.IsValid())
            {
                playableGraph = PlayableGraph.Create();
            }

            if (!mixerPlayable.IsValid())
            {
                mixerPlayable = AnimationMixerPlayable.Create(playableGraph, model.states.Length, true);
            }
            else
            {
                if (model.states.Length > previousState.Length)
                {
                    mixerPlayable.SetInputCount(model.states.Length);
                }
            }

            //NOTE(Brian): connect an AnimationClipPlayables to the mixer for each clip.
            foreach (Model.DCLAnimationState state in model.states)
            {
                if (!stateToPlayable.ContainsKey(state.name))
                {
                    if (clipNameToClip.ContainsKey(state.clip))
                    {
                        AnimationClip animClip = clipNameToClip[state.clip];

                        state.clipReference = animClip;
                        AnimationClipPlayable p = AnimationClipPlayable.Create(playableGraph, animClip);
                        p.Pause();

                        stateToPlayable[state.name] = p;

                        int freeSlot = 0;

                        //NOTE(Brian): Have to do this because we are forced to pass the slot index in a explicit way.
                        for (int slotIndex = 0; slotIndex < mixerPlayable.GetInputCount(); slotIndex++)
                        {
                            if (!mixerPlayable.GetInput(slotIndex).IsValid())
                            {
                                freeSlot = slotIndex;
                                break;
                            }
                        }

                        playableGraph.Connect(p, 0, mixerPlayable, freeSlot);
                    }
                }
            }

            //NOTE(Brian): Mixer is connected to this custom playable behaviour, and this to the output playable.
            //             This is done to implement loop behaviour. And maybe more advanced stuff later.
            //             We could make the looping in Update() too but this feels more legit.
            var blenderPlayable = ScriptPlayable<DCLAnimatorClipPlayableBehaviour>.Create(playableGraph, 1);
            blenderPlayable.GetBehaviour().dclComponent = this;
            blenderPlayable.GetBehaviour().stateToPlayable = stateToPlayable;

            playableGraph.Connect(mixerPlayable, 0, blenderPlayable, 0);

            if (!playableOutput.IsOutputValid())
            {
                playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
                playableOutput.SetSourcePlayable(blenderPlayable);
            }

            playableGraph.Play();

            //NOTE(Brian): We want to make a deep-ish copy of the current model so we can check for dirtyness
            //             later.
            previousState = new Model.DCLAnimationState[model.states.Length];

            for (int i = 0; i < model.states.Length; i++)
            {
                previousState[i] = model.states[i].Clone();
            }

            UpdateAnimatorState();
        }

        /// <summary>
        /// This method updates playable and weight animations attributes.
        /// </summary>
        private void UpdateAnimatorState()
        {
            foreach (var statePlayablePair in stateToPlayable)
            {
                Model.DCLAnimationState animState = GetStateByString(statePlayablePair.Key);

                if (animState.playing)
                {
                    //TODO(Brian): insert here the lerp transition code
                    mixerPlayable.SetInputWeight(statePlayablePair.Value, animState.weight);
                    if (statePlayablePair.Value.GetPlayState() != PlayState.Playing)
                    {
                        statePlayablePair.Value.Play();
                    }
                }
                else
                {
                    mixerPlayable.SetInputWeight(statePlayablePair.Value, 0);

                    if (statePlayablePair.Value.GetPlayState() == PlayState.Playing)
                    {
                        statePlayablePair.Value.Pause();
                        statePlayablePair.Value.SetTime(0);
                    }
                }
            }
        }

        public Model.DCLAnimationState GetStateByString(string stateName)
        {
            foreach (Model.DCLAnimationState state in model.states)
            {
                if (state.name == stateName)
                {
                    return state;
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
