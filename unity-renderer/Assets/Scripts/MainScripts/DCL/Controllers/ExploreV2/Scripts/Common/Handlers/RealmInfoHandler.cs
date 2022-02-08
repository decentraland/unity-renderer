public class RealmInfoHandler : IRealmDataView
{
    public string serverName { private set; get; }

    public void SetRealmInfo(string serverName)
    {
        this.serverName = serverName;
    }

    public bool ContainRealm(string serverName)
    {
        return this.serverName == serverName;
    }
}