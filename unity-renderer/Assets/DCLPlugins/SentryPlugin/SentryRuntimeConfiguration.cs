using UnityEngine;
using Sentry.Unity;

[CreateAssetMenu(fileName = "Assets/Resources/Sentry/SentryRuntimeConfiguration.asset", menuName = "Sentry/SentryRuntimeConfiguration", order = 999)]
public class SentryRuntimeConfiguration : Sentry.Unity.SentryRuntimeOptionsConfiguration
{
    /// Called at the player startup by SentryInitialization.
    /// You can alter configuration for the C# error handling and also
    /// native error handling in platforms **other** than iOS, macOS and Android.
    /// Learn more at https://docs.sentry.io/platforms/unity/configuration/options/#programmatic-configuration
    public override void Configure(SentryUnityOptions options)
    {
        // Note that changes to the options here will **not** affect iOS, macOS and Android events. (i.e. environment and release)
        // Take a look at `SentryBuildTimeOptionsConfiguration` instead.
        // TODO implement
    }
}
