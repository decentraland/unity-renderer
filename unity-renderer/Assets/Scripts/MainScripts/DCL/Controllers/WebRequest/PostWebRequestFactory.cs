using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class PostWebRequestFactory : IPostWebRequestFactory
    {
        private byte[] postData;
        private string contentType;

        public void SetBody(string data)
        {
            this.postData = Encoding.UTF8.GetBytes(data);
            this.contentType = "application/x-www-form-urlencoded";
        }

        public void SetBody(List<IMultipartFormSection> data)
        {
            WWWForm form = new WWWForm();
            foreach (var section in data)
                form.AddBinaryData(section.sectionName, section.sectionData, section.fileName, section.contentType);

            this.postData = form.data;
            this.contentType = "multipart/form-data";
        }

        public UnityWebRequest CreateWebRequest(string url)
        {
            UnityWebRequest unityWebRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            unityWebRequest.uploadHandler = new UploadHandlerRaw(postData);
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            unityWebRequest.SetRequestHeader("Content-Type", contentType);

            return unityWebRequest;
        }
    }
}
