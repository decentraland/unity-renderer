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

        public TextMeshProUGUI text;

        public AnimationClip[] maleAnims;
        public AnimationClip[] femaleAnims;

        int loadedWearablesCount = 0;

        State state = State.IDLE;
        bool loadingHasStarted = false;
        bool baseBodyIsReady = false;

        public Model model = new Model();

        [NonSerialized] public GameObject baseBody;
        [NonSerialized] public List<GameObject> wearables;
        public bool everythingIsLoaded { get { return state == State.READY; } }

        Material eyeMaterialCopy;
        Material eyebrowMaterialCopy;
        Material mouthMaterialCopy;
        List<Material> defaultMaterialCopies;

        Vector3 sourcePosition;
        Vector3 sourceScale;
        Quaternion sourceRotation;

        Vector3 targetPosition;
        Vector3 targetScale;
        Quaternion targetRotation;

        float currentMovementTime = 0;
        float endMovementTime = 0;

        float currentTime = 0;

        AvatarRandomizer randomizer;

        void Start()
        {
            eyeMaterialCopy = new Material(eyeMaterial);
            mouthMaterialCopy = new Material(mouthMaterial);
            eyebrowMaterialCopy = new Material(eyebrowMaterial);
            defaultMaterialCopies = new List<Material>();
        }

        void OnDestroy()
        {
            Destroy(eyeMaterialCopy);
            Destroy(mouthMaterialCopy);
            Destroy(eyebrowMaterialCopy);

            for (int i = 0; i < defaultMaterialCopies.Count; i++)
            {
                Material mat = defaultMaterialCopies[i];
                Destroy(mat);
            }
        }


        public override IEnumerator ApplyChanges(string newJson)
        {
            //NOTE(Brian): Horrible fix to the double ApplyChanges call, as its breaking the needed logic.
            if (newJson == "{}")
                yield break;

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
            }
            else
            {
                model = tmpModel;
            }

            text.text = model.name;

            LayoutGroup[] groups = text.transform.GetComponentsInParent<LayoutGroup>();

            for (int i = 0; i < groups.Length; i++)
            {
                LayoutGroup group = groups[i];
                LayoutRebuilder.ForceRebuildLayoutImmediate(group.transform as RectTransform);
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

            wearables = new List<GameObject>();

            var loader = LoadWearable(model.bodyShape, OnBaseModelSuccess, OnBaseModelFail);

            if (loader == null)
            {
                state = State.FAILED;
                yield break;
            }

            loadingHasStarted = true;

            loader.gameObject.name = "Base Body - " + model.bodyShape.contentName;

            for (int i = 0; i < model.wearables.Length; i++)
            {
                loader = LoadWearable(model.wearables[i], OnSuccess, OnFail);

                if (loader == null)
                    continue;

                loader.gameObject.name = "Body Part " + i;
            }

            state = State.LOADING;

            yield return new WaitUntil(() => state != State.LOADING);

            if (state == State.FAILED)
            {
                Debug.LogError("Avatar loading failed");
                yield break;
            }

            if (VERBOSE) Debug.Log("Avatar loading success!!");

            SetupAnimator();

            SceneController.i.StartCoroutine(SetupEyes());
            SceneController.i.StartCoroutine(SetupEyebrows());
            SceneController.i.StartCoroutine(SetupMouth());
        }

        IEnumerator SetupEyes()
        {
            if (VERBOSE) Debug.Log("fetching eyes materials...");

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
        }
        IEnumerator SetupEyebrows()
        {
            if (VERBOSE) Debug.Log("fetching eyebrows materials...");

            Texture loadedTexture = null;

            yield return AvatarUtils.FetchFaceUrl(model.eyebrows, model.baseUrl, (tex, mask) => loadedTexture = tex);

            AvatarUtils.MapSharedMaterialsRecursively(baseBody.transform,
                (mat) =>
                {
                    eyebrowMaterialCopy.SetTexture(AvatarUtils._MainTex, loadedTexture);

                    //NOTE(Brian): This isn't an error, we must also apply hair color to this mat
                    eyebrowMaterialCopy.SetColor(AvatarUtils._Color, model.hair.color);

                    return eyebrowMaterialCopy;
                },
                "eyebrows");

        }

        IEnumerator SetupMouth()
        {
            if (VERBOSE) Debug.Log("fetching mouth materials...");

            Texture loadedTexture = null;

            yield return AvatarUtils.FetchFaceUrl(model.mouth, model.baseUrl, (tex, mask) => loadedTexture = tex);

            AvatarUtils.MapSharedMaterialsRecursively(baseBody.transform,
                (mat) =>
                {
                    mouthMaterialCopy.SetTexture(AvatarUtils._MainTex, loadedTexture);

                    //NOTE(Brian): This isn't an error, we must also apply skin color to this mat
                    mouthMaterialCopy.SetColor(AvatarUtils._Color, model.skin.color);

                    return mouthMaterialCopy;
                },
                "mouth");
        }

        GLTFLoader LoadWearable(Model.Wearable wearable,
                                System.Action<LoadableMonoBehavior> OnSuccess,
                                System.Action<LoadableMonoBehavior> OnFail)
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

            GLTFLoader loadableShape = go.GetOrCreateComponent<GLTFLoader>();
            loadableShape.contentProvider = wearable.provider;
            loadableShape.entity = entity;
            loadableShape.initialVisibility = true;
            loadableShape.useVisualFeedback = Configuration.ParcelSettings.VISUAL_LOADING_ENABLED;

            loadableShape.Load(wearable.contentName, OnSuccess, OnFail);

            return loadableShape;
        }

        private void OnFail(LoadableMonoBehavior loadable)
        {
        }

        private void OnSuccess(LoadableMonoBehavior loadable)
        {
            if (loadable == null)
                return;

            loadedWearablesCount++;
            wearables.Add(loadable.gameObject);

            ApplyDefaultMaterial(loadable.transform);
            CheckAllWearablesAreLoaded();
        }

        private void OnBaseModelSuccess(LoadableMonoBehavior loadable)
        {
            if (loadable == null)
                return;

            baseBodyIsReady = true;
            baseBody = loadable.gameObject;

            ApplyDefaultMaterial(loadable.transform);
            AvatarUtils.RemoveUnusedBodyParts_Hack(baseBody);

            CheckAllWearablesAreLoaded();
        }

        private void OnBaseModelFail(LoadableMonoBehavior loadable)
        {
            state = State.FAILED;
        }


        void CheckAllWearablesAreLoaded()
        {
            if (VERBOSE) Debug.Log($"CheckAllWearablesLoaded... ready = {baseBodyIsReady} ... {loadedWearablesCount} >= {model.wearables.Length} ... state == {state}");
            if (baseBodyIsReady && loadedWearablesCount >= model.wearables.Length && state == State.LOADING)
            {
                // TODO(Brian): Here we take the animations from the baseBody and put them into each wearable.
                state = State.READY;
                if (VERBOSE) Debug.Log("ready!");
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

        #region LerpMethods

        public void MoveWithLerpTo(Vector3 pos, Quaternion rotation, Vector3 scale)
        {
            if (entity.gameObject.transform.position == Vector3.zero)
            {
                entity.gameObject.transform.position = pos;
                entity.gameObject.transform.rotation = rotation;
                entity.gameObject.transform.localScale = scale;
            }
            else
            {
                sourcePosition = entity.gameObject.transform.position;
                sourceRotation = entity.gameObject.transform.rotation;

                targetPosition = pos;
                targetRotation = rotation;
                entity.gameObject.transform.localScale = scale;

                this.currentTime = 0;
                this.currentMovementTime = 0;

                float distance = (targetPosition - sourcePosition).magnitude;

                this.endMovementTime = distance / MOVEMENT_SPEED;
            }
        }


        void UpdateLerp(float dt)
        {
            if (this.currentMovementTime < this.endMovementTime)
            {
                this.currentMovementTime += dt;
                this.currentMovementTime = Mathf.Clamp(this.currentMovementTime, 0, this.endMovementTime);

                float d = this.currentMovementTime / this.endMovementTime;

                entity.gameObject.transform.position = Vector3.Lerp(this.sourcePosition, this.targetPosition, d);
            }

            if (this.currentTime < ROTATION_SCALE_TIME)
            {
                this.currentTime += dt;
                this.currentTime = Mathf.Clamp(this.currentTime, 0, ROTATION_SCALE_TIME);

                float d = this.currentTime / ROTATION_SCALE_TIME;

                entity.gameObject.transform.rotation = Quaternion.Slerp(this.sourceRotation, this.targetRotation, d);
            }
        }

        void Update()
        {
            UpdateLerp(Time.deltaTime);
        }

        #endregion LerpMethods

    }
}
