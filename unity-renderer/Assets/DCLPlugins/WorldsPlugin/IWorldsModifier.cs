using System;
using Decentraland.Bff;

namespace DCLPlugins.WorldsPlugin
{
    public interface IWorldsModifier : IDisposable
    {
        void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration);

    }
}
