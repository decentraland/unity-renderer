using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

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

                public DCLAnimationState Clone() =>
                    (DCLAnimationState) this.MemberwiseClone();
            }

            public DCLAnimationState[] states;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.Animator)
                    return Utils.SafeUnimplemented<DCLAnimator, Model>(expected: ComponentBodyPayload.PayloadOneofCase.Animator, actual: pbModel.PayloadCase);

                if (pbModel.Animator.States.Count == 0)
                    return new Model();

                var model = new Model { states = new DCLAnimationState[pbModel.Animator.States.Count] };

                for (var i = 0; i < pbModel.Animator.States.Count; i++)
                {
                    model.states[i] = new DCLAnimationState();

                    if (pbModel.Animator.States[i].HasName) model.states[i].name = pbModel.Animator.States[i].Name;
                    if (pbModel.Animator.States[i].HasClip) model.states[i].clip = pbModel.Animator.States[i].Clip;
                    if (pbModel.Animator.States[i].HasPlaying) model.states[i].playing = pbModel.Animator.States[i].Playing;
                    if (pbModel.Animator.States[i].HasWeight) model.states[i].weight = pbModel.Animator.States[i].Weight;
                    if (pbModel.Animator.States[i].HasSpeed) model.states[i].speed = pbModel.Animator.States[i].Speed;
                    if (pbModel.Animator.States[i].HasLooping) model.states[i].looping = pbModel.Animator.States[i].Looping;
                    if (pbModel.Animator.States[i].HasShouldReset) model.states[i].shouldReset = pbModel.Animator.States[i].ShouldReset;
                }

                return model;
            }
        }

        [System.NonSerialized]
        public Animation animComponent = null;

        private string lastLoadedModelSrc;

        Dictionary<string, AnimationClip> clipNameToClip = new Dictionary<string, AnimationClip>();
        Dictionary<AnimationClip, AnimationState> clipToState = new Dictionary<AnimationClip, AnimationState>();

        public override string componentName => "animator";

        private void Awake()
        {
            model = new Model();
        }

        private void OnDestroy()
        {
            if (entity == null) return;

            entity.OnShapeLoaded -= OnEntityShapeLoaded;

            if (scene.componentsManagerLegacy.GetSharedComponent(entity, typeof(BaseShape)) is LoadableShape animationShape)
                animationShape.OnLoaded -= OnShapeLoaded;
        }

        public override IEnumerator ApplyChanges(BaseModel model)
        {
            entity.OnShapeLoaded -= OnEntityShapeLoaded;
            entity.OnShapeLoaded += OnEntityShapeLoaded;

            //Note: If the entity is still loading the Shape, We wait until it is fully loaded to init it
            //      If we don't wait, this can cause an issue with the asset bundles not loadings animations

            if (IsEntityShapeLoaded())
                UpdateAnimationState();

            return null;
        }

        public new Model GetModel() =>
            (Model) model;

        private bool IsEntityShapeLoaded() =>
            (scene.componentsManagerLegacy.GetSharedComponent(entity, typeof(BaseShape)) as LoadableShape)
            is { isLoaded: true };

        private void OnEntityShapeLoaded(IDCLEntity shapeEntity)
        {
            var animationShape = scene.componentsManagerLegacy.GetSharedComponent(entity, (typeof(BaseShape))) as LoadableShape;

            if (animationShape?.GetModel() is not LoadableShape.Model shapeModel)
                return;

            if ( shapeModel.src == lastLoadedModelSrc )
                return;

            lastLoadedModelSrc = shapeModel.src;

            if (animationShape.isLoaded)
            {
                UpdateAnimationState();
            }
            else
            {
                animationShape.OnLoaded -= OnShapeLoaded;
                animationShape.OnLoaded += OnShapeLoaded;
            }
        }

        private void OnShapeLoaded(LoadableShape shape)
        {
            shape.OnLoaded -= OnShapeLoaded;
            UpdateAnimationState();
        }

        private void Initialize()
        {
            if (entity == null || animComponent != null)
                return;

            //NOTE(Brian): fetch all the AnimationClips in Animation component.
            animComponent = transform.parent.GetComponentInChildren<Animation>(true);

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
                    unityState.wrapMode = state.looping ? WrapMode.Loop : WrapMode.Default;
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

        public override int GetClassId() { return (int) CLASS_ID_COMPONENT.ANIMATOR; }
    }
}
