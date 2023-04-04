using System;
using System.Collections;
using UnityEngine.Networking;

namespace DCL
{
    public interface IWebRequestAsyncOperation : IEnumerator, IDisposable
    {
        /// <summary>
        /// Event that will be invoked when the request has been completed.
        /// </summary>
        event Action<IWebRequestAsyncOperation> completed;

        /// <summary>
        /// WebRequest that is being managed.
        /// </summary>
        UnityWebRequest webRequest { get; }

        UnityWebRequestAsyncOperation asyncOp { get; }

        /// <summary>
        /// Returns true after the request has finished communicating with the remote server.
        /// </summary>
        bool isDone { get; }

        /// <summary>
        /// Returns true if the request was successfully finished.
        /// </summary>
        bool isSucceeded { get; }

        /// <summary>
        /// Returns true if webRequest has been disposed (webRequest = null).
        /// </summary>
        bool isDisposed { get; }

        /// <summary>
        /// Set to true for disposing the request just after it has been completed.
        /// </summary>
        bool disposeOnCompleted { get; }

        /// <summary>
        /// Returns the data that has been downloaded as a byte array, if not done or success it will return an empty array
        /// </summary>
        byte[] GetResultData();

        /// <summary>
        /// If in progress, halts the request as soon as possible.
        /// </summary>
        void Abort();

        /// <summary>
        /// Mark the request as completed and throw the corresponding event.
        /// </summary>
        /// <param name="success">True if the request was successfully ended.</param>
        void SetAsCompleted(bool success);
    }
}
