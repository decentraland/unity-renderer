﻿using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace DCL
{
    public class Environment
    {
        public static Model i = new Model(new ServiceLocator());

        private static CancellationTokenSource cancellationTokenSource;

        /// <summary>
        /// Setup the environment with the configured builder funcs.
        /// </summary>
        public static void Setup(ServiceLocator serviceLocator)
        {
            i.Dispose();
            i = new Model(serviceLocator);
            cancellationTokenSource = new CancellationTokenSource();
            serviceLocator.Initialize(cancellationTokenSource.Token).Forget();
        }

        public static UniTask SetupAsync(ServiceLocator serviceLocator)
        {
            i.Dispose();
            i = new Model(serviceLocator);
            cancellationTokenSource = new CancellationTokenSource();
            return serviceLocator.Initialize(cancellationTokenSource.Token);
        }

        /// <summary>
        /// Dispose() all the current environment systems.
        /// </summary>
        public static void Dispose()
        {
            i?.Dispose();
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
        }

        public class Model : IDisposable
        {
            public readonly ServiceLocator serviceLocator;
            public readonly MessagingContext messaging;
            public readonly PlatformContext platform;
            public readonly WorldRuntimeContext world;
            public readonly HUDContext hud;

            public Model (ServiceLocator serviceLocator)
            {
                this.serviceLocator = serviceLocator;
                messaging = new MessagingContext(serviceLocator);
                platform = new PlatformContext(serviceLocator);
                world = new WorldRuntimeContext(serviceLocator);
                hud = new HUDContext(serviceLocator);
            }

            public void Dispose()
            {
                this.serviceLocator.Dispose();
            }
        }
    }
}
