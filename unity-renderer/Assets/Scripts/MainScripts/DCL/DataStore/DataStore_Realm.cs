using Decentraland.Bff;
using Variables.RealmsInfo;

namespace DCL
{
    public class DataStore_Realm
    {
        public readonly CurrentRealmVariable playerRealm = new CurrentRealmVariable();
        public readonly BaseCollection<RealmModel> realmsInfo = new BaseCollection<RealmModel>();
        public readonly BaseVariable<AboutResponse.Types.AboutConfiguration> playerRealmAboutConfiguration = new BaseVariable<AboutResponse.Types.AboutConfiguration>();
        public readonly BaseVariable<AboutResponse.Types.LambdasInfo> playerRealmAboutLambdas = new BaseVariable<AboutResponse.Types.LambdasInfo>();
        public readonly BaseVariable<AboutResponse.Types.ContentInfo> playerRealmAboutContent = new BaseVariable<AboutResponse.Types.ContentInfo>();
        public readonly BaseVariable<string> realmName = new BaseVariable<string>();
    }
}
