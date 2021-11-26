using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class ServiceLocator : IDisposable
    {
        private Dictionary<Type, IDisposable> services = new Dictionary<Type, IDisposable>();

        public void Set<T>(T data) where T : IDisposable
        {
            Assert.IsTrue( typeof(T).IsInterface, "ServiceLocator's generic type should be an interface." );

            if (services.ContainsKey(typeof(T)))
            {
                Debug.Log($"Overwriting service for {typeof(T).FullName}");
                services[typeof(T)].Dispose();
                services[typeof(T)] = data;
                return;
            }

            services.Add(typeof(T), data);
        }

        public T Get<T>() where T : class, IDisposable
        {
            Assert.IsTrue( typeof(T).IsInterface, "ServiceLocator's generic type should be an interface." );
            if (!services.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"Not registered! use Set<T> to set the type first. type: {typeof(T).FullName}");
                return null;
            }


            return services[typeof(T)] as T;
        }

        public void Dispose()
        {
            foreach ( var service in services )
            {
                service.Value.Dispose();
            }
        }
    }
}