using System;

namespace Variables.RealmsInfo
{
    [Serializable]
    public class RealmModel
    {
        public string layer;
        public string serverName;
        public string url;
        public int usersCount;
        public int usersMax;
    }
}