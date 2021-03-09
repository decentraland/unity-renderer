using UnityEngine;

namespace DCL
{
    public class ContentProvider_Dummy : ContentProvider
    {
        public override bool HasContentsUrl(string url)
        {
            return !string.IsNullOrEmpty(url);
        }

        public override bool TryGetContentsUrl(string url, out string result)
        {
            result = url;

            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(baseUrl))
                result = baseUrl + "/" + url;
            else
                result = url;

            if (VERBOSE)
            {
                Debug.Log($">>> GetContentsURL from ... {url} ... RESULTING URL... = {result}");
            }

            return true;
        }
    }
}
