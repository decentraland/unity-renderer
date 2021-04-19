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
        public string domain = string.Empty;

        public bool Equals(CurrentRealmModel other)
        {
            if (other == null) return false;
            return Equals(other.serverName, other.layer, other.domain);
        }

        public bool Equals(string serverName, string layer, string domain)
        {
            return Equals(serverName, layer) && this.domain == domain;
        }
        
        public bool Equals(string serverName, string layer)
        {
            return this.serverName == serverName && this.layer == layer;
        }

        public CurrentRealmModel Clone()
        {
            return new CurrentRealmModel()
            {
                serverName = serverName,
                layer = layer,
                domain = domain
            };
        }
    }
}