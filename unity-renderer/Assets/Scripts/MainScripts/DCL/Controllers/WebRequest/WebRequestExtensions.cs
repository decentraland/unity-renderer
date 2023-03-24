using System;
using UnityEngine.Networking;

namespace DCL
{
    public static class WebRequestExtensions
    {
        public static bool WebRequestSucceded(this UnityWebRequest request)
        {
            return request != null &&
                   request.result == UnityWebRequest.Result.Success;
        }

        public static bool WebRequestServerError(this UnityWebRequest request)
        {
            return request != null &&
                   request.responseCode >= 500 &&
                   request.responseCode < 600;
        }

        public static bool WebRequestTimedOut(this UnityWebRequest request) =>
            request != null &&
            request.error == "Request timeout";

        public static bool WebRequestAborted(this UnityWebRequest request)
        {
            return request != null &&
                   request.result == UnityWebRequest.Result.ConnectionError &&
                   request.result == UnityWebRequest.Result.ProtocolError &&
                   !string.IsNullOrEmpty(request.error) &&
                   request.error.Contains("aborted", StringComparison.OrdinalIgnoreCase);
        }
    }
}
