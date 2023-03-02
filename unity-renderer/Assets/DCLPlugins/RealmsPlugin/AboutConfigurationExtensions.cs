using Decentraland.Bff;
using System.Linq;

namespace DCLPlugins.RealmPlugin
{
    public static class AboutConfigurationExtensions
    {
        public static bool IsWorld(this AboutResponse.Types.AboutConfiguration aboutConfiguration) =>
            aboutConfiguration.ScenesUrn.Any() && string.IsNullOrEmpty(aboutConfiguration.CityLoaderContentServer);
    }
}
