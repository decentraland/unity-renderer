using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using TMPro;
using UnityEngine;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class DCLFont : BaseDisposable
    {
        const string RESOURCE_FONT_FOLDER = "Fonts & Materials";

        private const string DEFAULT_SANS_SERIF_HEAVY = "Inter-Heavy SDF";
        private const string DEFAULT_SANS_SERIF_BOLD = "Inter-Bold SDF";
        private const string DEFAULT_SANS_SERIF_SEMIBOLD = "Inter-SemiBold SDF";
        private const string DEFAULT_SANS_SERIF = "Inter-Regular SDF";

        private readonly Dictionary<string, string> fontsMapping = new ()
        {
            { "builtin:SF-UI-Text-Regular SDF", DEFAULT_SANS_SERIF },
            { "builtin:SF-UI-Text-Heavy SDF", DEFAULT_SANS_SERIF_HEAVY },
            { "builtin:SF-UI-Text-Semibold SDF", DEFAULT_SANS_SERIF_SEMIBOLD },
            { "builtin:LiberationSans SDF", "LiberationSans SDF" },
            { "SansSerif", DEFAULT_SANS_SERIF },
            { "SansSerif_Heavy", DEFAULT_SANS_SERIF_HEAVY },
            { "SansSerif_Bold", DEFAULT_SANS_SERIF_BOLD },
            { "SansSerif_SemiBold", DEFAULT_SANS_SERIF_SEMIBOLD },
        };

        [System.Serializable]
        public class Model : BaseModel
        {
            public string src;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.Font)
                    return Utils.SafeUnimplemented<DCLFont, Model>(expected: ComponentBodyPayload.PayloadOneofCase.Font, actual: pbModel.PayloadCase);
                
                var pb = new Model();
                if (pbModel.Font.HasSrc) pb.src = pbModel.Font.Src;
                
                return pb;
            }
        }

        public bool loaded { private set; get; }
        public bool error { private set; get; }

        public TMP_FontAsset fontAsset { private set; get; }

        public DCLFont()
        {
            model = new Model();
        }

        public override int GetClassId() =>
            (int) CLASS_ID.FONT;

        public static bool IsFontLoaded(IParcelScene scene, string componentId)
        {
            if ( string.IsNullOrEmpty(componentId))
                return true;

            if (!scene.componentsManagerLegacy.HasSceneSharedComponent(componentId))
            {
                Debug.Log($"couldn't fetch font, the DCLFont component with id {componentId} doesn't exist");
                return false;
            }

            DCLFont fontComponent = scene.componentsManagerLegacy.GetSceneSharedComponent(componentId) as DCLFont;

            if (fontComponent == null)
            {
                Debug.Log($"couldn't fetch font, the shared component with id {componentId} is NOT a DCLFont");
                return false;
            }

            return true;
        }

        public static IEnumerator WaitUntilFontIsReady(IParcelScene scene, string componentId)
        {
            if ( string.IsNullOrEmpty(componentId))
                yield break;

            DCLFont fontComponent = scene.componentsManagerLegacy.GetSceneSharedComponent(componentId) as DCLFont;

            while (!fontComponent.loaded && !fontComponent.error)
            {
                yield return null;
            }
        }

        public static void SetFontFromComponent(IParcelScene scene, string componentId, TMP_Text text)
        {
            if ( string.IsNullOrEmpty(componentId))
                return;

            DCLFont fontComponent = scene.componentsManagerLegacy.GetSceneSharedComponent(componentId) as DCLFont;

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

            if (fontsMapping.TryGetValue(model.src, out string fontResourceName))
            {
                ResourceRequest request = Resources.LoadAsync($"{RESOURCE_FONT_FOLDER}/{fontResourceName}",
                    typeof(TMP_FontAsset));

                yield return request;

                if (request.asset != null)
                {
                    fontAsset = request.asset as TMP_FontAsset;
                }
                else
                {
                    Debug.Log($"couldn't fetch font from resources {fontResourceName}");
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
