using Decentraland.Bff;
using System;

namespace DCLPlugins.RealmPlugin
{
    public interface IRealmModifier : IDisposable
    {
        void OnEnteredRealm(bool isWorld, AboutResponse.Types.AboutConfiguration realmConfiguration);
    }
}
