using System.Collections;
using System.Linq;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;

namespace DCL.Components
{
    public class DCLVideoClip : BaseDisposable
    {
        [System.Serializable]
        public class Model
        {
            public string url;
        }

        public Model model;
        public bool isExternalURL { get; private set; }
        public bool isStream { get; private set; }

        public DCLVideoClip(IParcelScene scene) : base(scene)
        {
            model = new Model();
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.VIDEO_CLIP;
        }

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            model = Utils.SafeFromJson<Model>(newJson);
            isExternalURL = model.url.StartsWith("http://") || model.url.StartsWith("https://");
            isStream = !new[] {".mp4", ".ogg", ".mov", ".webm"}.Any(x => model.url.EndsWith(x));
            yield break;
        }

        public string GetUrl()
        {
            string contentsUrl = model.url;

            if (!isExternalURL)
            {
                scene.contentProvider.TryGetContentsUrl(model.url, out contentsUrl);
            }

            return contentsUrl;
        }
    }
}