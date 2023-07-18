using System.Collections.Generic;
using UnityEngine.Networking;

namespace DCL
{
    public interface IPostWebRequestFactory : IWebRequestFactory
    {
        void SetBody(string postData);

        public void SetBody(List<IMultipartFormSection> data);
    }
}
