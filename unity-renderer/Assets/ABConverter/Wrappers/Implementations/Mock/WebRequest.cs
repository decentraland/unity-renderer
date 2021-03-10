using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine.Networking;

namespace DCL
{
    public sealed partial class Mocked
    {
        public class DownloadHandler : DownloadHandlerScript
        {
            public string mockedText;

            protected override string GetText()
            {
                return mockedText;
            }

            protected override byte[] GetData()
            {
                return Encoding.UTF8.GetBytes(mockedText);
            }
        }

        public class WebRequest : IWebRequest
        {
            //This field maps url to url contents.
            public Dictionary<string, string> mockedContent = new Dictionary<string, string>();
            public float mockedDownloadTime = 0;

            public UnityEngine.Networking.DownloadHandler Get(string url)
            {
                var buffer = new Mocked.DownloadHandler();

                if (!mockedContent.ContainsKey(url))
                    throw new HttpRequestException($"Mocked 404 -- ({url})");

                buffer.mockedText = mockedContent[url];
                return buffer;
            }

            public void GetAsync(string url, Action<UnityEngine.Networking.DownloadHandler> OnCompleted, Action<string> OnFail)
            {
                if (mockedContent.ContainsKey(url))
                {
                    var buffer = new Mocked.DownloadHandler();
                    buffer.mockedText = mockedContent[url];
                    OnCompleted?.Invoke(buffer);
                    return;
                }

                OnFail?.Invoke("Url not found!");
            }
        }
    }
}