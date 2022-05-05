using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using DCL;

namespace DCL.Components
{
    public class DCLAvatarTexture : DCLTexture
    {

        [System.Serializable]
        public class ProfileRequestData
        {
            [System.Serializable]
            public class Avatars
            {
                public Avatar avatar;
            }

            [System.Serializable]
            public class Avatar
            {
                public Snapshots snapshots;
            }

            [System.Serializable]
            public class Snapshots
            {
                public string face256;
                public string body;
            }

            public Avatars[] avatars;
        }

        [System.Serializable]
        public class AvatarModel : DCLTexture.Model
        {
            public string userId;
            public override BaseModel GetDataFromJSON(string json) { return Utils.SafeFromJson<AvatarModel>(json); }
        }

        public DCLAvatarTexture() { 
            model = new AvatarModel(); 
        }
        
        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());
            
            //If the scene creates and destroy the component before our renderer has been turned on bad things happen!
            //TODO: Analyze if we can catch this upstream and stop the IEnumerator
            if (isDisposed)
                yield break;
            
            AvatarModel model = (AvatarModel) newModel;

            if (texture == null && !string.IsNullOrEmpty(model.userId))
            {
                string textureUrl = string.Empty;
                string sourceUrl = Environment.i.platform.serviceProviders.catalyst.lambdasUrl + "/profiles?id=" + model.userId;

                // The sourceUrl request should return an array, with an object
                //      the object has `timerstamp` and `avatars`, `avatars` is an array
                //      we only request a single avatar so with length=1
                //      avatars[0] has the avatar and we have to access to
                //      avatars[0].avatar.snapshots, and the links are
                //      face,face128,face256 and body
                
                // TODO: check if this user data already exists to avoid this fetch.
                yield return GetAvatarUrls(sourceUrl, (faceUrl) =>
                {
                    textureUrl = faceUrl;
                });
                
                if (!string.IsNullOrEmpty(textureUrl))
                {
                    model.src = textureUrl;
                    yield return base.ApplyChanges(model);
                }
            }
        }

        private static IEnumerator GetAvatarUrls(string url, Action<string> onURLSuccess)
        {
            yield return Environment.i.platform.webRequest.Get(
                url: url,
                downloadHandler: new DownloadHandlerBuffer(),
                timeout: 10,
                disposeOnCompleted: false,
                OnFail: (webRequest) =>
                {
                    Debug.LogWarning($"Request error! profile data couldn't be fetched! -- {webRequest.webRequest.error}");
                },
                OnSuccess: (webRequest) =>
                {
                    ProfileRequestData[] data = DCL.Helpers.Utils.ParseJsonArray<ProfileRequestData[]>(webRequest.webRequest.downloadHandler.text);
                    string face256Url = data[0]?.avatars[0]?.avatar.snapshots.face256;
                    onURLSuccess?.Invoke(face256Url);
                });
        }
    }
}