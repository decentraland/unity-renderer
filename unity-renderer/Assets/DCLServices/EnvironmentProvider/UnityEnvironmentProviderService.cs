using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DCLServices.EnvironmentProvider
{
    public class UnityEnvironmentProviderService : IEnvironmentProviderService
    {
        private readonly KernelConfig kernelConfig;

        public UnityEnvironmentProviderService(KernelConfig kernelConfig)
        {
            this.kernelConfig = kernelConfig;
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public bool IsProd()
        {
            string url = Application.absoluteURL;

            if (string.IsNullOrEmpty(url))
                return !Application.isEditor && !Debug.isDebugBuild && kernelConfig.Get().network == "mainnet";

            const string PATTERN = @"play\.decentraland\.([a-z0-9]+)\/";
            Match match = Regex.Match(url, PATTERN);

            if (match.Success)
                return match.Groups[1].Value.Equals("org");

            return !Application.isEditor && !Debug.isDebugBuild && kernelConfig.Get().network == "mainnet";
        }
    }
}
