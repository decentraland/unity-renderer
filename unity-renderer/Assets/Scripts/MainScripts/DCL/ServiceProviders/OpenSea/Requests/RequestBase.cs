using System;

namespace DCL.Helpers.NFT.Markets.OpenSea_Internal
{
    public abstract class RequestBase<T>
    {
        public event Action<RequestBase<T>> OnSuccess;
        public event Action<RequestBase<T>> OnFail;

        public bool resolved { private set; get; }  
        public bool rejected { private set; get; }
        public bool pending => !rejected && !resolved;
        public string error { private set; get; }
        public abstract string requestId { get; }

        public T resolvedValue { private set; get; }

        public void Resolve(T response)
        {
            resolvedValue = response;
            resolved = true;
            OnSuccess?.Invoke(this);
        }

        public void Reject(string error)
        {
            this.error = error;
            rejected = true;
            OnFail?.Invoke(this);
        }
    }
}