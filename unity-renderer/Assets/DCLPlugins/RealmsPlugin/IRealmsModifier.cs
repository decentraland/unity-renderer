using System;
using Decentraland.Bff;

namespace DCLPlugins.RealmsPlugin
{
    public interface IRealmsModifier : IDisposable
    {
        void OnEnteredRealm(bool isCatalyst, AboutResponse realmConfiguration);
    }
}
