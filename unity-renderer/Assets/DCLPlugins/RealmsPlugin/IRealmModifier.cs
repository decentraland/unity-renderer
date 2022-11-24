using System;
using Decentraland.Bff;

namespace DCLPlugins.RealmPlugin
{
    public interface IRealmModifier : IDisposable
    {
        void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration);
    }
}
