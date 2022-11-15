using Decentraland.Bff;
using Variables.RealmsInfo;

namespace DCL
{
    public class DataStore_Realm
    {
        public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        public readonly BaseCollection<RealmModel> realmsInfo = new BaseCollection<RealmModel>();
        public readonly BaseVariable<AboutResponse> playerRealmAbout = new BaseVariable<AboutResponse>();
        public readonly BaseVariable<string> realmName = new BaseVariable<string>();
    }
}