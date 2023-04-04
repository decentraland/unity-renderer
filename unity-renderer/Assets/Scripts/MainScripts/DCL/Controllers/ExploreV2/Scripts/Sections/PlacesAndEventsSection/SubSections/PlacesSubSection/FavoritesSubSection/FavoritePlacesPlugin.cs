using DCL;

public class FavoritePlacesPlugin : IPlugin
{

    public FavoritePlacesPlugin()
    {
        DataStore.i.HUDs.enableFavoritePlaces.Set(true);
    }

    public void Dispose()
    {

    }
}
