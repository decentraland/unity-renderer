﻿using DCL;
using UnityEngine.Networking;

namespace MainScripts.DCL.Helpers.SentryUtils
{
    public interface IWebRequestMonitor : IService
    {
        /// <summary>
        /// Tracks the web request and report its status to the underlying analytics
        /// </summary>
        /// <param name="webRequestOp">Web Requests' Async Op</param>
        /// <param name="endPointTemplate">Effective end point without the domain name</param>
        /// <param name="queryString">The query string component of the URL (e.g. query=foobar&page=2)</param>
        /// <param name="data">Submitted data in JSON, if omitted raw data is taken from `WebRequest`</param>
        void TrackWebRequest(UnityWebRequestAsyncOperation webRequestOp, string endPointTemplate, string queryString = null, string data = null);
    }
}
