namespace DCL.Bots
{
    public interface IBotsController
    {
        void InstantiateBotsAtWorldPos(WorldPosInstantiationConfig config);
        void InstantiateBotsAtCoords(CoordsInstantiationConfig config);
    }

    public class WorldPosInstantiationConfig
    {
        public int amount = 1;
        public float xPos;
        public float yPos;
        public float zPos;
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
        public float xCoord;
        public float yCoord;
        public float areaWidth = 0;
        public float areaDepth = 0;

        public override string ToString()
        {
            return $"amount: {amount}" +
                   $"\n xCoord: {xCoord}" +
                   $"\n yCoord: {yCoord}" +
                   $"\n areaWidth: {areaWidth}" +
                   $"\n areaDepth: {areaDepth}";
        }
    }
}