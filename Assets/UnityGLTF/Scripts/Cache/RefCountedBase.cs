using System;

namespace UnityGLTF.Cache
{
    public abstract class RefCountedBase
    {
        private bool _isDisposed = false;

        private int _refCount = 0;
        private readonly object _refCountLock = new object();

        public void IncreaseRefCount()
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException("Cannot inscrease the ref count on disposed cache data.");
            }

            lock (_refCountLock)
            {
                _refCount++;
            }

            OnIncreaseRefCount();
        }

        public void DecreaseRefCount()
        {
            if (_isDisposed)
            {
                throw new InvalidOperationException("Cannot decrease the ref count on disposed cache data.");
            }

            lock (_refCountLock)
            {
                if (_refCount <= 0)
                {
                    throw new InvalidOperationException("Cannot decrease the cache data ref count below zero.");
                }

                _refCount--;
            }

            OnDecreaseRefCount();

            if (_refCount <= 0)
            {
                DestroyCachedData();
            }
        }

        private void DestroyCachedData()
        {
            if (!_isDisposed)
            {
                OnDestroyCachedData();
            }

            _isDisposed = true;
        }

        protected abstract void OnDestroyCachedData();

        protected virtual void OnIncreaseRefCount()
        {
        }
        protected virtual void OnDecreaseRefCount()
        {
        }
    }
}
