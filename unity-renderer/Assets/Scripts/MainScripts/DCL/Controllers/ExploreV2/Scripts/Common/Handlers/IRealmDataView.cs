public interface IRealmDataView
{
    string serverName { get; }
    void SetRealmInfo(string serverName);
    bool ContainRealm(string serverName);
}