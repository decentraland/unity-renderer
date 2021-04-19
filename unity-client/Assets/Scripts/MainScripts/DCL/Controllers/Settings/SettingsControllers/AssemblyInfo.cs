using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("SettingsControllersTest")]

// Recommended workaround for NSubstitute issue where Configure an internal method will still call the base class
// https://github.com/nsubstitute/NSubstitute/issues/496
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]