using System.Collections;
using DCL.Controllers;
using TMPro;
using UnityEngine;

namespace DCL.Components
{
    public class DCLFont : BaseDisposable
    {
        const string RESOURCE_FONT_PREFIX = "builtin:";
        const string RESOURCE_FONT_FOLDER = "Fonts & Materials";

        [System.Serializable]
        public class Model
        {
            public string src;
        }

        public Model model;

        public bool loaded { private set; get; } = false;
        public bool error { private set; get; } = false;

        public TMP_FontAsset fontAsset { private set; get; }

        public DCLFont(DCL.Controllers.ParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public static IEnumerator SetFontFromComponent(ParcelScene scene, string componentId, TMP_Text text)
        {
            if (!scene.disposableComponents.ContainsKey(componentId))
            {
                Debug.Log($"couldn't fetch font, the DCLFont component with id {componentId} doesn't exist");
                yield break;
            }

            DCLFont fontComponent = scene.disposableComponents[componentId] as DCLFont;
            if (fontComponent == null)
            {
                Debug.Log($"couldn't fetch font, the shared component with id {componentId} is NOT a DCLFont");
                yield break;
            }

            while (!fontComponent.loaded && !fontComponent.error)
            {
                yield return null;
            }

            if (!fontComponent.error)
            {
                text.font = fontComponent.fontAsset;
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = SceneController.i.SafeFromJson<Model>(newJson);

            if (string.IsNullOrEmpty(model.src))
            {
                error = true;
                yield break;
            }

            if (model.src.StartsWith(RESOURCE_FONT_PREFIX))
            {
                string resourceName = model.src.Substring(RESOURCE_FONT_PREFIX.Length);

                ResourceRequest request = Resources.LoadAsync(string.Format("{0}/{1}", RESOURCE_FONT_FOLDER, resourceName), typeof(TMP_FontAsset));
                yield return request;

                if (request.asset != null)
                {
                    fontAsset = request.asset as TMP_FontAsset;
                }
                else
                {
                    Debug.Log($"couldn't fetch font from resources {resourceName}");
                }
                loaded = true;
                error = fontAsset == null;
            }
            else
            {
                // NOTE: only support fonts in resources
                error = true;
            }
        }

        public override void Dispose()
        {
            if (fontAsset != null)
                Object.Destroy(fontAsset);
            base.Dispose();
        }
    }
}