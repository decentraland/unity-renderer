using DCL;
using Decentraland.Bff;
using System.Collections.Generic;

public class RealmPluginTestsUtils
{
    private const string CATALYST_REALM_NAME = "CatalystRealmName";
    private const string WORLD_REALM_NAME = "WorldRealmName";

    public static void SetRealm(bool isWorld)
    {
        List<string> sceneUrn = new List<string>();

        if (isWorld)
            sceneUrn.Add("sceneUrn");

        DataStore.i.realm.playerRealmAboutConfiguration.Set(new AboutResponse.Types.AboutConfiguration()
        {
            RealmName = isWorld ? WORLD_REALM_NAME : CATALYST_REALM_NAME,
            Minimap = new AboutResponse.Types.MinimapConfiguration()
            {
                Enabled = !isWorld
            },
            ScenesUrn = { sceneUrn },
        });
    }

    public static AboutResponse.Types.AboutConfiguration GetAboutConfiguration(bool isWorld)
    {
        List<string> sceneUrn = new List<string>();

        if (isWorld)
            sceneUrn.Add("sceneUrn");

        return new AboutResponse.Types.AboutConfiguration()
        {
            RealmName = isWorld ? WORLD_REALM_NAME : CATALYST_REALM_NAME,
            Minimap = new AboutResponse.Types.MinimapConfiguration()
            {
                Enabled = !isWorld
            },
            ScenesUrn = { sceneUrn },
        };
    }
}
