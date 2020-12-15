using Variables.RealmsInfo;

namespace DCL
{
    public static class DataStore
    {
        static public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        static public readonly RealmsVariable realmsInfo = new RealmsVariable();
        static public readonly DebugConfig debugConfig = new DebugConfig();
    }
}