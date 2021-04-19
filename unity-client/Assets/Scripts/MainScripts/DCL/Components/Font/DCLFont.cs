using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using TMPro;
using UnityEngine;

namespace DCL.Components
{
    public class DCLFont : BaseDisposable
    {
        const string RESOURCE_FONT_PREFIX = "builtin:";
        const string RESOURCE_FONT_FOLDER = "Fonts & Materials";

        [System.Serializable]
        public class Model : BaseModel
        {
            public string src;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        public bool loaded { private set; get; } = false;
        public bool error { private set; get; } = false;

        public TMP_FontAsset fontAsset { private set; get; }

        public DCLFont()
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.FONT;
        }

        public static IEnumerator SetFontFromComponent(IParcelScene scene, string componentId, TMP_Text text)
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

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            Model model = (Model) newModel;

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
    }
}