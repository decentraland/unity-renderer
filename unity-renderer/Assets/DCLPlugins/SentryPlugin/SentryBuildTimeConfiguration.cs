using UnityEngine;
using Sentry.Unity;

[CreateAssetMenu(fileName = "Assets/Resources/Sentry/SentryBuildTimeConfiguration.asset", menuName = "Sentry/SentryBuildTimeConfiguration", order = 999)]
public class SentryBuildTimeConfiguration : Sentry.Unity.SentryBuildTimeOptionsConfiguration
{
    /// Called during app build. Changes made here will affect build-time processing, symbol upload, etc.
    /// Additionally, because iOS, macOS and Android native error handling is configured at build time,
    /// you can make changes to these options here.
    /// Learn more at https://docs.sentry.io/platforms/unity/configuration/options/#programmatic-configuration
    public override void Configure(SentryUnityOptions options, SentryCliOptions cliOptions)
    {
        // TODO implement
    }
}
