using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class ServiceLocator : IDisposable
    {
        private Dictionary<Type, IService> services = new Dictionary<Type, IService>();

        public T Set<T>(T data) where T : IService
        {
            Assert.IsTrue( typeof(T).IsInterface, "ServiceLocator's generic type should be an interface." );

            if (services.ContainsKey(typeof(T)))
            {
                Debug.Log($"Overwriting service for {typeof(T).FullName}");
                services[typeof(T)].Dispose();
                services[typeof(T)] = data;
                return data;
            }

            services.Add(typeof(T), data);
            return data;
        }

        public T Get<T>() where T : class, IService
        {
            Assert.IsTrue( typeof(T).IsInterface, "ServiceLocator's generic type should be an interface." );

            if (!services.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"Not registered! use Set<T> to set the type first. type: {typeof(T).FullName}");
                return null;
            }

            return services[typeof(T)] as T;
        }

        public void Initialize()
        {
            foreach ( var service in services )
            {
                service.Value.Initialize();
            }
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