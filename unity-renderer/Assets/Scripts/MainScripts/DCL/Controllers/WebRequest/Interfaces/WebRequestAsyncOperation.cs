using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public interface IWebRequestAsyncOperation
    {
        /// <summary>
        /// Event that will be invoked when the request has been completed.
        /// </summary>
        event Action<WebRequestAsyncOperation> completed;

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
        bool isSucceded { get; }
        
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
        /// Signals that this request is no longer being used, and should clean up any resources it is using (it aborts the request before disposing).
        /// </summary>
        void Dispose();

        /// <summary>
        /// Mark the request as completed and throw the corresponding event.
        /// </summary>
        /// <param name="success">True if the request was successfully ended.</param>
        void SetAsCompleted(bool success);
    }
    
    /// <summary>
    /// Our custom request async operation to be used with the WebRequestController.
    /// </summary>
    public class WebRequestAsyncOperation : CustomYieldInstruction, IWebRequestAsyncOperation
    {
        /// <summary>
        /// Event that will be invoked when the request has been completed.
        /// </summary>
        public event Action<WebRequestAsyncOperation> completed;

        /// <summary>
        /// WebRequest that is being managed.
        /// </summary>
        public UnityWebRequest webRequest { get; private set; }
        
        public UnityWebRequestAsyncOperation asyncOp { get; private set; }

        /// <summary>
        /// Returns true after the request has finished communicating with the remote server.
        /// </summary>
        public bool isDone { get; private set; }

        /// <summary>
        /// Returns true if the request was successfully finished.
        /// </summary>
        public bool isSucceded { get; private set; }

        /// <summary>
        /// Returns true if webRequest has been disposed (webRequest = null).
        /// </summary>
        public bool isDisposed { get { return webRequest == null; } }

        /// <summary>
        /// Set to true for disposing the request just after it has been completed.
        /// </summary>
        public bool disposeOnCompleted { get; set; }

        public override bool keepWaiting { get { return !isDone; } }

        public WebRequestAsyncOperation(UnityWebRequest webRequest)
        {
            this.webRequest = webRequest;
            isDone = false;
            isSucceded = false;
            disposeOnCompleted = true;
        }

        /// <summary>
        /// Mark the request as completed and throw the corresponding event.
        /// </summary>
        /// <param name="success">True if the request was successfully ended.</param>
        public void SetAsCompleted(bool success)
        {
            completed?.Invoke(this);
            isDone = true;
            isSucceded = success;

            if (disposeOnCompleted)
                Dispose();
        }

        /// <summary>
        /// If in progress, halts the request as soon as possible.
        /// </summary>
        public void Abort()
        {
            if (webRequest == null || isDone)
                return;

            webRequest.Abort();
        }

        /// <summary>
        /// Signals that this request is no longer being used, and should clean up any resources it is using (it aborts the request before disposing).
        /// </summary>
        public void Dispose()
        {
            Abort();

            if (webRequest == null)
                return;

            webRequest.Dispose();
            webRequest = null;
        }
        
        /// <summary>
        /// Returns the data that has been downloaded as a byte array, if not done or success it will return an empty array
        /// </summary>
        public byte[] GetResultData()
        {
            return webRequest.downloadHandler.data;
        }

        internal void SetNewWebRequest(UnityWebRequest newRequest) { webRequest = newRequest; }
        public UnityWebRequestAsyncOperation SendWebRequest()
        {
            asyncOp = webRequest.SendWebRequest();
            return asyncOp;
        }
    }
}