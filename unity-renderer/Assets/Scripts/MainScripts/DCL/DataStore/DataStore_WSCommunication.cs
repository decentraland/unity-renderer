namespace DCL
{
    public class DataStore_WSCommunication
    {
        [System.NonSerialized]
        public string url = "ws://localhost:5000/";

        public readonly BaseVariable<bool> communicationEstablished = new BaseVariable<bool>();
        public readonly BaseVariable<bool> communicationReady = new BaseVariable<bool>();
    }
}