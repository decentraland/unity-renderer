using System;
using System.Collections;
using System.Linq;
using DCL.Helpers;
using DCL.Models;
using Decentraland.Sdk.Ecs6;

namespace DCL.Components
{
    public class DCLVideoClip : BaseDisposable
    {
        private static readonly string[] NO_STREAM_EXTENSIONS = { ".mp4", ".ogg", ".mov", ".webm" };

        [Serializable]
        public class Model : BaseModel
        {
            public string url;

            public override BaseModel GetDataFromJSON(string json) =>
                Utils.SafeFromJson<Model>(json);

            public override BaseModel GetDataFromPb(ComponentBodyPayload pbModel)
            {
                if (pbModel.PayloadCase != ComponentBodyPayload.PayloadOneofCase.VideoClip)
                    return Utils.SafeUnimplemented<DCLVideoClip, Model>(expected: ComponentBodyPayload.PayloadOneofCase.VideoClip, actual: pbModel.PayloadCase);

                var pb = new Model();
                if (pbModel.VideoClip.HasUrl) pb.url = pbModel.VideoClip.Url;
                
                return pb;
            }
        }

        public bool isExternalURL { get; private set; }
        public bool isStream { get; private set; }

        public DCLVideoClip()
        {
            model = new Model();
        }

        public override int GetClassId() =>
            (int) CLASS_ID.VIDEO_CLIP;

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            Model model = (Model) newModel;
            isExternalURL = model.url.StartsWith("http://") || model.url.StartsWith("https://");

            string urlPath = model.url;
            if (Uri.TryCreate(urlPath, UriKind.Absolute, out Uri uri))
            {
                urlPath = uri.AbsolutePath;
            }
            isStream = !NO_STREAM_EXTENSIONS.Any(x => urlPath.EndsWith(x));
            yield break;
        }

        public string GetUrl()
        {
            Model model = (Model) this.model;

            string contentsUrl = model.url;

            if (!isExternalURL)
            {
                scene.contentProvider.TryGetContentsUrl(model.url, out contentsUrl);
            }

            return contentsUrl;
        }
    }
}
