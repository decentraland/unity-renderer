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
            Type type = typeof(T);
            Assert.IsTrue( type.IsInterface, "ServiceLocator's generic type should be an interface." );

            if (services.ContainsKey(type))
            {
                Debug.Log($"Overwriting service for {type.FullName}");
                services[type].Dispose();
                services[type] = data;
                return data;
            }

            services.Add(type, data);
            return data;
        }

        public T Get<T>() where T : class, IService
        {
            Type type = typeof(T);
            Assert.IsTrue( type.IsInterface, "ServiceLocator's generic type should be an interface." );

            if (!services.ContainsKey(type))
            {
                Debug.LogWarning($"Not registered! use Set<T> to set the type first. type: {type.FullName}");
                return null;
            }

            return services[type] as T;
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