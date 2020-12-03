using System;

namespace Variables.RealmsInfo
{
    public class CurrentRealmVariable : BaseVariable<CurrentRealmModel>
    {
        public override bool Equals(CurrentRealmModel other)
        {
            if (value == null)
            {
                return other == null;
            }

            return value.Equals(other);
        }
    }

    [Serializable]
    public class CurrentRealmModel
    {
        public string layer;
        public string serverName;

        public bool Equals(CurrentRealmModel other)
        {
            if (other == null) return false;
            return Equals(other.serverName, other.layer);
        }

        public bool Equals(string serverName, string layer)
        {
            return this.serverName == serverName && this.layer == layer;
        }
    }
}