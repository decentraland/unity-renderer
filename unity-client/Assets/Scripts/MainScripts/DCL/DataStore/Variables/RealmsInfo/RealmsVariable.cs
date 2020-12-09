using System;

namespace Variables.RealmsInfo
{
    public class RealmsVariable : BaseVariable<RealmModel[]>
    {
        public override bool Equals(RealmModel[] other)
        {
            return value == other;
        }
    }

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