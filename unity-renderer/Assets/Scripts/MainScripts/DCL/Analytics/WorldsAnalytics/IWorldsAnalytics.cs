namespace WorldsFeaturesAnalytics
{
    public interface IWorldsAnalytics
    {
        /*void SendPlayerEnteredWorld(string worldName, WorldAccessType accessType);

        void SendPlayerLeavesWorld(string worldName, double sessionTimeInSeconds, ExitSourceType exitSourceType);

        void SendPlayerUnableToAccessWorld(string worldName, UnableToAccessWorldReasonType unableToAccessReasonType);*/

        void OnEnteredRealm(bool isWorld, string newRealmName);
    }
}
