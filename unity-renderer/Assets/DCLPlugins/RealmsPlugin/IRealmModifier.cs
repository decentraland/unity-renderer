using System;
using static Decentraland.Bff.AboutResponse.Types;

namespace DCLPlugins.RealmPlugin
{
    public interface IRealmModifier : IDisposable
    {
        void OnEnteredRealm(bool isWorld, AboutConfiguration realmConfiguration);
    }
}
