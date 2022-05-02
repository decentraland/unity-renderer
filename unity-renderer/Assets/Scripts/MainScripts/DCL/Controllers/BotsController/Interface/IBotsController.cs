using System.Collections;
using DCL.Configuration;

namespace DCL.Bots
{
    public interface IBotsController
    {
        IEnumerator InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config);
        IEnumerator InstantiateBotsAtCoords(CoordsInstantiationConfig config);
        void StartRandomMovement(CoordsRandomMovementConfig config);
        void StopRandomMovement();
        void ClearBots();
        void RemoveBot(long targetEntityId);
    }

    public class WorldPosInstantiationConfig
    {
        public int amount = 1;
        public float xPos = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float yPos = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float zPos = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float areaWidth = 0;
        public float areaDepth = 0;

        public override string ToString()
        {
            return $"amount: {amount}" +
                   $"\n xPos: {xPos}" +
                   $"\n yPos: {yPos}" +
                   $"\n zPos: {zPos}" +
                   $"\n areaWidth: {areaWidth}" +
                   $"\n areaDepth: {areaDepth}";
        }
    }

    public class CoordsInstantiationConfig
    {
        public int amount = 1;
        public float xCoord = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float yCoord = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float areaWidth = ParcelSettings.PARCEL_SIZE;
        public float areaDepth = ParcelSettings.PARCEL_SIZE;

        public override string ToString()
        {
            return $"amount: {amount}" +
                   $"\n xCoord: {xCoord}" +
                   $"\n yCoord: {yCoord}" +
                   $"\n areaWidth: {areaWidth}" +
                   $"\n areaDepth: {areaDepth}";
        }
    }

    public class CoordsRandomMovementConfig
    {
        public float populationNormalizedPercentage = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float waypointsUpdateTime = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float xCoord = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float yCoord = EnvironmentSettings.UNINITIALIZED_FLOAT;
        public float areaWidth = ParcelSettings.PARCEL_SIZE;
        public float areaDepth = ParcelSettings.PARCEL_SIZE;

        public override string ToString()
        {
            return $"populationNormalizedPercentage: {populationNormalizedPercentage}" +
                   $"waypointsUpdateTime: {waypointsUpdateTime}" +
                   $"\n xCoord: {xCoord}" +
                   $"\n yCoord: {yCoord}" +
                   $"\n areaWidth: {areaWidth}" +
                   $"\n areaDepth: {areaDepth}";
        }
    }
}