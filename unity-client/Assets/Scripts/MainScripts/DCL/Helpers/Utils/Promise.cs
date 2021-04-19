using System;
using UnityEngine;

namespace DCL.Helpers
{
    public class Promise<T> : CustomYieldInstruction, IDisposable
    {
        private bool resolved = false;
        private bool failed = false;

        private Action<T> onSuccess;
        private Action<string> onError;

        public override bool keepWaiting => !resolved;
        public T value { private set; get; }
        public string error { private set; get; }

        public void Resolve(T result)
        {
            failed = false;
            resolved = true;
            value = result;
            error = null;
            onSuccess?.Invoke(result);
        }

        public void Reject(string errorMessage)
        {
            failed = true;
            resolved = true;
            error = errorMessage;
            onError?.Invoke(error);
        }

        public Promise<T> Then(Action<T> successCallback)
        {
            if (!resolved)
            {
                onSuccess = successCallback;
            }
            else if (!failed)
            {
                successCallback?.Invoke(value);
            }

            return this;
        }

        public void Catch(Action<string> errorCallback)
        {
            if (!resolved)
            {
                onError = errorCallback;
            }
            else if (failed)
            {
                errorCallback?.Invoke(this.error);
            }
        }

        public void Dispose()
        {
            onSuccess = null;
            onError = null;
            resolved = true;
        }
    }
}