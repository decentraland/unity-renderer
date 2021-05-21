using UnityEngine;

namespace DCL
{
    public interface IPoolableObject
    {
        bool isInsidePool { get; }
        GameObject gameObject { get; }
        void Release();
        void RemoveFromPool();

        event System.Action OnGet;
        event System.Action OnRelease;
    }
}