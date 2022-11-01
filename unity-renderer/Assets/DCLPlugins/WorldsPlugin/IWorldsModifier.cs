using System;
using Decentraland.Bff;

public interface IWorldsModifier : IDisposable
{
    void EnteredRealm(bool isCatalyst, AboutResponse realmConfiguration);

}
