namespace WorldsFeaturesAnalytics
{
    public interface IWorldsAnalytics
    {
        void OnEnteredRealm(bool isWorld, string newRealmName);
    }
}
