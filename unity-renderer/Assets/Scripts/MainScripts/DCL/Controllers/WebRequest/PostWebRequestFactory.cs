using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace DCL
{
    public class PostWebRequestFactory : IPostWebRequestFactory
    {
        private string postData;
        private List<IMultipartFormSection> formData;

        private bool isMultipart;

        public void SetBody(string data)
        {
            this.postData = data;
            isMultipart = false;
        }

        public void SetBody(List<IMultipartFormSection> data)
        {
            this.formData = data;
            isMultipart = true;
        }

        public UnityWebRequest CreateWebRequest(string url)
        {
            UnityWebRequest unityWebRequest;

            if (isMultipart)
                unityWebRequest = UnityWebRequest.Post(url, formData);
            else
            {
                unityWebRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
                unityWebRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postData));
            }

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            return unityWebRequest;
        }
    }
}
