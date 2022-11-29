using Decentraland.Bff;
using System;

namespace DCLPlugins.RealmPlugin
{
    public interface IRealmModifier : IDisposable
    {
        void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration);
    }
}
