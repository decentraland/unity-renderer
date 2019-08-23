using DCL.Components;
using DCL.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityGLTF;

namespace DCL
{
    public class AvatarShape : BaseComponent
    {
        public static bool VERBOSE = false;

        public const string CATEGORY_EYES = "eyes";
        public const string CATEGORY_EYEBROWS = "eyebrows";
        public const string CATEGORY_MOUTH = "mouth";
        public const string CATEGORY_HAIR = "hair";

        public const string MATERIAL_FILTER_HAIR = "hair";
        public const string MATERIAL_FILTER_SKIN = "skin";

        public const string MALE_BODY_NAME = "male";
        public const string FEMALE_BODY_NAME = "female";

        private const float MOVEMENT_SPEED = 2.5f;
        private const float ROTATION_SCALE_TIME = 0.25f;


        enum State
        {
            IDLE,
            LOADING,
            READY,
            FAILED
        }

        [System.Serializable]
        public class Model
        {
            [System.Serializable]
            public class Wearable
            {
                public string category;
                public string contentName;
                public ContentProvider.MappingPair[] contents = new ContentProvider.MappingPair[] { };
                public ContentProvider provider; //NOTE(Brian): For Unity uses only
            }

            [System.Serializable]
            public class Snapshots
            {
                public string face;
                public string body;
            }

            [System.Serializable]
            public class Skin
            {
                public Color color;
            }

            [System.Serializable]
            public class Hair
            {
                public Color color;
            }

            [System.Serializable]
            public class Face
            {
                public string mask;
                public string texture;
            }

            [System.Serializable]
            public class Eyes : Face
            {
                public Color color;
            }

            public string baseUrl;
            public string name;

            public Wearable bodyShape = new Wearable();
            public Wearable[] wearables = new Wearable[] { };
            public Snapshots snapshots;
            public Skin skin;
            public Hair hair;
            public Eyes eyes;
            public Face eyebrows;
            public Face mouth;

            public bool useDummyModel = true;
        }


        public Material defaultMaterial;
        public Material eyeMaterial;
        public Material eyebrowMaterial;
        public Material mouthMaterial;

        public AvatarName avatarName;
        public AvatarMovementController avatarMovementController;

        public AnimationClip[] maleAnims;
        public AnimationClip[] femaleAnims;

        [SerializeField] private GameObject minimapRepresentation;

        int loadedWearablesCount = 0;

        State state = State.IDLE;
        bool loadingHasStarted = false;
        bool baseBodyIsReady = false;
        bool eyesReady = false;
        bool browsReady = false;
        bool mouthReady = false;

        public Model model = new Model();

        [NonSerialized] public LoadWrapper baseBody;
        [NonSerialized] public List<LoadWrapper> wearables;
        public bool everythingIsLoaded { get { return state == State.READY && eyesReady && mouthReady && browsReady; } }
        public bool isLoading { get { return state == State.LOADING; } }

        Material eyeMaterialCopy;
        Material eyebrowMaterialCopy;
        Material mouthMaterialCopy;
        List<Material> defaultMaterialCopies;

        AvatarRandomizer randomizer;

        void Start()
        {
            InitMaterials();
            minimapRepresentation.SetActive(false);
        }

        void OnDestroy()
        {
            UnloadMaterials();

            baseBody?.Unload();

            if (wearables != null)
            {
                for (int index = 0; index < wearables.Count; index++)
                {
                    var loadWrapper = wearables[index];
                    loadWrapper?.Unload();
                }
            }

            if (entity != null)
                entity.OnTransformChange = null;
        }

        void InitMaterials()
        {
            eyeMaterialCopy = new Material(eyeMaterial);
            mouthMaterialCopy = new Material(mouthMaterial);
            eyebrowMaterialCopy = new Material(eyebrowMaterial);
            defaultMaterialCopies = new List<Material>();
        }
        void UnloadMaterials()
        {
            Destroy(eyeMaterialCopy);
            Destroy(mouthMaterialCopy);
            Destroy(eyebrowMaterialCopy);

            if (defaultMaterialCopies != null)
            {
                for (int i = 0; i < defaultMaterialCopies.Count; i++)
                {
                    Destroy(defaultMaterialCopies[i]);
                }

                defaultMaterialCopies.Clear();
            }
        }
        void ResetAvatar()
        {
            minimapRepresentation.SetActive(false);

            UnloadMaterials();
            InitMaterials();

            Destroy(baseBody.gameObject);

            if (wearables != null)
            {
                for (int i = 0; i < wearables.Count; i++)
                {
                    Destroy(wearables[i].gameObject);
                }
            }

            baseBody = null;
            wearables.Clear();

            loadingHasStarted = false;
            loadedWearablesCount = 0;
            baseBodyIsReady = false;
            eyesReady = false;
            browsReady = false;
            mouthReady = false;
            state = State.IDLE;
        }


        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): Horrible fix to the double ApplyChanges call, as its breaking the needed logic.
            if (newJson == "{}")
                yield break;

            if (entity.OnTransformChange == null)
            {
                var movementController = GetComponent<AvatarMovementController>();
                entity.OnTransformChange += movementController.OnTransformChanged;
            }

            Model lastModel = model;
            Model tmpModel = SceneController.i.SafeFromJson<Model>(newJson);

            if (tmpModel.useDummyModel)
            {
                if (randomizer == null)
                {
                    randomizer = new AvatarRandomizer();
                    yield return randomizer.FetchAllAvatarAssets();
                }

                string name = tmpModel.name;

                int seed = 0;

                foreach (char c in name)
                {
                    seed += c;
                }

                model = randomizer.GetRandomModel(seed);
                model.name = name;
                model.useDummyModel = true;
            }
            else
            {
                model = tmpModel;
            }

            if (lastModel.name != model.name)
            {
                avatarName.SetName(model.name);

                //NOTE(Brian): regenerate avatar if name changes
                if (model.useDummyModel && loadingHasStarted)
                {
                    ResetAvatar();
                }
            }

            if (string.IsNullOrEmpty(model.bodyShape.contentName))
            {
                state = State.FAILED;
                yield break;
            }

            if (loadingHasStarted)
            {
                yield break;
            }

            wearables = new List<LoadWrapper>();

            StopCoroutine(SetupEyes());
            StopCoroutine(SetupEyebrows());
            StopCoroutine(SetupMouth());
            StopCoroutine(ShowAll());

            var loader = LoadWearable(model.bodyShape, OnBaseModelSuccess, OnBaseModelFail);

            if (loader == null)
            {
                state = State.FAILED;
                yield break;
            }

            loadingHasStarted = true;

            loader.gameObject.name = "Base Body - " + model.bodyShape.contentName;

            //NOTE(Brian): Model loading can finish instantly, so setting this after LoadWearable can overwrite 
            //             the State.READY value!
            state = State.LOADING;

            for (int i = 0; i < model.wearables.Length; i++)
            {
                loader = LoadWearable(model.wearables[i], OnSuccess, OnFail);

                if (loader == null)
                    continue;

                loader.gameObject.name = "Body Part " + i;
            }

            yield return new WaitUntil(() => state != State.LOADING);

            if (state == State.FAILED)
            {
                Debug.LogError("Avatar loading failed");
                yield break;
            }

            if (VERBOSE) Debug.Log($"{entity.entityId} - Avatar loading success!!");

            SetupAnimator();

            StartCoroutine(SetupEyes());
            StartCoroutine(SetupEyebrows());
            StartCoroutine(SetupMouth());
        }

        IEnumerator SetupEyes()
        {
            if (VERBOSE) Debug.Log($"{entity.entityId} - fetching eyes materials...");

            Texture loadedTexture = null;
            Texture loadedMask = null;

            //NOTE(Brian): Some eyes don't have masks. This is by design.

            yield return AvatarUtils.FetchFaceUrl(model.eyes, model.baseUrl, (tex, mask) => { loadedTexture = tex; loadedMask = mask; });

            AvatarUtils.MapSharedMaterialsRecursively(baseBody.transform,
                (mat) =>
                {
                    eyeMaterialCopy.SetTexture(AvatarUtils._EyesTexture, loadedTexture);
                    eyeMaterialCopy.SetTexture(AvatarUtils._IrisMask, loadedMask);
                    eyeMaterialCopy.SetColor(AvatarUtils._EyeTint, model.eyes.color);

                    return eyeMaterialCopy;
                },
                "eyes");

            eyesReady = true;
        }
        IEnumerator SetupEyebrows()
        {
            if (VERBOSE) Debug.Log($"{entity.entityId} - fetching eyebrows materials...");

            Texture loadedTexture = null;

            yield return AvatarUtils.FetchFaceUrl(model.eyebrows, model.baseUrl, (tex, mask) => loadedTexture = tex);

            AvatarUtils.MapSharedMaterialsRecursively(baseBody.transform,
                (mat) =>
                {
                    eyebrowMaterialCopy.SetTexture(AvatarUtils._BaseMap, loadedTexture);

                    //NOTE(Brian): This isn't an error, we must also apply hair color to this mat
                    eyebrowMaterialCopy.SetColor(AvatarUtils._BaseColor, model.hair.color);

                    return eyebrowMaterialCopy;
                },
                "eyebrows");

            browsReady = true;
        }

        IEnumerator SetupMouth()
        {
            if (VERBOSE) Debug.Log($"{entity.entityId} - fetching mouth materials...");

            Texture loadedTexture = null;

            yield return AvatarUtils.FetchFaceUrl(model.mouth, model.baseUrl, (tex, mask) => loadedTexture = tex);

            AvatarUtils.MapSharedMaterialsRecursively(baseBody.transform,
                (mat) =>
                {
                    mouthMaterialCopy.SetTexture(AvatarUtils._BaseMap, loadedTexture);

                    //NOTE(Brian): This isn't an error, we must also apply skin color to this mat
                    mouthMaterialCopy.SetColor(AvatarUtils._BaseColor, model.skin.color);

                    return mouthMaterialCopy;
                },
                "mouth");

            mouthReady = true;
        }

        LoadWrapper_GLTF LoadWearable(Model.Wearable wearable,
                                System.Action<LoadWrapper> OnSuccess,
                                System.Action<LoadWrapper> OnFail)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.up * 0.75f;

            if (wearable.provider == null)
            {
                wearable.provider = new ContentProvider();
                wearable.provider.baseUrl = model.baseUrl;
                wearable.provider.contents = new List<ContentProvider.MappingPair>(wearable.contents);
                wearable.provider.BakeHashes();
            }

            if (!wearable.provider.HasContentsUrl(wearable.contentName))
            {
#if UNITY_EDITOR
                go.name += "- Failed";
#endif
                return null;
            }

            LoadWrapper_GLTF loadableShape = go.GetOrCreateComponent<LoadWrapper_GLTF>();
            loadableShape.contentProvider = wearable.provider;
            loadableShape.entity = entity;
            loadableShape.initialVisibility = false;
            loadableShape.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;

            loadableShape.Load(wearable.contentName, OnSuccess, OnFail);

            return loadableShape;
        }

        private void OnFail(LoadWrapper loadable)
        {
        }

        private void OnSuccess(LoadWrapper loadable)
        {
            if (loadable == null)
                return;

            loadedWearablesCount++;
            wearables.Add(loadable);

            ApplyDefaultMaterial(loadable.transform);
            CheckAllWearablesAreLoaded();
        }

        private void OnBaseModelSuccess(LoadWrapper loadable)
        {
            if (loadable == null)
                return;

            baseBodyIsReady = true;
            baseBody = loadable;

            ApplyDefaultMaterial(loadable.transform);
            AvatarUtils.RemoveUnusedBodyParts_Hack(baseBody.gameObject);

            CheckAllWearablesAreLoaded();
        }

        private void OnBaseModelFail(LoadWrapper loadable)
        {
            state = State.FAILED;
        }


        void CheckAllWearablesAreLoaded()
        {
            if (VERBOSE) Debug.Log($"{entity.entityId} - CheckAllWearablesLoaded... ready = {baseBodyIsReady} ... {loadedWearablesCount} >= {model.wearables.Length} ... state == {state}");
            if (baseBodyIsReady && loadedWearablesCount >= model.wearables.Length && state == State.LOADING)
            {
                // TODO(Brian): Here we take the animations from the baseBody and put them into each wearable.
                state = State.READY;
                StartCoroutine(ShowAll());
                if (VERBOSE) Debug.Log($"{entity.entityId} - Ready!");
            }
        }

        private void SetupAnimator()
        {
            InstantiatedGLTFObject obj = baseBody.GetComponentInChildren<InstantiatedGLTFObject>();
            Animation animation = obj.gameObject.GetOrCreateComponent<Animation>();

            AvatarAnimatorLegacy animator = GetComponent<AvatarAnimatorLegacy>();
            string bodyName = model.bodyShape.contentName.ToLower();

            AnimationClip[] animArray = null;

            //NOTE(Brian): Please don't alter the following if..else statements, 
            //             as "female" contains "male" and the logic will fail.
            if (bodyName.Contains(MALE_BODY_NAME))
            {
                animArray = maleAnims;
            }
            else if (bodyName.Contains(FEMALE_BODY_NAME))
            {
                animArray = femaleAnims;
            }

            foreach (var clip in animArray)
            {
                if (animation.GetClip(clip.name) == null)
                    animation.AddClip(clip, clip.name);
            }

            animator.target = transform;
            animator.animation = animation;

            //NOTE(Brian): Set bones/rootBone of all wearables to be the same of the baseBody,
            //             so all of them are animated together.
            var mainSkinnedRenderer = baseBody.GetComponentInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < wearables.Count; i++)
            {
                SkinnedMeshRenderer[] skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
                for (int i1 = 0; i1 < skinnedRenderers.Length; i1++)
                {
                    skinnedRenderers[i1].rootBone = mainSkinnedRenderer.rootBone;
                    skinnedRenderers[i1].bones = mainSkinnedRenderer.bones;
                }
            }
        }

        void ApplyDefaultMaterial(Transform root)
        {
            List<Material> copies = AvatarUtils.ReplaceMaterialsWithCopiesOf(root, defaultMaterial);
            defaultMaterialCopies.AddRange(copies);

            AvatarUtils.SetColorInHierarchy(root, MATERIAL_FILTER_SKIN, model.skin.color);
            AvatarUtils.SetColorInHierarchy(root, MATERIAL_FILTER_HAIR, model.hair.color);
        }

        IEnumerator ShowAll()
        {
            yield return new WaitUntil(() => everythingIsLoaded);
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
            }

            minimapRepresentation.SetActive(true);
        } 
    }
}
