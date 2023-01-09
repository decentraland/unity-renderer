using DCL.Configuration;
using UnityEngine;
using Sentry.Unity;

[CreateAssetMenu(fileName = "Assets/Resources/Sentry/SentryRuntimeOptionsConfiguration.asset", menuName = "Sentry/Assets/Resources/Sentry/SentryRuntimeOptionsConfiguration.asset", order = 999)]
public class SentryRuntimeOptionsConfiguration : Sentry.Unity.ScriptableOptionsConfiguration
{
    /// See base class for documentation.
    /// Learn more at https://docs.sentry.io/platforms/unity/configuration/options/#programmatic-configuration
    public override void Configure(SentryUnityOptions options)
    {
        if (EnvironmentSettings.RUNNING_TESTS) { options.Environment = "test-" + options.Environment; }
    }
}
