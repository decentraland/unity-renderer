using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public delegate IService ServiceBuilder();

    public class ServiceLocator : IDisposable
    {
        private Dictionary<Type, IService> services = new Dictionary<Type, IService>();
        private Dictionary<Type, ServiceBuilder> serviceBuilders = new Dictionary<Type, ServiceBuilder>();

        public void Unregister<T>(ServiceBuilder data) where T : IService
        {
            Type type = typeof(T);
            Assert.IsTrue( type.IsInterface, "ServiceLocator's generic type should be an interface." );

            if (!serviceBuilders.ContainsKey(type))
            {
                Debug.LogWarning($"Trying to unregister non-existent type! type: {type.FullName}");
                return;
            }

            serviceBuilders.Remove(type);
        }

        public ServiceBuilder Register<T>(ServiceBuilder data) where T : IService
        {
            Type type = typeof(T);
            Assert.IsTrue( type.IsInterface, "ServiceLocator's generic type should be an interface." );

            if (serviceBuilders.ContainsKey(type))
            {
                serviceBuilders[type] = data;
                return data;
            }

            serviceBuilders.Add(type, data);
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
            Dispose();

            services.Clear();

            foreach ( var builder in serviceBuilders )
            {
                services.Add( builder.Key, builder.Value.Invoke() );
            }

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