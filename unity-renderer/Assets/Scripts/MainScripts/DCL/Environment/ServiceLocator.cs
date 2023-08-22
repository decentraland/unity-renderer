using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public delegate IService ServiceBuilder();

    public class ServiceLocator : IDisposable
    {
        private static bool VERBOSE = false;
        private static Logger logger = new Logger("ServiceLocator") { verboseEnabled = VERBOSE };

        private Dictionary<Type, IService> services = new Dictionary<Type, IService>();
        private Dictionary<Type, ServiceBuilder> serviceBuilders = new Dictionary<Type, ServiceBuilder>();

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

        public void Unregister<T>() where T : IService
        {
            Type type = typeof(T);
            Assert.IsTrue( type.IsInterface, "ServiceLocator's generic type should be an interface." );

            if (!serviceBuilders.ContainsKey(type))
            {
                logger.Warning($"Trying to unregister non-existent type! type: {type.FullName}");
                return;
            }

            serviceBuilders.Remove(type);
        }

        public T Get<T>() where T : class
        {
            Type type = typeof(T);
            Assert.IsTrue( type.IsInterface, "ServiceLocator's generic type should be an interface." );

            if (!serviceBuilders.ContainsKey(type))
            {
                logger.Verbose($"Not registered! use Register<T> to set the type builder first and then Initialize() to create it. type: {type.FullName}");
                return null;
            }

            if (!services.ContainsKey(type))
            {
                logger.Warning($"Not initialized! use Initialize() to create this service. type: {type.FullName}");
                return null;
            }

            return services[type] as T;
        }

        public async UniTask Initialize(CancellationToken cancellationToken = default)
        {
            foreach ( var service in services )
            {
                service.Value.Dispose();
            }

            services.Clear();

            foreach ( var kvp in serviceBuilders )
            {
                var builderType = kvp.Key;
                var builder = kvp.Value;
                services.Add( builderType, builder.Invoke() );
            }

            foreach ( var service in services )
            {
                service.Value.Initialize();
            }

            await UniTask.WhenAll(services.Select(s => s.Value.InitializeAsync(cancellationToken))).SuppressCancellationThrow();
        }

        public void Dispose()
        {
            foreach ( var service in services )
            {
                service.Value.Dispose();
            }

            services.Clear();
            serviceBuilders.Clear();
        }
    }
}
